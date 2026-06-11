import { useEffect, useState } from 'react';
import { CatalogRail } from '../models/CatalogItem';
import {
	CatalogShow,
	listarEspetaculosPertoDe,
	listarEspetaculosPopulares,
} from '../repositories/CatalogRepository';
import {
	listarRecomendacoesUsuario,
	RecommendedShow,
} from '../repositories/RecommendationRepository';

const CATALOG_IMAGE = {
	hero: require('../../../assets/images/catalog/catalog-hero.png'),
	heroArtwork: require('../../../assets/images/catalog/catalog-hero-overimage.png'),
	whiteShell: require('../../../assets/images/catalog/component-whitebgtop.png'),
	logo: require('../../../assets/images/catalog/logo-spotlight.png'),
	searchIcon: require('../../../assets/images/catalog/searchbar-icon.png'),
	filterIcon: require('../../../assets/images/catalog/filter-config-component.png'),
	mammaMia: require('../../../assets/images/catalog/catalog-mammamia.png'),
	elis: require('../../../assets/images/catalog/catalog-elis.png'),
	hair: require('../../../assets/images/catalog/catalog-hair.png'),
} as const;

const FILTER_OPTIONS = ['Musical', 'Stand-up', 'Show', 'Espetaculo', 'Palestra', 'Meet-up'] as const;

const FALLBACK_POPULAR_ITEMS = [
	{ id: '16', title: 'Mamma Mia!', subtitle: 'VER MAIS', image: CATALOG_IMAGE.mammaMia },
	{ id: '17', title: 'Mamma Mia!', subtitle: 'VER MAIS', image: CATALOG_IMAGE.mammaMia },
	{ id: '18', title: 'Mamma Mia!', subtitle: 'VER MAIS', image: CATALOG_IMAGE.mammaMia },
	{ id: '19', title: 'Mamma Mia!', subtitle: 'VER MAIS', image: CATALOG_IMAGE.mammaMia },
];

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
	const [recommendationsLoading, setRecommendationsLoading] = useState(false);
	const [recommendationsError, setRecommendationsError] = useState<string | null>(null);

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
						error instanceof Error ? error.message : 'Nao foi possivel carregar recomendacoes.',
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
				const popular = await listarEspetaculosPopulares(8);
				if (active) {
					setPopularShows(popular);
				}

				const nearby = await listarEspetaculosPertoDe(options?.userLocationAddress, 8);
				if (active) {
					setNearbyShows(nearby);
				}
			} catch {
				if (active) {
					setPopularShows([]);
					setNearbyShows([]);
				}
			}
		}

		loadCatalogSections();

		return () => {
			active = false;
		};
	}, [options?.userLocationAddress]);

	const recommendationItems = recommendations.map((item) => ({
		id: item.id,
		title: item.title,
		subtitle: item.genre ?? 'Recomendado para voce',
		image: getRecommendationImage(item),
		imageUri: item.imageUrl,
		recommendation: {
			score: item.score,
			algorithm: item.algorithm,
			reason: item.reason,
			generatedAt: item.generatedAt,
		},
	}));

	const popularItems = popularShows.map(toCatalogItem);
	const nearbyItems = nearbyShows.map((item) => ({
		...toCatalogItem(item),
		subtitle: item.genre ?? 'Perto de voce',
	}));

	const sections: CatalogRail[] = [
		...(recommendationItems.length > 0
			? [{ id: 'recommended', title: 'Recomendados para voce:', items: recommendationItems }]
			: []),
		...(nearbyItems.length > 0
			? [{ id: 'nearby', title: 'Perto de voce:', items: nearbyItems }]
			: []),
		{
			id: 'popular',
			title: 'Os mais populares do momento:',
			items: popularItems.length > 0 ? popularItems : FALLBACK_POPULAR_ITEMS,
		},
	];

	return {
		assets: CATALOG_IMAGE,
		filterOptions: FILTER_OPTIONS,
		query,
		recommendationsError,
		recommendationsLoading,
		selectedFilter,
		sections,
		setQuery,
		setSelectedFilter,
		userName: 'Leonardo',
	};
}

function toCatalogItem(item: CatalogShow) {
	return {
		id: item.id,
		title: item.title,
		subtitle: item.genre ?? 'VER MAIS',
		image: getCatalogImage(item),
		imageUri: item.imageUrl,
	};
}

function getRecommendationImage(item: RecommendedShow) {
	const normalized = `${item.title} ${item.genre ?? ''}`.toLowerCase();

	if (normalized.includes('mamma') || normalized.includes('musical')) {
		return CATALOG_IMAGE.mammaMia;
	}

	if (normalized.includes('elis') || normalized.includes('drama')) {
		return CATALOG_IMAGE.elis;
	}

	return CATALOG_IMAGE.hair;
}

function getCatalogImage(item: CatalogShow) {
	const normalized = `${item.title} ${item.genre ?? ''}`.toLowerCase();

	if (normalized.includes('mamma')) {
		return CATALOG_IMAGE.mammaMia;
	}

	if (normalized.includes('elis')) {
		return CATALOG_IMAGE.elis;
	}

	if (normalized.includes('hair')) {
		return CATALOG_IMAGE.hair;
	}

	return CATALOG_IMAGE.heroArtwork;
}
