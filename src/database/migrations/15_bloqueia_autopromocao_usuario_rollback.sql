-- 15_bloqueia_autopromocao_usuario_rollback.sql

DROP TRIGGER IF EXISTS trg_bloqueia_autopromocao_usuario ON public.usuario;
DROP FUNCTION IF EXISTS public.bloquear_autopromocao_usuario();
