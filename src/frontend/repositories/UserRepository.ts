import { fetchJson, getApiErrorMessage } from '@/frontend/repositories/http';

export type UserProfileUpdate = {
  nome?: string;
  dataNascimento?: string;
};

export type UserProfileResponse = {
  id: string;
  nome: string;
  email: string;
  dataNascimento?: string | null;
  tipoUsuario: 'cadastrado' | 'administrador';
};

export async function atualizarPerfilUsuario(
  accessToken: string | null | undefined,
  payload: UserProfileUpdate,
) {
  if (!accessToken) {
    throw new Error('Entre novamente para editar o perfil.');
  }

  const { response, payload: responsePayload } = await fetchJson<UserProfileResponse>('/usuario/me', {
    method: 'PUT',
    headers: {
      Authorization: `Bearer ${accessToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
    operation: 'editar perfil',
  });

  if (!response.ok) {
    throw new Error(getApiErrorMessage(responsePayload, 'Nao foi possivel editar o perfil.'));
  }

  return responsePayload;
}

export async function obterPerfilUsuario(accessToken: string | null | undefined) {
  if (!accessToken) {
    throw new Error('Entre novamente para carregar o perfil.');
  }

  const { response, payload } = await fetchJson<UserProfileResponse>('/usuario/me', {
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
    operation: 'carregar perfil',
  });

  if (!response.ok) {
    throw new Error(getApiErrorMessage(payload, 'Nao foi possivel carregar o perfil.'));
  }

  return payload;
}

export async function promoverUsuarioAdministrador(
  accessToken: string | null | undefined,
  email: string,
) {
  if (!accessToken) {
    throw new Error('Entre novamente como administrador.');
  }

  const { response, payload } = await fetchJson<UserProfileResponse>('/usuario/admin/usuarios/promover', {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${accessToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ email }),
    operation: 'promover administrador',
  });

  if (!response.ok) {
    throw new Error(getApiErrorMessage(payload, 'Nao foi possivel promover o usuario.'));
  }

  return payload;
}
