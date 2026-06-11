import React from 'react';
import { Image, ImageSourcePropType, Pressable, StyleSheet, Text, View, useWindowDimensions } from 'react-native';
import { colors, typography } from '@/frontend/theme';
import { HeroProfileCircle } from './HeroProfileCircle';

type CatalogHeroProps = {
	title: string;
	subtitle: string;
	actionLabel: string;
	heroSource: ImageSourcePropType;
	onActionPress?: () => void;
	overSource: ImageSourcePropType;
	profileLabel: string;
	profileSource?: ImageSourcePropType | null;
};

export function CatalogHero({
	title,
	subtitle,
	actionLabel,
	heroSource,
	onActionPress,
	profileLabel,
	profileSource,
}: CatalogHeroProps) {
	const { width } = useWindowDimensions();
	const heroAsset = Image.resolveAssetSource(heroSource);
	const heroImageHeight = Math.round((width * heroAsset.height) / heroAsset.width);

	return (
		<View style={styles.hero}>
			<Image source={heroSource} style={[styles.heroImage, { height: heroImageHeight }]} />
			<View style={styles.overlay} />
			<View style={styles.headerRow}>
				<View style={styles.headerSpacer} />
				<View style={styles.welcomeRow}>
					<Text style={styles.welcomeText} numberOfLines={1} ellipsizeMode="tail">
						Bem vindo, {profileLabel}!
					</Text>
					<HeroProfileCircle imageSource={profileSource} fallbackLabel={profileLabel} size={38} />
				</View>
			</View>

			<View style={styles.textBlock}>
				<Text style={styles.title} numberOfLines={2}>
					{title}
				</Text>
				<Text style={styles.subtitle} numberOfLines={2}>
					{subtitle}
				</Text>
				<Pressable onPress={onActionPress} style={styles.cta} accessibilityRole="button">
					<Text style={styles.ctaText}>{actionLabel}</Text>
				</Pressable>
			</View>
		</View>
	);
}

const styles = StyleSheet.create({
	hero: {
		height: 252,
		position: 'relative',
		overflow: 'visible',
		width: '100%',
		zIndex: 6,
		elevation: 6,
	},
	heroImage: {
		position: 'absolute',
		top: 0,
		left: 0,
		right: 0,
		width: '100%',
		resizeMode: 'cover',
	},
	overlay: {
		...StyleSheet.absoluteFillObject,
		backgroundColor: 'rgba(17, 20, 18, 0.04)',
	},
	headerRow: {
		alignItems: 'center',
		flexDirection: 'row',
		justifyContent: 'space-between',
		paddingHorizontal: 16,
		paddingTop: 64,
		position: 'relative',
		zIndex: 2,
	},
	headerSpacer: {
		flex: 1,
	},
	welcomeRow: {
		alignItems: 'center',
		flexDirection: 'row',
		gap: 8,
		maxWidth: 206,
	},
	welcomeText: {
		color: colors.background,
		...typography.caption,
		fontSize: 9,
		lineHeight: 11,
		maxWidth: 156,
		textAlign: 'right',
	},
	textBlock: {
		maxWidth: 166,
		paddingLeft: 14,
		paddingTop: 12,
		position: 'relative',
		zIndex: 2,
	},
	title: {
		color: '#dae5e1',
		...typography.title,
		fontSize: 30,
		lineHeight: 28,
	},
	subtitle: {
		color: colors.background,
		...typography.body,
		fontSize: 10,
		lineHeight: 12,
		marginTop: 2,
	},
	cta: {
		alignItems: 'center',
		backgroundColor: '#303030',
		borderRadius: 999,
		justifyContent: 'center',
		marginTop: 8,
		minHeight: 28,
		width: 132,
	},
	ctaText: {
		color: '#dae5e1',
		...typography.button,
		fontSize: 10,
		lineHeight: 12,
	},
});
