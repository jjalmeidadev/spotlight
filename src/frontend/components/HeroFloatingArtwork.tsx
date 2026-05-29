import React from 'react';
import { Image, ImageSourcePropType, StyleProp, StyleSheet, ImageStyle } from 'react-native';

type HeroFloatingArtworkProps = {
	source: ImageSourcePropType;
	style?: StyleProp<ImageStyle>;
};

export function HeroFloatingArtwork({ source, style }: HeroFloatingArtworkProps) {
    return <Image source={source} style={[styles.image, style]} resizeMode="contain" />;
}

const styles = StyleSheet.create({
	image: {
		position: 'absolute',
		right: 0,
		bottom: -7,
		resizeMode: 'contain',
	},
});