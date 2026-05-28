import * as Location from 'expo-location';

export type LocalizacaoUsuario = {
  formattedAddress: string;
  latitude: number;
  longitude: number;
  placeId: string;
  provider: string;
};

const API_BASE_URL = process.env.EXPO_PUBLIC_API_BASE_URL ?? 'http://localhost:5000';

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
    `${API_BASE_URL}/geolocalizacao/api/geocoding/reverse?latitude=${latitude}&longitude=${longitude}`,
  );

  const payload = await response.json();

  if (!response.ok) {
    throw new Error(payload?.error ?? 'Nao foi possivel resolver sua localizacao.');
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

  const response = await fetch(`${API_BASE_URL}/usuario/api/me/localizacao`, {
    method: 'PUT',
    headers: {
      Authorization: `Bearer ${accessToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(localizacao),
  });

  const payload = await response.json();

  if (!response.ok) {
    throw new Error(payload?.error ?? 'Nao foi possivel salvar sua localizacao.');
  }
}
