import React from 'react';
import { ImageBackground, ImageSourcePropType, Pressable, StyleSheet, Text, View } from 'react-native';

type CatalogPosterCardProps = {
	title: string;
	subtitle?: string;
	imageSource: ImageSourcePropType;
	imageUri?: string | null;
	onPress?: () => void;
};

export function CatalogPosterCard({ title, subtitle, imageSource, imageUri, onPress }: CatalogPosterCardProps) {
	const source = imageUri ? { uri: imageUri } : imageSource;

	return (
		<Pressable onPress={onPress} style={styles.pressable} accessibilityRole="button">
			<ImageBackground source={source} style={styles.card} imageStyle={styles.image}>
				<View style={styles.overlay} />
				<View style={styles.copy}>
					<Text numberOfLines={2} style={styles.title}>
						{title}
					</Text>
					{subtitle ? (
						<Text numberOfLines={1} style={styles.subtitle}>
							{subtitle}
						</Text>
					) : null}
				</View>
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
		backgroundColor: 'rgba(12, 12, 12, 0.34)',
	},
	copy: {
		gap: 4,
		padding: 10,
		zIndex: 1,
	},
	title: {
		color: '#FFFFFF',
		fontSize: 13,
		fontWeight: '800',
		lineHeight: 15,
		textShadowColor: 'rgba(0, 0, 0, 0.34)',
		textShadowOffset: { width: 0, height: 1 },
		textShadowRadius: 2,
	},
	subtitle: {
		color: 'rgba(255, 255, 255, 0.86)',
		fontSize: 9,
		fontWeight: '700',
		lineHeight: 11,
	},
});
