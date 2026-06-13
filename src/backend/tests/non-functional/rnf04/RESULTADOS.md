# RNF04 — Achados e estado da auditoria

## Critério

100% dos elementos interativos devem possuir pelo menos 44×44 unidades de layout e funcionar em Android e iOS.

## Evidência estática já identificada

A inspeção do código-base encontrou alvos que exigem correção ou validação física:

| Local | Dimensão declarada | Avaliação estática |
|---|---:|---|
| `FilterDropdown.button` | `minHeight: 34` | Abaixo de 44 |
| `FilterDropdown.menuItem` | `minHeight: 30` | Abaixo de 44 |
| `CatalogHero.cta` | `minHeight: 28` | Abaixo de 44 |
| ação secundária em `DetalhesView` | `minHeight: 36` | Abaixo de 44 |
| botão de confirmação em `App.js` | `minHeight: 36` | Abaixo de 44 |
| opção em `PreferenceSelectionScreen` | `minHeight: 42` | Abaixo de 44 |

A constante `spacing.touchMin` está definida como 44 e é utilizada por componentes como `Chip` e diversos controles de detalhes, mas não garante conformidade integral da interface.

## Situação

Auditoria estática e checklist implementados. A validação física por toque em Android e iOS permanece necessária para o veredito final.
