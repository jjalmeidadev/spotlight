-- RNF05: retorna somente métricas agregadas; nenhum hash é exposto.
WITH password_hashes AS (
    SELECT encrypted_password
    FROM auth.users
    WHERE encrypted_password IS NOT NULL
      AND encrypted_password <> ''
), classified AS (
    SELECT
        encrypted_password,
        encrypted_password ~ '^\$2[aby]\$[0-9]{2}\$' AS is_bcrypt,
        CASE
            WHEN encrypted_password ~ '^\$2[aby]\$[0-9]{2}\$'
            THEN substring(encrypted_password FROM '^\$2[aby]\$([0-9]{2})\$')::integer
            ELSE NULL
        END AS bcrypt_cost
    FROM password_hashes
)
SELECT
    COUNT(*) AS total_password_hashes,
    COUNT(*) FILTER (WHERE is_bcrypt) AS bcrypt_hashes,
    COUNT(*) FILTER (WHERE NOT is_bcrypt) AS non_bcrypt_hashes,
    COALESCE(MIN(bcrypt_cost), 0) AS minimum_bcrypt_cost,
    COUNT(*) FILTER (
        WHERE length(encrypted_password) < 40
           OR encrypted_password !~ '^\$'
    ) AS plaintext_suspicions
FROM classified;
