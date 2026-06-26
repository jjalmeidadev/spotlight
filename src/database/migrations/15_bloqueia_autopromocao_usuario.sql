-- 15_bloqueia_autopromocao_usuario.sql
-- Impede que usuarios comuns alterem diretamente public.usuario.tipo_usuario.
-- A promocao administrativa continua ocorrendo pelo backend/admin ou por service role.

CREATE OR REPLACE FUNCTION public.bloquear_autopromocao_usuario()
RETURNS TRIGGER
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = public
AS $$
BEGIN
    IF auth.uid() IS NOT NULL
       AND OLD.tipo_usuario IS DISTINCT FROM NEW.tipo_usuario
       AND NOT public.is_admin() THEN
        RAISE EXCEPTION 'Somente administradores podem alterar tipo_usuario.';
    END IF;

    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS trg_bloqueia_autopromocao_usuario ON public.usuario;

CREATE TRIGGER trg_bloqueia_autopromocao_usuario
    BEFORE UPDATE OF tipo_usuario ON public.usuario
    FOR EACH ROW
    EXECUTE FUNCTION public.bloquear_autopromocao_usuario();
