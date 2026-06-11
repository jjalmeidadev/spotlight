import { getApiBaseUrl } from '@/frontend/config/api';

const API_BASE_URL = getApiBaseUrl();
const SUPABASE_URL = process.env.EXPO_PUBLIC_SUPABASE_URL ?? 'https://nyaxqgujyyvkjnqzbklf.supabase.co';

export type EspetaculoMidiaApi = {
  tipoMidia: 'catalog_card' | 'catalog_hero_background' | 'catalog_hero_artwork' | 'detail_image';
  storageBucket: string;
  storagePath: string;
  altText?: string | null;
  ordemExibicao: number;
};

export type EspetaculoResumoApi = {
  id: number;
  titulo: string;
  genero?: string | null;
  status: string;
  faixaEtaria?: string | null;
  classificacaoIndicativa?: string | null;
  duracao?: number | null;
  notaMedia: number;
  popularidade: number;
  midias?: EspetaculoMidiaApi[];
};

export type SessaoApi = {
  id: number;
  data: string;
  horario: string;
  precoMinimo?: number | null;
  precoMaximo?: number | null;
  disponibilidade: number;
  linkCompra?: string | null;
};

export type EspetaculoDetalheApi = EspetaculoResumoApi & {
  descricao?: string | null;
  produtora?: string | null;
  categorias?: Array<{ id: number; nome: string }>;
  sessoes?: SessaoApi[];
};

type ListaPaginada<T> = {
  data: T[];
};

type Lista<T> = {
  data: T[];
};

type SimilarApi = {
  espetaculo: EspetaculoResumoApi;
  score: number;
  tipoAlgoritmo: string;
};

export type RecomendacaoUsuarioApi = {
  espetaculo: EspetaculoResumoApi;
  score: number;
  tipoAlgoritmo: string;
  motivo?: string | null;
  dataGeracao: string;
};

export async function listarEspetaculosCatalogo(): Promise<EspetaculoResumoApi[]> {
  const url = `${API_BASE_URL}/catalogo/espetaculos?pageSize=50&ordenarPor=popularidade`;
  const payload = await fetchJson<ListaPaginada<EspetaculoResumoApi>>(url);
  return payload.data ?? [];
}

export async function obterEspetaculoCatalogo(id: string): Promise<EspetaculoDetalheApi> {
  return fetchJson<EspetaculoDetalheApi>(`${API_BASE_URL}/catalogo/espetaculos/${id}`);
}

export async function listarEspetaculosSimilares(id: string): Promise<EspetaculoResumoApi[]> {
  const payload = await fetchJson<Lista<SimilarApi>>(
    `${API_BASE_URL}/recomendacao/espetaculos/${id}/similares?limite=10`,
  );

  return (payload.data ?? []).map((item) => item.espetaculo);
}

export async function listarRecomendacoesUsuario(
  accessToken: string,
  limite = 10,
): Promise<RecomendacaoUsuarioApi[]> {
  const payload = await fetchJson<Lista<RecomendacaoUsuarioApi>>(
    `${API_BASE_URL}/recomendacao/me?limite=${limite}`,
    {
      headers: {
        Authorization: `Bearer ${accessToken}`,
      },
    },
  );

  return payload.data ?? [];
}

export function obterUrlMidia(
  espetaculo: { midias?: EspetaculoMidiaApi[] },
  tipoMidia: EspetaculoMidiaApi['tipoMidia'],
) {
  const midia = espetaculo.midias
    ?.filter((item) => item.tipoMidia === tipoMidia)
    .sort((a, b) => a.ordemExibicao - b.ordemExibicao)[0];

  if (!midia) return undefined;

  const encodedPath = normalizarStoragePath(midia.storagePath)
    .split('/')
    .map((segment) => encodeURIComponent(segment))
    .join('/');

  return `${SUPABASE_URL}/storage/v1/object/public/${midia.storageBucket}/${encodedPath}`;
}

function normalizarStoragePath(storagePath: string) {
  return storagePath.trim();
}

async function fetchJson<T>(url: string, init?: RequestInit): Promise<T> {
  const response = await fetch(url, init);
  const text = await response.text();
  const payload = text.trim() ? JSON.parse(text) : {};

  if (!response.ok) {
    throw new Error(payload?.error ?? 'Nao foi possivel carregar os dados do catalogo.');
  }

  return payload as T;
}
