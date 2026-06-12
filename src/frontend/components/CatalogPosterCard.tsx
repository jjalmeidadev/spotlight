import React from 'react';
import { ImageBackground, ImageSourcePropType, Pressable, StyleSheet, View } from 'react-native';

const CARD_WIDTH = 118;
const CATALOG_CARD_ASPECT_RATIO = 368 / 416;

type CatalogPosterCardProps = {
	title: string;
	subtitle?: string;
	imageSource: ImageSourcePropType;
	imageUri?: string | null;
	onPress?: () => void;
};

export function CatalogPosterCard({ title, subtitle, imageSource, imageUri, onPress }: CatalogPosterCardProps) {
	void title;
	void subtitle;

	const source = imageUri ? { uri: imageUri } : imageSource;

	return (
		<Pressable onPress={onPress} style={styles.pressable} accessibilityRole="button">
			<ImageBackground source={source} style={styles.card} imageStyle={styles.image} />
		</Pressable>
	);
}

const styles = StyleSheet.create({
	pressable: {
		width: CARD_WIDTH,
	},
	card: {
		aspectRatio: CATALOG_CARD_ASPECT_RATIO,
		borderRadius: 10,
		overflow: 'hidden',
		justifyContent: 'flex-end',
		width: '100%',
	},
	image: {
		borderRadius: 10,
	},
});
