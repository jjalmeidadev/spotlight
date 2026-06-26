# Manual de implantação - Spotlight

Este manual descreve o processo de implantação do Spotlight em uma instância
EC2 da AWS usando Docker Compose. O fluxo foi pensado para o AWS Academy
Learner Lab, onde serviços gerenciados como ECS/EKS podem ser limitados por
permissoes de IAM.

## Índice

- [1. Visão geral da estratégia](#1-visão-geral-da-estratégia)
- [2. Arquivos de deploy](#2-arquivos-de-deploy)
- [3. Pré-requisitos](#3-pré-requisitos)
- [4. Configurar credenciais temporárias da AWS](#4-configurar-credenciais-temporárias-da-aws)
- [5. Criar a key pair da EC2](#5-criar-a-key-pair-da-ec2)
- [6. Criar a stack CloudFormation](#6-criar-a-stack-cloudformation)
- [7. Validar a EC2](#7-validar-a-ec2)
- [8. Publicar a aplicação](#8-publicar-a-aplicação)
- [9. Liberar MongoDB Atlas](#9-liberar-mongodb-atlas)
- [10. Validar containers e endpoints](#10-validar-containers-e-endpoints)
- [11. Operação e manutenção](#11-operação-e-manutenção)
- [12. Problemas comuns](#12-problemas-comuns)
- [13. Resultado esperado](#13-resultado-esperado)
- [14. Observações de segurança](#14-observações-de-segurança)

## 1. Visão geral da estratégia

A infraestrutura e criada por CloudFormation, e a aplicação e executada na EC2
com Docker Compose.

Componentes criados ou usados:

| Camada | Recurso | Função |
|---|---|---|
| Infraestrutura | CloudFormation | Cria EC2, Security Group e bootstrap inicial |
| Computação | EC2 Amazon Linux 2023 | Hospeda os containers |
| Runtime | Docker + Docker Compose | Executa frontend, gateway e microsserviços |
| Entrada pública | Security Group porta 80 | Expoe o frontend/Nginx |
| Administracao | Security Group porta 22 | SSH restrito ao IP do operador |
| Banco relacional/Auth | Supabase | PostgreSQL, Auth e Storage |
| Telemetria | MongoDB Atlas | Eventos de interação do usuário |

O `docker-compose.prod.yml` publica apenas o serviço `frontend` na porta 80 da
EC2. O Nginx do frontend serve o build web e encaminha `/api/*` para o API
Gateway interno. Gateway e microsserviços não possuem portas públicas.

## 2. Arquivos de deploy

| Arquivo | Descricao |
|---|---|
| `deploy/aws/spotlight-ec2.yml` | Template CloudFormation da EC2, Security Group e bootstrap Docker |
| `deploy/aws/aws-runner.ps1` | Wrapper que usa AWS CLI nativa ou `amazon/aws-cli` via Docker |
| `deploy/aws/create-keypair.ps1` | Cria key pair EC2 e salva o `.pem` localmente |
| `deploy/aws/deploy-stack.ps1` | Cria ou atualiza a stack CloudFormation |
| `deploy/aws/publish-app.ps1` | Envia código e `.env` para a EC2 e executa Docker Compose |
| `deploy/nginx/default.conf` | Nginx do frontend, incluindo proxy `/api` para o gateway |
| `.env` | Variáveis de ambiente da aplicação. Nunca versionar |
| `.codex-tmp/aws-lab.ps1` | Credenciais temporarias do Learner Lab. Nunca versionar |

## 3. Pré-requisitos

Na maquina local:

- Docker Desktop iniciado.
- Git, SSH e SCP disponiveis no terminal.
- PowerShell com permissão para executar scripts usando
  `-ExecutionPolicy Bypass`.
- Credenciais temporarias do AWS Academy Learner Lab.
- `.env` preenchido com as variaveis reais da aplicação.

Na AWS:

- Learner Lab iniciado.
- Região definida, por padrão `us-east-1`.
- VPC e subnets padrão disponíveis no laboratório.
- Permissão para criar EC2, Security Group, Key Pair e CloudFormation stack.

Servicos externos:

- Supabase configurado com schema, dados e chaves corretas.
- MongoDB Atlas com o IP publico da EC2 liberado em Network Access.

## 4. Configurar credenciais temporárias da AWS

No Learner Lab, abrir **AWS Details** e copiar as credenciais CLI. Salvar em um
arquivo local ignorado pelo Git:

```powershell
New-Item -ItemType Directory -Force .codex-tmp
notepad .codex-tmp\aws-lab.ps1
```

Conteudo esperado:

```powershell
$env:AWS_ACCESS_KEY_ID="COLE_AQUI"
$env:AWS_SECRET_ACCESS_KEY="COLE_AQUI"
$env:AWS_SESSION_TOKEN="COLE_AQUI"
$env:AWS_DEFAULT_REGION="us-east-1"
```

O arquivo `.codex-tmp/aws-lab.ps1` e carregado automaticamente pelos scripts de
deploy. Ele não deve ser commitado.

## 5. Criar a key pair da EC2

Executar:

```powershell
powershell -ExecutionPolicy Bypass -File .\deploy\aws\create-keypair.ps1 -KeyName spotlight-ec2-key
```

Resultado esperado:

```text
Created key pair 'spotlight-ec2-key' and saved PEM to .codex-tmp\spotlight-ec2-key.pem
```

O arquivo `.pem` e usado para SSH/SCP. Ele tambem deve permanecer fora do Git.

## 6. Criar a stack CloudFormation

Executar:

```powershell
powershell -ExecutionPolicy Bypass -File .\deploy\aws\deploy-stack.ps1 -KeyName spotlight-ec2-key -InstanceType t3.small
```

O script:

1. Valida as credenciais com `sts get-caller-identity`.
2. Localiza VPC e subnet padrao.
3. Restringe SSH ao IP publico atual do operador.
4. Cria a stack `spotlight-ec2`.
5. Retorna `InstanceId`, `PublicIp`, `PublicUrl`, `ApiHealthUrl` e comando SSH.

Se `t3.small` não estiver disponivel no laboratório, tentar:

```powershell
powershell -ExecutionPolicy Bypass -File .\deploy\aws\deploy-stack.ps1 -KeyName spotlight-ec2-key -InstanceType t2.small
```

Recomendacao de tamanho:

- `t3.small`: mínimo recomendado.
- `t3.medium`: melhor para builds mais pesados, se permitido pelo laboratório.
- `t2.micro`: não recomendado, pode faltar memoria durante build dos containers.

## 7. Validar a EC2

Substituir `<PUBLIC_IP>` pelo IP retornado pela stack:

```powershell
ssh -i .\.codex-tmp\spotlight-ec2-key.pem ec2-user@<PUBLIC_IP>
```

Dentro da EC2, validar:

```bash
docker --version
docker compose version
ls -ld /opt/spotlight /opt/spotlight/app /opt/spotlight/releases
```

Se Docker ou Compose não estiverem instalados, consultar:

```bash
sudo tail -n 120 /var/log/spotlight-bootstrap.log
sudo tail -n 160 /var/log/cloud-init-output.log
```

## 8. Publicar a aplicação

Com a stack criada e o `.env` preenchido localmente:

```powershell
powershell -ExecutionPolicy Bypass -File .\deploy\aws\publish-app.ps1 -PublicIp <PUBLIC_IP> -KeyPath .\.codex-tmp\spotlight-ec2-key.pem -EnvFile .\.env
```

O script:

1. Gera um arquivo `.tar.gz` com o codigo versionado.
2. Envia o codigo para `/tmp/spotlight-source.tar.gz`.
3. Envia uma copia temporaria do `.env` para `/tmp/spotlight.env`.
4. Define `FRONTEND_PORT=80` para producao na EC2.
5. Cria uma release em `/opt/spotlight/releases/app-<timestamp>`.
6. Atualiza o symlink `/opt/spotlight/app`.
7. Executa:

```bash
docker compose -f docker-compose.prod.yml up -d --build
```

Importante: o `.env` fica somente na EC2, dentro da release ativa, com
permissao restrita.

## 9. Liberar MongoDB Atlas

Depois que a EC2 estiver criada, copiar o `PublicIp` da stack e adicionar no
MongoDB Atlas:

```text
<PUBLIC_IP>/32
```

Caminho no Atlas:

```text
Security > Network Access > Add IP Address
```

Sem essa liberação, o servico `usuario` pode ficar `unhealthy`, porque o
readiness dele valida conexão com MongoDB.

Depois de liberar o IP, reiniciar o servico:

```powershell
ssh -i .\.codex-tmp\spotlight-ec2-key.pem ec2-user@<PUBLIC_IP> "cd /opt/spotlight/app && docker compose -f docker-compose.prod.yml restart usuario"
```

## 10. Validar containers e endpoints

Na EC2:

```bash
cd /opt/spotlight/app
docker compose -f docker-compose.prod.yml ps
```

Estado esperado:

```text
frontend        healthy
gateway         healthy
catalogo        healthy
usuario         healthy
recomendacao    healthy
geolocalizacao  healthy
```

Validar URLs públicas:

```powershell
Invoke-WebRequest -UseBasicParsing http://<PUBLIC_IP>/ |
  Select-Object StatusCode

Invoke-WebRequest -UseBasicParsing http://<PUBLIC_IP>/api/health |
  Select-Object StatusCode,Content
```

Endpoints uteis:

| URL | Resultado esperado |
|---|---|
| `http://<PUBLIC_IP>/` | Frontend Spotlight |
| `http://<PUBLIC_IP>/api/health` | `Healthy` |
| `http://<PUBLIC_IP>/swagger` | Swagger UI unificado do gateway |
| `http://<PUBLIC_IP>/api/catalogo/health` | `Healthy` |
| `http://<PUBLIC_IP>/api/usuario/health` | `Healthy` |
| `http://<PUBLIC_IP>/api/usuario/health/ready` | `Healthy` |
| `http://<PUBLIC_IP>/api/recomendacao/health` | `Healthy` |
| `http://<PUBLIC_IP>/api/geolocalizacao/health` | `Healthy` |

## 11. Operação e manutenção

Ver logs:

```bash
cd /opt/spotlight/app
docker compose -f docker-compose.prod.yml logs -f frontend
docker compose -f docker-compose.prod.yml logs -f gateway
docker compose -f docker-compose.prod.yml logs -f usuario
```

Reiniciar serviço específico:

```bash
docker compose -f docker-compose.prod.yml restart usuario
```

Rebuild completo:

```bash
docker compose -f docker-compose.prod.yml up -d --build
```

Parar containers sem apagar imagens:

```bash
docker compose -f docker-compose.prod.yml stop
```

Desligar a EC2 para economizar creditos do Learner Lab:

```powershell
powershell -ExecutionPolicy Bypass -NoProfile -Command ". .\deploy\aws\aws-runner.ps1; Initialize-AwsRunner -WorkspaceRoot (Resolve-Path .); Invoke-AwsCli ec2 stop-instances --region us-east-1 --instance-ids <INSTANCE_ID>"
```

Remover a stack inteira:

```powershell
powershell -ExecutionPolicy Bypass -NoProfile -Command ". .\deploy\aws\aws-runner.ps1; Initialize-AwsRunner -WorkspaceRoot (Resolve-Path .); Invoke-AwsCli cloudformation delete-stack --region us-east-1 --stack-name spotlight-ec2"
```

## 12. Problemas comuns

| Sintoma | Causa provável | Ação |
|---|---|---|
| Stack falha criando EC2 | Tipo de instância indisponível na AZ | Usar outra subnet ou outro instance type |
| Docker não instalado apos criar EC2 | Falha no UserData/cloud-init | Ver logs em `/var/log/spotlight-bootstrap.log` |
| `usuario` unhealthy | MongoDB Atlas bloqueando IP ou erro na connection string | Liberar `<PUBLIC_IP>/32` no Atlas e reiniciar `usuario` |
| Frontend branco | Erro JavaScript no bundle ou cache antigo | Abrir DevTools Console, corrigir erro e fazer hard reload |
| Navegador continua usando build antigo | `index.html` cacheado | Hard reload ou limpar site data |
| `/api/*` retorna 502 | Gateway ou serviço interno fora do ar | Conferir `docker compose ps` e logs do gateway |
| Build falha por memória | Instância pequena demais | Usar `t3.small` ou `t3.medium` |

## 13. Resultado esperado

Ao final do processo:

- CloudFormation stack `spotlight-ec2` criada.
- EC2 rodando Amazon Linux 2023.
- Security Group com HTTP publico e SSH restrito.
- Docker e Docker Compose instalados.
- Aplicacao publicada em `http://<PUBLIC_IP>/`.
- Gateway disponivel em `http://<PUBLIC_IP>/api/health`.
- Swagger unificado disponivel em `http://<PUBLIC_IP>/swagger`.
- Todos os containers em estado `healthy`.

## 14. Observações de segurança

- Nunca versionar `.env`, `.pem` ou `.codex-tmp/aws-lab.ps1`.
- Não abrir portas internas dos microsserviços para a internet.
- Manter somente porta 80 pública enquanto não houver HTTPS.
- Restringir SSH ao IP do operador.
- Parar ou remover a EC2 quando o laboratório não estiver em uso.
