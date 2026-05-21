DROP TRIGGER IF EXISTS on_auth_user_created ON auth.users;
DROP FUNCTION IF EXISTS public.handle_new_user();

DROP TABLE IF EXISTS recomendacao         CASCADE;
DROP TABLE IF EXISTS avaliacao            CASCADE;

DROP TABLE IF EXISTS sessao               CASCADE;

DROP TABLE IF EXISTS espetaculo_ator      CASCADE;
DROP TABLE IF EXISTS espetaculo_categoria CASCADE;

DROP TABLE IF EXISTS espetaculo           CASCADE;

DROP TABLE IF EXISTS ator                 CASCADE;
DROP TABLE IF EXISTS categoria            CASCADE;
DROP TABLE IF EXISTS teatro               CASCADE;

DROP TABLE IF EXISTS usuario              CASCADE;

DROP TYPE IF EXISTS status_espetaculo_enum CASCADE;
DROP TYPE IF EXISTS tipo_usuario_enum      CASCADE;