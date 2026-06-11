import {
  listarEspetaculosSimilares,
  obterEspetaculoCatalogo,
  obterUrlMidia,
} from '../repositories/CatalogRepository';

export type EspetaculoSemelhante = {
  id: string;
  titulo: string;
  imagemUrl?: string;
};

export type EspetaculoDetalhe = {
  id: string;
  titulo: string;
  descricao: string;
  imagemUrl?: string;
  percentualAprovacao: number;
  notaMedia: number;
  tags: string[];
  linkCompra?: string;
};

export async function obterDetalhesEspetaculo(espetaculoId: string): Promise<EspetaculoDetalhe> {
  const detalhe = await obterEspetaculoCatalogo(espetaculoId);
  const primeiraSessao = detalhe.sessoes?.[0];
  const categorias = detalhe.categorias?.map((categoria) => categoria.nome) ?? [];
  const tags = deduplicar([
    detalhe.genero,
    primeiraSessao ? formatarDataSessao(primeiraSessao.data) : undefined,
    detalhe.duracao ? `${detalhe.duracao}min` : undefined,
    ...categorias.slice(0, 1),
  ].filter(Boolean) as string[]);

  return {
    id: String(detalhe.id),
    titulo: detalhe.titulo,
    descricao: detalhe.descricao ?? 'Descricao indisponivel para este espetaculo.',
    imagemUrl: obterUrlMidia(detalhe, 'detail_image'),
    percentualAprovacao: Math.round(Number(detalhe.notaMedia ?? 0) * 20),
    notaMedia: Number(detalhe.notaMedia ?? 0),
    tags,
    linkCompra: primeiraSessao?.linkCompra ?? undefined,
  };
}

export async function obterEspetaculosSemelhantes(
  espetaculoId: string,
): Promise<EspetaculoSemelhante[]> {
  if (!espetaculoId.trim()) return [];

  const similares = await listarEspetaculosSimilares(espetaculoId);

  return similares.map((item) => ({
    id: String(item.id),
    titulo: item.titulo,
    imagemUrl: obterUrlMidia(item, 'catalog_card'),
  }));
}

function formatarDataSessao(data: string) {
  const [, mes, dia] = data.split('-');

  if (!dia || !mes) return data;

  return `${dia}/${mes}`;
}

function deduplicar(valores: string[]) {
  return valores.filter((valor, index, todos) => todos.indexOf(valor) === index);
}
