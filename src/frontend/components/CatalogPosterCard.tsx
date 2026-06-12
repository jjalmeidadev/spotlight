import React from 'react';
import { ImageBackground, ImageSourcePropType, Pressable, StyleSheet, Text, View } from 'react-native';

const CARD_WIDTH = 118;
const CATALOG_CARD_ASPECT_RATIO = 368 / 416;

type CatalogPosterCardProps = {
	title: string;
	subtitle?: string;
	imageSource?: ImageSourcePropType;
	imageUri?: string | null;
	onPress?: () => void;
};

export function CatalogPosterCard({ title, subtitle, imageSource, imageUri, onPress }: CatalogPosterCardProps) {
	const source = imageUri ? { uri: imageUri } : imageSource;
	const [imageFailed, setImageFailed] = React.useState(false);
	const shouldShowImage = Boolean(source) && !imageFailed;

	return (
		<Pressable onPress={onPress} style={styles.pressable} accessibilityRole="button">
			{shouldShowImage ? (
				<ImageBackground
					source={source}
					style={styles.card}
					imageStyle={styles.image}
					onError={() => setImageFailed(true)}
				/>
			) : (
				<View style={[styles.card, styles.placeholder]}>
					<View style={styles.textOverlay}>
						<Text numberOfLines={3} style={styles.title}>
							{title}
						</Text>
						{subtitle ? (
							<Text numberOfLines={1} style={styles.subtitle}>
								{subtitle}
							</Text>
						) : null}
					</View>
				</View>
			)}
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
	placeholder: {
		backgroundColor: '#17211D',
	},
	textOverlay: {
		bottom: 10,
		left: 10,
		position: 'absolute',
		right: 10,
	},
	title: {
		color: '#FFFFFF',
		fontFamily: 'Gotham-Bold',
		fontSize: 15,
		lineHeight: 17,
	},
	subtitle: {
		color: '#D7DE65',
		fontFamily: 'Gotham-Bold',
		fontSize: 8,
		lineHeight: 10,
		marginTop: 4,
		textTransform: 'uppercase',
	},
});
