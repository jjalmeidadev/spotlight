import React from 'react';
import { ImageBackground, ImageSourcePropType, Pressable, StyleSheet, View } from 'react-native';

type CatalogPosterCardProps = {
	title: string;
	subtitle?: string;
	imageSource: ImageSourcePropType;
	onPress?: () => void;
};

export function CatalogPosterCard({ imageSource, onPress }: CatalogPosterCardProps) {
	return (
		<Pressable onPress={onPress} style={styles.pressable} accessibilityRole="button">
			<ImageBackground source={imageSource} style={styles.card} imageStyle={styles.image}>
				<View style={styles.overlay} />
			</ImageBackground>
		</Pressable>
	);
}

const styles = StyleSheet.create({
	pressable: {
		width: 118,
	},
	card: {
		borderRadius: 10,
		height: 162,
		overflow: 'hidden',
		justifyContent: 'flex-end',
		width: '100%',
	},
	image: {
		borderRadius: 10,
	},
	overlay: {
		...StyleSheet.absoluteFillObject,
		backgroundColor: 'rgba(12, 12, 12, 0.1)',
	},
});