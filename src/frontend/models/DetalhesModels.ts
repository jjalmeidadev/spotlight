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

const MOCK_DETALHES: Record<string, EspetaculoDetalhe> = {
  '16': {
    id: '16',
    titulo: 'A Noviça Rebelde',
    descricao:
      'Maria é uma jovem noviça que deixa o convento para ser governanta dos sete filhos do capitão Von Trapp. Com música e afeto, ela transforma a casa e ajuda a família a redescobrir o amor em meio às mudanças da Áustria nos anos 1930.',
    imagemUrl:
      'https://images.unsplash.com/photo-1503095396549-759542698bca?auto=format&fit=crop&w=1200&q=80',
    percentualAprovacao: 93,
    notaMedia: 3,
    tags: ['Espetáculo', '23/07', '60min'],
    linkCompra: 'https://bilheteria.aventura.com.br/novica-rebelde',
  },
};

const MOCK_SEMELHANTES: EspetaculoSemelhante[] = [
  {
    id: '3',
    titulo: 'Mamma Mia!',
    imagemUrl:
      'https://images.unsplash.com/photo-1514525253161-7a46d19cd819?auto=format&fit=crop&w=800&q=80',
  },
  {
    id: '3b',
    titulo: 'Mamma Mia!',
    imagemUrl:
      'https://images.unsplash.com/photo-1514525253161-7a46d19cd819?auto=format&fit=crop&w=800&q=80',
  },
];

function wait(ms = 450) {
  return new Promise((resolve) => {
    setTimeout(resolve, ms);
  });
}

export async function obterDetalhesEspetaculo(espetaculoId: string): Promise<EspetaculoDetalhe> {
  await wait();

  const detalhe = MOCK_DETALHES[espetaculoId] ?? MOCK_DETALHES['16'];

  if (!detalhe) {
    throw new Error('Não foi possível carregar os detalhes do espetáculo.');
  }

  return { ...detalhe, id: espetaculoId };
}

export async function obterEspetaculosSemelhantes(
  espetaculoId: string,
): Promise<EspetaculoSemelhante[]> {
  await wait(250);

  if (!espetaculoId.trim()) {
    return [];
  }

  return MOCK_SEMELHANTES;
}
