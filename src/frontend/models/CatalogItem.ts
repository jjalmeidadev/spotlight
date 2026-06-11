import { ImageSourcePropType } from 'react-native';

export type CatalogItem = {
	id: string;
	title: string;
	subtitle?: string;
	image?: ImageSourcePropType;
};

export type CatalogRail = {
	id: string;
	title: string;
	items: CatalogItem[];
};
