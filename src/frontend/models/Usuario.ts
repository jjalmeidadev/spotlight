export type Genero = 'Feminino' | 'Masculino' | 'Outro' | 'Prefiro nao informar';

export type Usuario = {
  id: string;
  nome: string;
  email: string;
  dataNascimento: string;
  genero: Genero;
  compartilharLocalizacao: boolean;
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

type SupabaseAuthResponse = {
  access_token?: string;
  refresh_token?: string;
  expires_in?: number;
  user?: {
    id: string;
    email?: string;
    user_metadata?: {
      nome?: string;
      dataNascimento?: string;
      genero?: Genero;
      compartilharLocalizacao?: boolean;
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

  const usuario = await requestSupabaseAuth('/auth/v1/signup', {
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

  const usuarioLogado = await loginUsuario({
    email: payload.email,
    senha: payload.senha,
  });

  return {
    ...usuarioLogado,
    nome: usuario.nome || usuarioLogado.nome,
    dataNascimento: usuario.dataNascimento || usuarioLogado.dataNascimento,
    genero: usuario.genero || usuarioLogado.genero,
    compartilharLocalizacao: payload.compartilharLocalizacao,
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

  const payload = await response.json();

  if (!response.ok) {
    throw new Error(getSupabaseErrorMessage(payload));
  }

  const auth = payload as SupabaseAuthResponse;
  const metadata = auth.user?.user_metadata;

  return {
    id: auth.user?.id ?? '',
    nome: metadata?.nome ?? auth.user?.email?.split('@')[0] ?? '',
    email: auth.user?.email ?? '',
    dataNascimento: metadata?.dataNascimento ?? '',
    genero: metadata?.genero ?? 'Prefiro nao informar',
    compartilharLocalizacao: metadata?.compartilharLocalizacao ?? false,
    accessToken: auth.access_token ?? null,
    refreshToken: auth.refresh_token ?? null,
  };
}

function getSupabaseErrorMessage(payload: any) {
  return payload?.msg
    ?? payload?.message
    ?? payload?.error_description
    ?? payload?.error
    ?? 'Nao foi possivel concluir a autenticacao.';
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
