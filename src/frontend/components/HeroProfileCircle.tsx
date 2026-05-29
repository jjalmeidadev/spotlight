import React from 'react';
import { Image, ImageSourcePropType, StyleSheet, Text, View } from 'react-native';

type HeroProfileCircleProps = {
	imageSource?: ImageSourcePropType | null;
	fallbackLabel?: string;
	size?: number;
};

export function HeroProfileCircle({ imageSource, fallbackLabel = 'U', size = 34 }: HeroProfileCircleProps) {
	return (
		<View style={[styles.wrapper, { width: size, height: size, borderRadius: size / 2 }]}>
			{imageSource ? (
				<Image source={imageSource} style={[styles.image, { width: size, height: size, borderRadius: size / 2 }]} />
			) : (
				<Text style={[styles.fallback, { fontSize: Math.max(9, Math.round(size * 0.42)) }]}>{fallbackLabel.slice(0, 1).toUpperCase()}</Text>
			)}
		</View>
	);
}

const styles = StyleSheet.create({
	wrapper: {
		alignItems: 'center',
		justifyContent: 'center',
		overflow: 'hidden',
		backgroundColor: 'rgba(44, 44, 44, 0.88)',
	},
	image: {
		resizeMode: 'cover',
	},
	fallback: {
		color: '#f4fbf8',
		fontWeight: '700',
	},
});