-- 14_admin_metadata_usuario_rollback.sql
-- Restaura public.handle_new_user() para sempre criar usuarios cadastrados.

CREATE OR REPLACE FUNCTION public.handle_new_user()
RETURNS TRIGGER
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = public
AS $$
BEGIN
    INSERT INTO public.usuario (
        id_usuario,
        nome,
        email,
        data_nascimento
    )
    VALUES (
        NEW.id,
        COALESCE(NEW.raw_user_meta_data->>'nome', split_part(NEW.email, '@', 1)),
        NEW.email,
        NULLIF(NEW.raw_user_meta_data->>'dataNascimento', '')::DATE
    );

    RETURN NEW;
END;
$$;
