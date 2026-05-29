import { useState } from 'react';
import { CatalogRail } from '../models/CatalogItem';

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

const FILTER_OPTIONS = ['Musical', 'Stand-up', 'Show', 'Espetáculo', 'Palestra', 'Meet-up'] as const;

const POPULAR_ITEMS = [
	{ id: 'm1', title: 'Mamma Mia!', subtitle: 'VER MAIS', image: CATALOG_IMAGE.mammaMia },
	{ id: 'm2', title: 'Mamma Mia!', subtitle: 'VER MAIS', image: CATALOG_IMAGE.mammaMia },
	{ id: 'm3', title: 'Mamma Mia!', subtitle: 'VER MAIS', image: CATALOG_IMAGE.mammaMia },
	{ id: 'm4', title: 'Mamma Mia!', subtitle: 'VER MAIS', image: CATALOG_IMAGE.mammaMia },
];

const WATCHED_ITEMS = [
	{ id: 'e1', title: 'Elis, a Musical', subtitle: 'VER MAIS', image: CATALOG_IMAGE.elis },
	{ id: 'e2', title: 'Elis, a Musical', subtitle: 'VER MAIS', image: CATALOG_IMAGE.elis },
	{ id: 'e3', title: 'Elis, a Musical', subtitle: 'VER MAIS', image: CATALOG_IMAGE.elis },
	{ id: 'e4', title: 'Elis, a Musical', subtitle: 'VER MAIS', image: CATALOG_IMAGE.elis },
];

const AFFINITY_ITEMS = [
	{ id: 'h1', title: 'Hair, o Musical', subtitle: 'VER MAIS', image: CATALOG_IMAGE.hair },
	{ id: 'h2', title: 'Hair, o Musical', subtitle: 'VER MAIS', image: CATALOG_IMAGE.hair },
	{ id: 'h3', title: 'Hair, o Musical', subtitle: 'VER MAIS', image: CATALOG_IMAGE.hair },
	{ id: 'h4', title: 'Hair, o Musical', subtitle: 'VER MAIS', image: CATALOG_IMAGE.hair },
];

export function useCatalogViewModel() {
	const [query, setQuery] = useState('');
	const [selectedFilter, setSelectedFilter] = useState<(typeof FILTER_OPTIONS)[number] | null>(null);

	const sections: CatalogRail[] = [
		{ id: 'popular', title: 'Os mais populares do momento:', items: POPULAR_ITEMS },
		{ id: 'watched', title: 'Outras pessoas também assistiram:', items: WATCHED_ITEMS },
		{ id: 'affinity', title: 'Porque você gostou de “Macacos”:', items: AFFINITY_ITEMS },
	];

	return {
		assets: CATALOG_IMAGE,
		filterOptions: FILTER_OPTIONS,
		query,
		selectedFilter,
		sections,
		setQuery,
		setSelectedFilter,
		userName: 'Leonardo',
	};
}