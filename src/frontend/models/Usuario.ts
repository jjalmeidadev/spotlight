export type Genero = 'Feminino' | 'Masculino' | 'Outro' | 'Prefiro não informar';

export type TipoUsuario = 'cadastrado' | 'administrador';

export type Usuario = {
  id: string;
  nome: string;
  email: string;
  idade: number;
  genero: Genero;
  compartilharLocalizacao: boolean;
  tipoUsuario: TipoUsuario;
};

export type LoginPayload = {
  email: string;
  senha: string;
};

export type CadastroPayload = {
  nome: string;
  email: string;
  senha: string;
  idade: string;
  genero: Genero;
  compartilharLocalizacao: boolean;
};

const MOCK_USER: Usuario = {
  id: '1',
  nome: 'Mariana',
  email: 'mariana@aventura.app',
  idade: 24,
  genero: 'Feminino',
  compartilharLocalizacao: true,
  tipoUsuario: 'cadastrado',
};

function wait(ms = 450) {
  return new Promise((resolve) => {
    setTimeout(resolve, ms);
  });
}

export async function loginUsuario(payload: LoginPayload): Promise<Usuario> {
  await wait();

  if (!payload.email.trim() || !payload.senha.trim()) {
    throw new Error('Preencha e-mail e senha para entrar.');
  }

  const email = payload.email.trim();
  const isAdmin = email.toLowerCase().includes('admin');

  return {
    ...MOCK_USER,
    nome: isAdmin ? 'Admin' : MOCK_USER.nome,
    email,
    tipoUsuario: isAdmin ? 'administrador' : 'cadastrado',
  };
}

export async function cadastrarUsuario(payload: CadastroPayload): Promise<Usuario> {
  await wait();

  if (!payload.nome.trim() || !payload.email.trim() || !payload.senha.trim()) {
    throw new Error('Preencha nome, e-mail e senha.');
  }

  const idade = Number(payload.idade);

  if (!Number.isFinite(idade) || idade <= 0) {
    throw new Error('Informe uma idade válida.');
  }

  return {
    id: String(Date.now()),
    nome: payload.nome.trim(),
    email: payload.email.trim(),
    idade,
    genero: payload.genero,
    compartilharLocalizacao: payload.compartilharLocalizacao,
    tipoUsuario: 'cadastrado',
  };
}
