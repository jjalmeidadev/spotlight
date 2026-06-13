# RNF05 — Segurança com JWT, bcrypt e HTTPS

## Objetivo

Validar autenticação e autorização JWT, rejeição de credenciais inválidas, transporte HTTPS, armazenamento de senhas com bcrypt e vulnerabilidades detectáveis pelo OWASP ZAP.

## Conteúdo

| Arquivo | Função |
|---|---|
| `postman/RNF05-Seguranca.postman_collection.json` | Cenários JWT 401, 403 e token válido |
| `postman/RNF05.postman_environment.example.json` | Ambiente sem segredos |
| `scripts/run-newman.ps1` | Executa a collection com Newman em Docker |
| `scripts/run-static-security-check.ps1` | Inspeciona JWT, autorização, HTTPS e segredos versionados |
| `scripts/run-transport-check.ps1` | Verifica HTTPS e bloqueio/redirecionamento de HTTP |
| `scripts/run-password-storage-check.ps1` | Consulta apenas métricas agregadas de hashes no Supabase |
| `scripts/run-zap.ps1` | Executa ZAP Baseline e, opcionalmente, API Scan autorizado |
| `scripts/generate-rnf05-report.ps1` | Consolida as evidências em Markdown |
| `scripts/run-rnf05.ps1` | Orquestra todas as frentes |
| `sql/verify-password-storage.sql` | Valida bcrypt e custo sem expor hashes |
| `config/zap-rules.conf` | Classificação inicial dos alertas passivos |

## Segredos

Nunca salve tokens ou credenciais nos arquivos. Use somente variáveis do processo:

```powershell
$env:RNF05_ACCESS_TOKEN = "token JWT válido"
$env:RNF05_COMMON_USER_TOKEN = "token de usuário cadastrado sem papel admin"
$env:RNF05_DATABASE_URL = "postgresql://usuario:senha@host:5432/postgres?sslmode=require"
```

## Execução rápida

Somente JWT negativo e inspeção estática local:

```powershell
powershell -ExecutionPolicy Bypass -File src/backend/tests/non-functional/rnf05/scripts/run-rnf05.ps1 `
  -SkipTransport -SkipPasswordStorage -SkipZap
```

Execução completa em ambiente autorizado:

```powershell
powershell -ExecutionPolicy Bypass -File src/backend/tests/non-functional/rnf05/scripts/run-rnf05.ps1 `
  -HttpsUrl "https://URL-DE-HOMOLOGACAO/health" `
  -HttpUrl "http://URL-DE-HOMOLOGACAO/health" `
  -ZapTargetUrl "https://URL-DE-HOMOLOGACAO/swagger/index.html" `
  -OpenApiUrl "https://URL-DE-HOMOLOGACAO/swagger/v1/swagger.json"
```

O ZAP Baseline é passivo. O API Scan ativo só é executado quando `-RunActiveApiScan` é informado e deve ser usado exclusivamente contra ambiente autorizado.

## Critérios de aceitação

- token ausente, malformado, adulterado ou expirado retorna HTTP 401;
- usuário autenticado sem papel necessário retorna HTTP 403;
- token válido não é rejeitado por autenticação;
- HTTP é bloqueado ou redirecionado para HTTPS;
- 100% dos hashes analisados usam bcrypt;
- custo mínimo do bcrypt é 10;
- nenhuma suspeita de senha em texto plano;
- nenhum alerta ZAP de severidade alta permanece sem tratamento;
- nenhum segredo é incluído nas evidências.

## Evidências

Os artefatos são gerados em `rnf05/.results/` e devem permanecer fora do versionamento até a revisão. O relatório consolidado é criado como `RNF05-RELATORIO.md` dentro da execução correspondente.
