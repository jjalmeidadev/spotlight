import { fetchJson, getApiErrorMessage } from '@/frontend/repositories/http';

const CATALOG_IMAGE_BASE_URL = process.env.EXPO_PUBLIC_CATALOG_IMAGE_BASE_URL;
const SUPABASE_URL = process.env.EXPO_PUBLIC_SUPABASE_URL ?? 'https://nyaxqgujyyvkjnqzbklf.supabase.co';
const CATALOG_TIMEOUT_MS = 22000;

export type CatalogShow = {
  id: string;
  title: string;
  genre?: string | null;
  status: string;
  ageRange?: string | null;
  contentRating?: string | null;
  duration?: number | null;
  producer?: string | null;
  averageRating: number;
  popularity: number;
  imageUrl?: string | null;
  media?: CatalogMedia[];
};

export type ListCatalogShowsOptions = {
  genre?: string | null;
  limit?: number;
  query?: string | null;
  sortBy?: 'popularidade' | 'nota_media' | 'titulo';
  status?: string | null;
};

export type CatalogShowDetail = CatalogShow & {
  description?: string | null;
  sessions: CatalogSession[];
};

export type CatalogMedia = {
  tipoMidia: 'catalog_card' | 'catalog_hero_background' | 'catalog_hero_artwork' | 'detail_image';
  storageBucket: string;
  storagePath: string;
  altText?: string | null;
  ordemExibicao: number;
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
  faixaEtaria?: string | null;
  classificacaoIndicativa?: string | null;
  duracao?: number | string | null;
  imagemUrl?: string | null;
  midias?: CatalogMedia[];
};

type CatalogDetailItem = CatalogListItem & {
  descricao?: string | null;
  produtora?: string | null;
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

export async function listarEspetaculos(options: ListCatalogShowsOptions = {}): Promise<CatalogShow[]> {
  const params = new URLSearchParams({
    ordenarPor: options.sortBy ?? 'popularidade',
    pageSize: String(options.limit ?? 8),
  });

  if (options.status) {
    params.set('status', options.status);
  }

  if (options.genre) {
    params.set('genero', options.genre);
  }

  if (options.query?.trim()) {
    params.set('q', options.query.trim());
  }

  const { response, payload } = await fetchJson<CatalogListEnvelope>(`/catalogo/espetaculos?${params.toString()}`, {
    headers: { Accept: 'application/json' },
    operation: 'carregar catalogo',
    retries: 1,
    timeoutMs: CATALOG_TIMEOUT_MS,
  });

  if (!response.ok) {
    throw new Error(getApiErrorMessage(payload, 'Nao foi possivel carregar o catalogo.'));
  }

  return (payload.data ?? [])
    .map(toCatalogShow)
    .filter((item): item is CatalogShow => item !== null);
}

export async function listarEspetaculosEmCartaz(limite = 12): Promise<CatalogShow[]> {
  return listarEspetaculos({ limit: limite, sortBy: 'titulo', status: 'em_cartaz' });
}

export async function listarCatalogoCompleto(limite = 20): Promise<CatalogShow[]> {
  return listarEspetaculos({ limit: limite, sortBy: 'popularidade' });
}

export async function listarEspetaculosPopulares(limite = 10): Promise<CatalogShow[]> {
  const itens = await listarEspetaculos({ limit: Math.max(limite * 2, 20), sortBy: 'popularidade' });
  return itens.slice(0, limite);
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
  const { response, payload } = await fetchJson<CatalogDetailItem>(`/catalogo/espetaculos/${encodeURIComponent(id)}`, {
    headers: { Accept: 'application/json' },
    operation: 'carregar detalhes do espetaculo',
    retries: 1,
    timeoutMs: CATALOG_TIMEOUT_MS,
  });

  if (!response.ok) {
    throw new Error(getApiErrorMessage(payload, 'Nao foi possivel carregar o espetaculo.'));
  }

  const show = toCatalogShow(payload);
  if (!show) {
    throw new Error('Resposta de catalogo sem espetaculo valido.');
  }

  return {
    ...show,
    imageUrl: obterUrlMidia(payload, 'detail_image') ?? buildKnownStorageImageUrl(payload.titulo ?? show.title, 'detail_image') ?? show.imageUrl,
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
    ageRange: item.faixaEtaria,
    contentRating: item.classificacaoIndicativa,
    duration: item.duracao == null ? null : Number(item.duracao),
    producer: (item as CatalogDetailItem).produtora ?? null,
    averageRating: Number(item.notaMedia ?? 0),
    popularity: Number(item.popularidade ?? 0),
    imageUrl: item.imagemUrl ?? obterUrlMidia(item, 'catalog_card') ?? obterUrlMidiaConhecida(item.titulo, 'catalog_card') ?? buildStorageImageUrl(id),
    media: item.midias ?? [],
  };
}

export function obterUrlMidia(
  item: { midias?: CatalogMedia[] },
  tipoMidia: CatalogMedia['tipoMidia'],
) {
  const media = item.midias
    ?.filter((entry) => entry.tipoMidia === tipoMidia)
    .sort((a, b) => a.ordemExibicao - b.ordemExibicao)[0];

  if (!media) {
    return null;
  }

  const encodedPath = normalizeStoragePath(media.storagePath)
    .split('/')
    .map((segment) => encodeURIComponent(segment))
    .join('/');

  return `${SUPABASE_URL.replace(/\/$/, '')}/storage/v1/object/public/${media.storageBucket}/${encodedPath}`;
}

function normalizeStoragePath(path: string) {
  return path
    .trim()
    .replace(/\s+(\.[a-z0-9]+)$/i, '$1')
    .replace(/__+/g, '_');
}

export function obterUrlMidiaConhecida(
  title: string | null | undefined,
  tipoMidia: CatalogMedia['tipoMidia'],
) {
  if (!title) {
    return null;
  }

  return buildKnownStorageImageUrl(title, tipoMidia);
}

function buildStorageImageUrl(id: string) {
  if (!CATALOG_IMAGE_BASE_URL) {
    return null;
  }

  return `${CATALOG_IMAGE_BASE_URL.replace(/\/$/, '')}/${encodeURIComponent(id)}.png`;
}

function buildKnownStorageImageUrl(
  title: string,
  tipoMidia: CatalogMedia['tipoMidia'],
) {
  const slug = TITLE_IMAGE_SLUGS[normalizeTitle(title)];
  if (!slug) {
    return null;
  }

  const fileNameByType: Record<CatalogMedia['tipoMidia'], string> = {
    catalog_card: `${slug}_catalog_card.png`,
    catalog_hero_background: `${slug}_catalog_hero_background.png`,
    catalog_hero_artwork: `${slug}_catalog_hero_artwork.png`,
    detail_image: `${slug}_detail_image.png`,
  };

  const encodedPath = `${tipoMidia}/${fileNameByType[tipoMidia]}`
    .split('/')
    .map((segment) => encodeURIComponent(segment))
    .join('/');

  return `${SUPABASE_URL.replace(/\/$/, '')}/storage/v1/object/public/espetaculos/${encodedPath}`;
}

function normalizeTitle(value: string) {
  return value
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase()
    .replace(/&/g, 'e')
    .replace(/[^a-z0-9]+/g, ' ')
    .trim();
}

const TITLE_IMAGE_SLUGS: Record<string, string> = {
  'elis a musical': 'elis',
  'a novica rebelde': 'rebelde',
  'mamma mia': 'mamma',
  zaquim: 'zaquim',
  'pixar in concert': 'pixar',
  'ayrton senna o musical': 'ayrton',
  'romeu julieta ao som de marisa monte': 'romeu',
  'musical hairspray': 'hairspray',
  'clube da esquina os sonhos nao envelhecem': 'clube',
  'nossa historia com chico buarque': 'nossa',
  'prima facie com debora falabella': 'prima',
  'a cor purpura o musical': 'purpura',
  'bibi uma vida em musical': 'bibi',
  hair: 'hair',
  'chacrinha o musical': 'chacrinha',
};

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
