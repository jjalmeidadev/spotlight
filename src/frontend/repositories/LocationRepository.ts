import * as Location from 'expo-location';
import { getApiBaseUrl } from '@/frontend/config/api';

export type LocalizacaoUsuario = {
  formattedAddress: string;
  latitude: number;
  longitude: number;
  placeId: string;
  provider: string;
};

const API_BASE_URL = getApiBaseUrl();

export async function obterLocalizacaoAtual(): Promise<LocalizacaoUsuario> {
  const { status } = await Location.requestForegroundPermissionsAsync();

  if (status !== Location.PermissionStatus.GRANTED) {
    throw new Error('Permissao de localizacao negada.');
  }

  const position = await Location.getCurrentPositionAsync({
    accuracy: Location.Accuracy.Balanced,
  });

  const { latitude, longitude } = position.coords;
  const response = await fetch(
    `${API_BASE_URL}/geolocalizacao/geocoding/reverse?latitude=${latitude}&longitude=${longitude}`,
  );

  const payload = await parseJsonResponse(response);

  if (!response.ok) {
    throw new Error(getApiErrorMessage(payload, 'Nao foi possivel resolver sua localizacao.'));
  }

  return payload as LocalizacaoUsuario;
}

export async function salvarLocalizacaoUsuario(
  accessToken: string | null | undefined,
  localizacao: LocalizacaoUsuario,
) {
  if (!accessToken) {
    throw new Error('Token de acesso ausente para salvar localizacao.');
  }

  const response = await fetch(`${API_BASE_URL}/usuario/me/localizacao`, {
    method: 'PUT',
    headers: {
      Authorization: `Bearer ${accessToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(localizacao),
  });

  const payload = await parseJsonResponse(response);

  if (!response.ok) {
    throw new Error(getApiErrorMessage(payload, 'Nao foi possivel salvar sua localizacao.'));
  }
}

function getApiErrorMessage(payload: any, fallback: string) {
  const details = typeof payload?.details === 'string'
    ? ` (${payload.details})`
    : '';

  return `${payload?.error ?? fallback}${details}`;
}

async function parseJsonResponse(response: Response) {
  const text = await response.text();

  if (!text.trim()) {
    return {};
  }

  return JSON.parse(text);
}
