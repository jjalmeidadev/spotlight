export type Genero = 'Feminino' | 'Masculino' | 'Outro' | 'Prefiro nao informar';

export type TipoUsuario = 'cadastrado' | 'administrador';

export type Usuario = {
  id: string;
  nome: string;
  email: string;
  dataNascimento: string;
  genero: Genero;
  compartilharLocalizacao: boolean;
  tipoUsuario: TipoUsuario;
  requerConfirmacaoEmail?: boolean;
  localizacao?: {
    formattedAddress: string;
    latitude: number;
    longitude: number;
    placeId: string;
    provider: string;
  };
  accessToken?: string | null;
  refreshToken?: string | null;
};

export type LoginPayload = {
  email: string;
  senha: string;
};

export type CadastroPayload = {
  nome: string;
  email: string;
  senha: string;
  dataNascimento: string;
  genero: Genero;
  compartilharLocalizacao: boolean;
};

const SUPABASE_URL = process.env.EXPO_PUBLIC_SUPABASE_URL ?? 'https://nyaxqgujyyvkjnqzbklf.supabase.co';
const SUPABASE_ANON_KEY = process.env.EXPO_PUBLIC_SUPABASE_ANON_KEY ?? '';
const AUTH_REDIRECT_URL = process.env.EXPO_PUBLIC_AUTH_REDIRECT_URL ?? 'spotlightmobile://email-confirmado';

type SupabaseAuthResponse = {
  access_token?: string;
  refresh_token?: string;
  expires_in?: number;
  user?: {
    id: string;
    email?: string;
    app_metadata?: {
      tipoUsuario?: TipoUsuario;
      tipo_usuario?: TipoUsuario;
    };
    user_metadata?: {
      nome?: string;
      dataNascimento?: string;
      genero?: Genero;
      compartilharLocalizacao?: boolean;
      tipoUsuario?: TipoUsuario;
    };
  };
};

export async function loginUsuario(payload: LoginPayload): Promise<Usuario> {
  if (!payload.email.trim() || !payload.senha.trim()) {
    throw new Error('Preencha e-mail e senha para entrar.');
  }

  return requestSupabaseAuth('/auth/v1/token?grant_type=password', {
    email: payload.email.trim(),
    password: payload.senha,
  });
}

export async function cadastrarUsuario(payload: CadastroPayload): Promise<Usuario> {
  if (!payload.nome.trim() || !payload.email.trim() || !payload.senha.trim()) {
    throw new Error('Preencha nome, e-mail e senha.');
  }

  const dataNascimento = toIsoDate(payload.dataNascimento);

  if (!dataNascimento) {
    throw new Error('Informe uma data de nascimento valida no formato DD/MM/AAAA.');
  }

  const usuario = await requestSupabaseAuth(`/auth/v1/signup?redirect_to=${encodeURIComponent(AUTH_REDIRECT_URL)}`, {
    email: payload.email.trim(),
    password: payload.senha,
    data: {
      nome: payload.nome.trim(),
      dataNascimento,
      genero: payload.genero,
      compartilharLocalizacao: payload.compartilharLocalizacao,
    },
  });

  if (usuario.accessToken) {
    return usuario;
  }

  return {
    ...usuario,
    nome: usuario.nome || payload.nome.trim(),
    email: usuario.email || payload.email.trim(),
    dataNascimento: usuario.dataNascimento || dataNascimento,
    genero: usuario.genero || payload.genero,
    compartilharLocalizacao: payload.compartilharLocalizacao,
    tipoUsuario: usuario.tipoUsuario ?? 'cadastrado',
    requerConfirmacaoEmail: true,
  };
}

async function requestSupabaseAuth(path: string, body: unknown): Promise<Usuario> {
  if (!SUPABASE_ANON_KEY) {
    throw new Error('Configure EXPO_PUBLIC_SUPABASE_ANON_KEY para autenticar.');
  }

  const response = await fetch(`${SUPABASE_URL}${path}`, {
    method: 'POST',
    headers: {
      apikey: SUPABASE_ANON_KEY,
      Authorization: `Bearer ${SUPABASE_ANON_KEY}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(body),
  });

  const payload = await parseJsonResponse(response);

  if (!response.ok) {
    throw new Error(getSupabaseErrorMessage(payload));
  }

  const auth = payload as SupabaseAuthResponse;
  const metadata = auth.user?.user_metadata;
  const appMetadata = auth.user?.app_metadata;
  const email = auth.user?.email ?? '';
  const tipoUsuario = normalizeTipoUsuario(
    appMetadata?.tipoUsuario
      ?? appMetadata?.tipo_usuario
      ?? metadata?.tipoUsuario,
  );

  return {
    id: auth.user?.id ?? '',
    nome: metadata?.nome ?? email.split('@')[0] ?? '',
    email,
    dataNascimento: metadata?.dataNascimento ?? '',
    genero: metadata?.genero ?? 'Prefiro nao informar',
    compartilharLocalizacao: metadata?.compartilharLocalizacao ?? false,
    tipoUsuario,
    accessToken: auth.access_token ?? null,
    refreshToken: auth.refresh_token ?? null,
  };
}

function normalizeTipoUsuario(value?: string): TipoUsuario {
  return value === 'administrador' ? 'administrador' : 'cadastrado';
}

function getSupabaseErrorMessage(payload: any) {
  const message = payload?.msg
    ?? payload?.message
    ?? payload?.error_description
    ?? payload?.error
    ?? 'Nao foi possivel concluir a autenticacao.';

  return /email not confirmed/i.test(message)
    ? 'Confirme seu e-mail antes de entrar.'
    : message;
}

async function parseJsonResponse(response: Response) {
  const text = await response.text();

  if (!text.trim()) {
    return {};
  }

  return JSON.parse(text);
}

function toIsoDate(value: string) {
  const trimmed = value.trim();
  const match = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(trimmed);
  if (!match) {
    return null;
  }

  const [, day, month, year] = match;
  const isoDate = `${year}-${month}-${day}`;
  const date = new Date(`${isoDate}T00:00:00Z`);

  return !Number.isNaN(date.getTime()) && date.toISOString().startsWith(isoDate)
    ? isoDate
    : null;
}
