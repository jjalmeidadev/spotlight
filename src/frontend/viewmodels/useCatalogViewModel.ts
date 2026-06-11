import { useEffect, useMemo, useState } from 'react';
import { CatalogRail } from '../models/CatalogItem';
import {
	EspetaculoResumoApi,
	listarEspetaculosCatalogo,
	listarRecomendacoesUsuario,
	obterUrlMidia,
} from '../repositories/CatalogRepository';
import { CATALOG_IMAGE, normalizarTitulo } from '../config/catalogImages';

const FILTER_OPTIONS = ['Musical', 'Stand-up', 'Show', 'Espetaculo', 'Palestra', 'Meet-up'] as const;

export function useCatalogViewModel(accessToken?: string | null) {
	const [query, setQuery] = useState('');
	const [selectedFilter, setSelectedFilter] = useState<(typeof FILTER_OPTIONS)[number] | null>(null);
	const [espetaculos, setEspetaculos] = useState<EspetaculoResumoApi[]>([]);
	const [recomendacoes, setRecomendacoes] = useState<EspetaculoResumoApi[]>([]);
	const [loading, setLoading] = useState(true);
	const [error, setError] = useState<string | null>(null);

	useEffect(() => {
		let ativo = true;

		async function carregar() {
			try {
				setLoading(true);
				setError(null);

				const dados = await listarEspetaculosCatalogo();
				if (!ativo) return;
				setEspetaculos(dados);

				if (!accessToken) {
					setRecomendacoes([]);
					return;
				}

				try {
					const recomendacoesUsuario = await listarRecomendacoesUsuario(accessToken);
					if (ativo) setRecomendacoes(recomendacoesUsuario.map((item) => item.espetaculo));
				} catch (err) {
					console.log('Nao foi possivel carregar recomendacoes personalizadas:', err);
					if (ativo) setRecomendacoes([]);
				}
			} catch (err) {
				if (ativo) {
					setError(err instanceof Error ? err.message : 'Nao foi possivel carregar o catalogo.');
					setEspetaculos([]);
					setRecomendacoes([]);
				}
			} finally {
				if (ativo) setLoading(false);
			}
		}

		carregar();

		return () => {
			ativo = false;
		};
	}, [accessToken]);

	const filtrados = useMemo(() => {
		const termo = query.trim().toLocaleLowerCase('pt-BR');
		return filtrarEspetaculos(espetaculos, termo, selectedFilter);
	}, [espetaculos, query, selectedFilter]);

	const recomendacoesFiltradas = useMemo(() => {
		const termo = query.trim().toLocaleLowerCase('pt-BR');
		return filtrarEspetaculos(recomendacoes, termo, selectedFilter);
	}, [recomendacoes, query, selectedFilter]);

	const heroEspetaculo =
		espetaculos.find((item) => normalizarTitulo(item.titulo).includes('elis')) ?? espetaculos[0];
	const heroBackgroundUrl = heroEspetaculo
		? obterUrlMidia(heroEspetaculo, 'catalog_hero_background')
		: undefined;
	const heroArtworkUrl = heroEspetaculo
		? obterUrlMidia(heroEspetaculo, 'catalog_hero_artwork')
		: undefined;

	const assets = {
		...CATALOG_IMAGE,
		hero: heroBackgroundUrl ? { uri: heroBackgroundUrl } : undefined,
		heroArtwork: heroArtworkUrl ? { uri: heroArtworkUrl } : undefined,
	};

	const categoriaMaiorScore = recomendacoes[0]?.genero?.trim();
	const affinityItems = recomendacoesFiltradas.length > 0
		? recomendacoesFiltradas
		: filtrados.slice(10, 20);
	const affinityTitle = categoriaMaiorScore
		? `Porque voce gostou de ${categoriaMaiorScore}:`
		: 'Porque voce gostou de musicais:';

	const sections: CatalogRail[] = [
		{ id: 'popular', title: 'Os mais populares do momento:', items: filtrados.slice(0, 10).map(mapCatalogItem) },
		{ id: 'watched', title: 'Outras pessoas tambem assistiram:', items: filtrados.slice(5, 15).map(mapCatalogItem) },
		{ id: 'affinity', title: affinityTitle, items: affinityItems.map(mapCatalogItem) },
	];

	return {
		assets,
		error,
		filterOptions: FILTER_OPTIONS,
		heroEspetaculo,
		loading,
		query,
		selectedFilter,
		sections: sections.filter((section) => section.items.length > 0),
		setQuery,
		setSelectedFilter,
		userName: 'Leonardo',
	};
}

function filtrarEspetaculos(
	espetaculos: EspetaculoResumoApi[],
	termo: string,
	selectedFilter: (typeof FILTER_OPTIONS)[number] | null,
) {
	return espetaculos.filter((item) => {
		const combinaBusca = !termo || item.titulo.toLocaleLowerCase('pt-BR').includes(termo);
		const combinaFiltro =
			!selectedFilter ||
			item.genero?.toLocaleLowerCase('pt-BR').includes(selectedFilter.toLocaleLowerCase('pt-BR'));

		return combinaBusca && combinaFiltro;
	});
}

function mapCatalogItem(espetaculo: EspetaculoResumoApi) {
	const imageUrl = obterUrlMidia(espetaculo, 'catalog_card');

	return {
		id: String(espetaculo.id),
		title: espetaculo.titulo,
		subtitle: 'VER MAIS',
		image: imageUrl ? { uri: imageUrl } : undefined,
	};
}
