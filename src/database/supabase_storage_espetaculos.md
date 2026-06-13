# Supabase Storage - Midias de Espetaculos

Este documento registra a convencao usada para armazenar e resolver imagens reais
do catalogo via Supabase Storage.

## Bucket

- Nome: `espetaculos`
- Acesso de leitura: publico
- Escrita/alteracao/remocao: restrita a usuarios administradores pela policy de
  Storage criada em `migrations/09_espetaculo_midia.sql`

## Estrutura de paths

Os metadados ficam em `public.espetaculo_midia` e apontam para objetos no bucket.
O backend retorna apenas `storageBucket`, `storagePath`, `tipoMidia`, `altText` e
`ordemExibicao`; o frontend monta a URL publica a partir desses campos.

Convencao inicial aplicada:

```text
<tipo_midia>/<slug-ou-prefixo-do-espetaculo>_<tipo_midia>.png
```

Exemplo:

```text
catalog_card/elis_catalog_card.png
catalog_hero_background/elis_catalog_hero_background.png
catalog_hero_artwork/elis_catalog_hero_artwork .png
detail_image/elis__detail_image.png
```

Alguns arquivos iniciais preservam nomes enviados ao bucket com espacos ou
marcadores duplicados. Nesses casos, o `storage_path` registrado no banco deve ser
exatamente igual ao nome do objeto no Supabase Storage.

Observacao: o DoD original mencionava estrutura por `id_espetaculo`. O estado
implementado organiza por tipo de midia e correlaciona o espetaculo via
`public.espetaculo_midia.id_espetaculo`, que e o ponto de verdade consumido pelo
backend.

## Tipos de midia

Os tipos aceitos pela constraint `chk_espetaculo_midia_tipo` sao:

- `catalog_card`: imagem final do card no catalogo, ja contendo titulo e chamada
  visual embutidos no asset.
- `catalog_hero_background`: fundo do hero do catalogo.
- `catalog_hero_artwork`: recorte/personagem destacado sobre o hero.
- `detail_image`: imagem principal da tela de detalhes.

Cada espetaculo deve ter pelo menos uma imagem de cada tipo para a experiencia
visual completa do catalogo e dos detalhes.

## Ordem de aplicacao

Para ambientes incrementais, aplique nesta ordem:

```text
00_supabase_compat.sql
01_create_database.sql
02_initial_data.sql
migrations/04_trigger_updated_at_espetaculo.sql
migrations/05_timestamptz.sql
migrations/06_indexes.sql
migrations/07_constraints_sessao_usuario.sql
migrations/08_tipo_algoritmo_e_unique_recomendacao.sql
migrations/09_espetaculo_midia.sql
migrations/10_seed_catalogo_incremental.sql
migrations/11_seed_recomendacao_incremental.sql
```

Arquivos `*_rollback.sql` sao scripts manuais de reversao e nao devem ser
executados por aplicadores automaticos de migrations.

## Validacao rapida

```sql
SELECT tipo_midia, COUNT(*)
FROM public.espetaculo_midia
GROUP BY tipo_midia
ORDER BY tipo_midia;

SELECT COUNT(*) AS total_midias
FROM public.espetaculo_midia;
```

Resultado esperado para o seed inicial atual:

- 15 registros por tipo de midia.
- 60 registros no total.

## Fluxo backend/frontend

1. A API de catalogo carrega as midias relacionadas em `public.espetaculo_midia`.
2. A API de recomendacao retorna `catalog_card` para recomendacoes e similares.
3. O frontend consome os metadados pela API e resolve URLs publicas do Supabase.
4. Assets locais antigos de `catalog_card`, `catalog_hero_background`,
   `catalog_hero_artwork` e `detail_image` nao devem ser usados como fonte do
   catalogo.
