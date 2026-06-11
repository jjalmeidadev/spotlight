import React from 'react';
import { ImageBackground, ImageSourcePropType, Pressable, StyleSheet, View } from 'react-native';
import { colors } from '@/frontend/theme';

const SHOWCASE_CARD_ASPECT_RATIO = 368 / 416;

type ShowcaseCardProps = {
  title: string;
  subtitle?: string;
  imageUri?: string;
  imageSource?: ImageSourcePropType;
  onPress?: () => void;
};

export function ShowcaseCard({ imageUri, imageSource, onPress }: ShowcaseCardProps) {
  const source = imageSource ?? (imageUri ? { uri: imageUri } : undefined);

  return (
    <Pressable onPress={onPress} style={styles.pressable}>
      {source ? (
        <ImageBackground
          source={source}
          resizeMode="cover"
          imageStyle={styles.image}
          style={styles.card}
        />
      ) : (
        <View style={[styles.card, styles.placeholder]} />
      )}
    </Pressable>
  );
}

const styles = StyleSheet.create({
  pressable: {
    width: 160,
  },
  card: {
    aspectRatio: SHOWCASE_CARD_ASPECT_RATIO,
    width: '100%',
    borderRadius: 18,
    overflow: 'hidden',
    justifyContent: 'flex-end',
    backgroundColor: colors.surface,
  },
  image: {
    borderRadius: 18,
  },
  placeholder: {
    backgroundColor: '#17211D',
  },
});
