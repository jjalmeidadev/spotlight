# RNF07 — Auditoria estática e estado

## Critério

100% dos logs emitidos em até 1 segundo e contendo timestamp, nível, serviço, sessão/correlation ID e dispositivo.

## Auditoria do código-base

Os serviços utilizam Serilog e `UseSerilogRequestLogging`, porém os arquivos `appsettings.json` analisados configuram somente nível mínimo. Não foi identificada configuração explícita de saída JSON nem enriquecimento padronizado com serviço, sessão/correlation ID e dispositivo.

Dessa forma, a presença do Serilog atende parcialmente à infraestrutura de logging, mas não comprova os cinco campos obrigatórios do RNF07.

## Implementação entregue

- geração de eventos positivos e negativos;
- envio de `X-Correlation-ID`, `X-Session-ID` e `X-Device-ID`;
- coleta dos logs dos quatro containers;
- verificador de campos obrigatórios;
- busca por possíveis segredos;
- relatório por evento.

## Situação

Validador implementado. A configuração atual tende a registrar uma não conformidade até que os cabeçalhos sejam propagados e os logs sejam enriquecidos de forma estruturada.
