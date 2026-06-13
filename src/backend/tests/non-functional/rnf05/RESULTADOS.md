# RNF05 — Registro de resultados

## Escopo implementado

A suíte cobre autenticação JWT, autorização por papel, transporte HTTPS, armazenamento de senha e varredura passiva de segurança.

## Matriz de evidências

| Controle | Evidência produzida | Critério |
|---|---|---|
| Token ausente/inválido/expirado | Newman JSON e JUnit | HTTP 401 |
| Usuário sem permissão | Newman JSON e JUnit | HTTP 403 |
| Token válido | Newman JSON e JUnit | Não retornar 401/403 |
| HTTPS obrigatório | `transport-results.json` | HTTP bloqueado ou redirecionado para HTTPS |
| Bcrypt | `password-storage-results.json` | 100% bcrypt, custo mínimo 10 |
| Texto plano | `password-storage-results.json` | 0 suspeitas |
| JWT no código | `static-security-results.json` | assinatura e expiração validadas |
| Segredos versionados | `static-security-results.json` | 0 ocorrências confirmadas |
| Vulnerabilidades web | relatórios ZAP | 0 alertas altos sem tratamento |

## Consolidação

O resultado final é produzido automaticamente por `scripts/generate-rnf05-report.ps1` a partir dos artefatos reais da execução. A atualização consolidada de `documentos/Projeto.md` será realizada ao final de todos os RNFs.
