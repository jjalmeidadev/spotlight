import { ImageSourcePropType } from 'react-native';

export type CatalogItem = {
	id: string;
	title: string;
	subtitle?: string;
	image?: ImageSourcePropType;
	imageUri?: string | null;
	recommendation?: {
		score: number;
		algorithm: string;
		reason?: string | null;
		generatedAt?: string;
	};
};

export type CatalogRail = {
	id: string;
	title: string;
	items: CatalogItem[];
};
