const API_BASE_URL = process.env.EXPO_PUBLIC_API_BASE_URL ?? 'http://localhost:5000';
const CATALOG_IMAGE_BASE_URL = process.env.EXPO_PUBLIC_CATALOG_IMAGE_BASE_URL;

export type CatalogShow = {
  id: string;
  title: string;
  genre?: string | null;
  status: string;
  averageRating: number;
  popularity: number;
  imageUrl?: string | null;
};

export type CatalogShowDetail = CatalogShow & {
  description?: string | null;
  sessions: CatalogSession[];
};

export type CatalogSession = {
  id: number;
  date?: string;
  time?: string;
  minPrice?: number | null;
  maxPrice?: number | null;
  availability?: number;
  purchaseLink?: string | null;
  theater?: {
    id: number;
    name: string;
    city?: string | null;
    state?: string | null;
  };
};

type CatalogListEnvelope = {
  data?: CatalogListItem[];
};

type CatalogListItem = {
  id?: number | string;
  titulo?: string;
  genero?: string | null;
  status?: string;
  notaMedia?: number | string;
  popularidade?: number | string;
  imagemUrl?: string | null;
};

type CatalogDetailItem = CatalogListItem & {
  descricao?: string | null;
  sessoes?: Array<{
    id?: number;
    data?: string;
    horario?: string;
    precoMinimo?: number | string | null;
    precoMaximo?: number | string | null;
    disponibilidade?: number;
    linkCompra?: string | null;
    teatro?: {
      id?: number;
      nome?: string;
      cidade?: string | null;
      estado?: string | null;
    };
  }>;
};

export async function listarEspetaculosPopulares(limite = 8): Promise<CatalogShow[]> {
  const params = new URLSearchParams({
    ordenarPor: 'popularidade',
    status: 'em_cartaz',
    pageSize: String(limite),
  });

  const response = await fetch(`${API_BASE_URL}/catalogo/espetaculos?${params.toString()}`, {
    headers: { Accept: 'application/json' },
  });
  const payload = await parseJsonResponse<CatalogListEnvelope>(response);

  if (!response.ok) {
    throw new Error(getApiErrorMessage(payload, 'Nao foi possivel carregar o catalogo.'));
  }

  return (payload.data ?? [])
    .map(toCatalogShow)
    .filter((item): item is CatalogShow => item !== null);
}

export async function listarEspetaculosPertoDe(
  formattedAddress: string | null | undefined,
  limite = 8,
): Promise<CatalogShow[]> {
  const address = normalizeText(formattedAddress);
  if (!address) {
    return [];
  }

  const candidatos = await listarEspetaculosPopulares(Math.max(limite * 3, 12));
  const detalhes = await Promise.allSettled(
    candidatos.map((item) => obterDetalheEspetaculoCatalogo(item.id)),
  );

  return detalhes
    .flatMap((result) => (result.status === 'fulfilled' ? [result.value] : []))
    .filter((item) => item.sessions.some((session) => isSessionNearAddress(session, address)))
    .slice(0, limite);
}

export async function obterDetalheEspetaculoCatalogo(id: string): Promise<CatalogShowDetail> {
  const response = await fetch(`${API_BASE_URL}/catalogo/espetaculos/${encodeURIComponent(id)}`, {
    headers: { Accept: 'application/json' },
  });
  const payload = await parseJsonResponse<CatalogDetailItem>(response);

  if (!response.ok) {
    throw new Error(getApiErrorMessage(payload, 'Nao foi possivel carregar o espetaculo.'));
  }

  const show = toCatalogShow(payload);
  if (!show) {
    throw new Error('Resposta de catalogo sem espetaculo valido.');
  }

  return {
    ...show,
    description: payload.descricao,
    sessions: (payload.sessoes ?? []).map((session) => ({
      id: Number(session.id ?? 0),
      date: session.data,
      time: session.horario,
      minPrice: session.precoMinimo == null ? null : Number(session.precoMinimo),
      maxPrice: session.precoMaximo == null ? null : Number(session.precoMaximo),
      availability: session.disponibilidade,
      purchaseLink: session.linkCompra,
      theater: session.teatro
        ? {
          id: Number(session.teatro.id ?? 0),
          name: session.teatro.nome ?? '',
          city: session.teatro.cidade,
          state: session.teatro.estado,
        }
        : undefined,
    })),
  };
}

function toCatalogShow(item: CatalogListItem): CatalogShow | null {
  if (!item.id || !item.titulo) {
    return null;
  }

  const id = String(item.id);

  return {
    id,
    title: item.titulo,
    genre: item.genero,
    status: item.status ?? '',
    averageRating: Number(item.notaMedia ?? 0),
    popularity: Number(item.popularidade ?? 0),
    imageUrl: item.imagemUrl ?? buildStorageImageUrl(id),
  };
}

function buildStorageImageUrl(id: string) {
  if (!CATALOG_IMAGE_BASE_URL) {
    return null;
  }

  return `${CATALOG_IMAGE_BASE_URL.replace(/\/$/, '')}/${encodeURIComponent(id)}.png`;
}

function isSessionNearAddress(session: CatalogSession, address: string) {
  const city = normalizeText(session.theater?.city);
  const state = normalizeText(session.theater?.state);

  return Boolean((city && address.includes(city)) || (state && address.includes(state)));
}

function normalizeText(value: string | null | undefined) {
  return value
    ?.normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase()
    .trim();
}

function getApiErrorMessage(payload: any, fallback: string) {
  return payload?.error ?? payload?.message ?? fallback;
}

async function parseJsonResponse<T>(response: Response): Promise<T> {
  const text = await response.text();

  if (!text.trim()) {
    return {} as T;
  }

  return JSON.parse(text) as T;
}
