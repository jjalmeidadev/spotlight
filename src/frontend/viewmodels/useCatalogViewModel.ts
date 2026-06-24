import { useEffect, useMemo, useState } from 'react';
import { CatalogItem, CatalogRail } from '../models/CatalogItem';
import {
	CatalogShow,
	listarCatalogoCompleto,
	listarEspetaculos,
	listarEspetaculosEmCartaz,
	listarEspetaculosPertoDe,
	obterUrlMidiaConhecida,
	obterUrlMidia,
} from '../repositories/CatalogRepository';
import {
	listarEspetaculosSimilares,
	listarRecomendacoesUsuario,
	RecommendedShow,
	SimilarShow,
} from '../repositories/RecommendationRepository';
import { CATALOG_IMAGE, normalizarTitulo } from '../config/catalogImages';

const FILTER_OPTIONS = [
	'Teatro Musical',
	'Dramaturgia',
	'Show Musical',
	'Concerto e Show',
	'Teatro Infantil',
] as const;

type UseCatalogViewModelOptions = {
	accessToken?: string | null;
	userLocationAddress?: string | null;
};

export function useCatalogViewModel(options?: UseCatalogViewModelOptions) {
	const [query, setQuery] = useState('');
	const [selectedFilter, setSelectedFilter] = useState<(typeof FILTER_OPTIONS)[number] | null>(null);
	const [recommendations, setRecommendations] = useState<RecommendedShow[]>([]);
	const [popularShows, setPopularShows] = useState<CatalogShow[]>([]);
	const [nearbyShows, setNearbyShows] = useState<CatalogShow[]>([]);
	const [emCartazShows, setEmCartazShows] = useState<CatalogShow[]>([]);
	const [catalogShows, setCatalogShows] = useState<CatalogShow[]>([]);
	const [filteredShows, setFilteredShows] = useState<CatalogShow[]>([]);
	const [catalogLoading, setCatalogLoading] = useState(false);
	const [catalogError, setCatalogError] = useState<string | null>(null);
	const [catalogReloadKey, setCatalogReloadKey] = useState(0);
	const [recommendationsLoading, setRecommendationsLoading] = useState(false);
	const [recommendationsError, setRecommendationsError] = useState<string | null>(null);
	const [similarShows, setSimilarShows] = useState<SimilarShow[]>([]);

	useEffect(() => {
		let active = true;

		async function loadRecommendations() {
			if (!options?.accessToken) {
				setRecommendations([]);
				setRecommendationsError(null);
				return;
			}

			try {
				setRecommendationsLoading(true);
				setRecommendationsError(null);

				const data = await listarRecomendacoesUsuario(options.accessToken, 8);
				if (active) {
					setRecommendations(data);
				}
			} catch (error) {
				if (active) {
					setRecommendations([]);
					setRecommendationsError(
						error instanceof Error ? error.message : 'Não foi possível carregar recomendações.',
					);
				}
			} finally {
				if (active) {
					setRecommendationsLoading(false);
				}
			}
		}

		loadRecommendations();

		return () => {
			active = false;
		};
	}, [options?.accessToken]);

	useEffect(() => {
		let active = true;

		async function loadCatalogSections() {
			try {
				setCatalogLoading(true);
				setCatalogError(null);

				const [catalogo, emCartaz] = await Promise.all([
					listarCatalogoCompleto(20),
					listarEspetaculosEmCartaz(20),
				]);
				const popular = catalogo.slice(0, 10);

				if (active) {
					setPopularShows(popular);
					setEmCartazShows(emCartaz);
					setCatalogShows(catalogo);
				}

				const nearby = await listarEspetaculosPertoDe(options?.userLocationAddress, 8);
				if (active) {
					setNearbyShows(nearby);
				}
			} catch (error) {
				if (active) {
					setCatalogError(
						error instanceof Error ? error.message : 'Não foi possível carregar o catálogo.',
					);
				}
			} finally {
				if (active) {
					setCatalogLoading(false);
				}
			}
		}

		loadCatalogSections();

		return () => {
			active = false;
		};
	}, [options?.userLocationAddress, catalogReloadKey]);

	useEffect(() => {
		let active = true;
		const timeout = setTimeout(async () => {
			const trimmedQuery = query.trim();
			if (!trimmedQuery && !selectedFilter) {
				setFilteredShows([]);
				return;
			}

			try {
				const data = await listarEspetaculos({
					genre: selectedFilter,
					limit: 20,
					query: trimmedQuery,
					sortBy: 'popularidade',
				});

				if (active) {
					setFilteredShows(data);
				}
			} catch {
				if (active) {
					setFilteredShows([]);
				}
			}
		}, 260);

		return () => {
			active = false;
			clearTimeout(timeout);
		};
	}, [query, selectedFilter]);

	const recommendationItems = recommendations.map((item) => ({
		id: item.id,
		title: item.title,
		subtitle: item.genre ?? 'Recomendado para você',
		image: getRecommendationImage(item),
		imageUri: item.imageUrl ?? obterUrlMidiaConhecida(item.title, 'catalog_card'),
		recommendation: {
			score: item.score,
			algorithm: item.algorithm,
			reason: item.reason,
			generatedAt: item.generatedAt,
		},
	}));

	const filteredItems = filteredShows.map(toCatalogItem);
	const popularItems = popularShows.map(toCatalogItem);
	const nearbyItems = nearbyShows.map((item) => ({
		...toCatalogItem(item),
		subtitle: item.genre ?? 'Perto de você',
	}));
	const emCartazItems = emCartazShows.map(toCatalogItem);
	const catalogItems = catalogShows.map(toCatalogItem);
	const recommendationFallbackItems = deduplicarCatalogItems([
		...popularItems,
		...emCartazItems,
		...catalogItems,
	])
		.slice(0, 8)
		.map((item) => ({
			...item,
			subtitle: item.subtitle ?? 'Sugestão inicial',
		}));
	const recommendedSectionItems = recommendationItems.length > 0
		? recommendationItems
		: recommendationFallbackItems;
	const recommendedSectionTitle = recommendationItems.length > 0
		? 'Outras pessoas com gosto parecido curtiram:'
		: 'Sugestões iniciais:';
	const heroShow = useMemo(
		() => catalogShows.find((item) => normalizarTitulo(item.title).includes('elis')) ?? popularShows[0] ?? null,
		[catalogShows, popularShows],
	);
	const heroBackgroundUrl = heroShow
		? obterUrlMidia({ midias: heroShow.media }, 'catalog_hero_background')
			?? obterUrlMidiaConhecida(heroShow.title, 'catalog_hero_background')
		: null;
	const heroArtworkUrl = heroShow
		? obterUrlMidia({ midias: heroShow.media }, 'catalog_hero_artwork')
			?? obterUrlMidiaConhecida(heroShow.title, 'catalog_hero_artwork')
		: null;
	const assets = useMemo(
		() => ({
			...CATALOG_IMAGE,
			hero: heroBackgroundUrl ? { uri: heroBackgroundUrl } : undefined,
			heroArtwork: heroArtworkUrl ? { uri: heroArtworkUrl } : undefined,
		}),
		[heroBackgroundUrl, heroArtworkUrl],
	);

	const heroItem = useMemo<CatalogItem | null>(
		() => (heroShow ? toCatalogItem(heroShow) : recommendationItems[0] ?? popularItems[0] ?? emCartazItems[0] ?? null),
		[heroShow, recommendationItems, popularItems, emCartazItems],
	);
	const similarOriginId = recommendations[0]?.id ?? heroShow?.id ?? popularShows[0]?.id ?? null;
	const similarOriginTitle = recommendations[0]?.title ?? heroShow?.title ?? popularShows[0]?.title ?? null;
	const similarItems = similarShows
		.filter((item) => item.id !== similarOriginId)
		.map((item) => ({
			id: item.id,
			title: item.title,
			subtitle: item.genre ?? 'Similaridade por item',
			imageUri: item.imageUrl ?? obterUrlMidiaConhecida(item.title, 'catalog_card'),
			recommendation: {
				score: item.score,
				algorithm: item.algorithm,
			},
		}));

	useEffect(() => {
		let active = true;

		async function loadSimilarShows() {
			if (!similarOriginId) {
				setSimilarShows([]);
				return;
			}

			try {
				const data = await listarEspetaculosSimilares(similarOriginId, 8);
				if (active) {
					setSimilarShows(data);
				}
			} catch {
				if (active) {
					setSimilarShows([]);
				}
			}
		}

		loadSimilarShows();

		return () => {
			active = false;
		};
	}, [similarOriginId]);

	const sections: CatalogRail[] = [
		...(filteredItems.length > 0
			? [{ id: 'search-results', title: 'Resultados encontrados:', items: filteredItems }]
			: []),
		...(recommendedSectionItems.length > 0
			? [{ id: 'recommended', title: recommendedSectionTitle, items: recommendedSectionItems }]
			: []),
		...(similarItems.length > 0 && similarOriginTitle
			? [{ id: 'similar-by-item', title: `Porque você gostou de ${similarOriginTitle}:`, items: similarItems }]
			: []),
		...(nearbyItems.length > 0
			? [{ id: 'nearby', title: 'Perto de você:', items: nearbyItems }]
			: []),
		...(popularItems.length > 0
			? [{ id: 'popular', title: 'Os mais populares do momento:', items: popularItems }]
			: []),
		...(emCartazItems.length > 0
			? [{ id: 'em-cartaz', title: 'Em cartaz:', items: emCartazItems }]
			: []),
		...(catalogItems.length > 0
			? [{ id: 'catalogo-completo', title: 'Catálogo completo:', items: catalogItems }]
			: []),
	];

	return {
		assets,
		catalogError,
		catalogLoading,
		filterOptions: FILTER_OPTIONS,
		heroItem,
		query,
		recommendationsError,
		recommendationsLoading,
		selectedFilter,
		sections,
		recarregarCatalogo: () => setCatalogReloadKey((value) => value + 1),
		setQuery,
		setSelectedFilter,
		userName: 'Leonardo',
	};
}

function toCatalogItem(item: CatalogShow) {
	const catalogCardUrl = obterUrlMidia({ midias: item.media }, 'catalog_card');

	return {
		id: item.id,
		title: item.title,
		subtitle: item.genre ?? 'VER MAIS',
		imageUri: catalogCardUrl ?? item.imageUrl ?? obterUrlMidiaConhecida(item.title, 'catalog_card'),
	};
}

function deduplicarCatalogItems(items: CatalogItem[]) {
	const vistos = new Set<string>();

	return items.filter((item) => {
		if (vistos.has(item.id)) {
			return false;
		}

		vistos.add(item.id);
		return true;
	});
}

function getRecommendationImage(item: RecommendedShow) {
	void item;
	return undefined;
}
