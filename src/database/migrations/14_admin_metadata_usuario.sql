-- 14_admin_metadata_usuario.sql
-- Permite criar usuarios administradores por fluxo administrativo do Supabase
-- usando raw_app_meta_data.tipoUsuario ou raw_app_meta_data.tipo_usuario.
-- Cadastros comuns continuam recebendo tipo_usuario = 'cadastrado'.

ALTER TABLE auth.users
    ADD COLUMN IF NOT EXISTS raw_app_meta_data JSONB NOT NULL DEFAULT '{}'::JSONB;

UPDATE auth.users au
SET raw_app_meta_data = jsonb_set(
    COALESCE(au.raw_app_meta_data, '{}'::JSONB),
    '{tipoUsuario}',
    '"administrador"'::JSONB,
    true
)
WHERE EXISTS (
    SELECT 1
    FROM public.usuario u
    WHERE u.id_usuario = au.id
      AND u.tipo_usuario = 'administrador'
);

CREATE OR REPLACE FUNCTION public.handle_new_user()
RETURNS TRIGGER
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = public
AS $$
DECLARE
    v_tipo_usuario TEXT;
BEGIN
    v_tipo_usuario := COALESCE(
        NEW.raw_app_meta_data->>'tipoUsuario',
        NEW.raw_app_meta_data->>'tipo_usuario',
        'cadastrado'
    );

    IF v_tipo_usuario NOT IN ('cadastrado', 'administrador') THEN
        v_tipo_usuario := 'cadastrado';
    END IF;

    INSERT INTO public.usuario (
        id_usuario,
        nome,
        email,
        data_nascimento,
        tipo_usuario
    )
    VALUES (
        NEW.id,
        COALESCE(NEW.raw_user_meta_data->>'nome', split_part(NEW.email, '@', 1)),
        NEW.email,
        NULLIF(NEW.raw_user_meta_data->>'dataNascimento', '')::DATE,
        v_tipo_usuario::tipo_usuario_enum
    );

    RETURN NEW;
END;
$$;
