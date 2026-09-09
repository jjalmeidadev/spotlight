import React from 'react';
import {
  FlatList,
  ListRenderItem,
  NativeScrollEvent,
  NativeSyntheticEvent,
  StyleSheet,
  Text,
  View,
  ViewStyle,
} from 'react-native';
import { colors, spacing, typography } from '@/frontend/theme';

export type CarouselItem<T> = T & { id: string };

type CarouselProps<T> = {
  title?: string;
  subtitle?: string;
  data: Array<CarouselItem<T>>;
  renderItem: ListRenderItem<CarouselItem<T>>;
  itemWidth?: number;
  itemGap?: number;
  contentContainerStyle?: ViewStyle;
  onEndReached?: () => void;
  onScroll?: (event: NativeSyntheticEvent<NativeScrollEvent>) => void;
};

export function Carousel<T>({
  title,
  subtitle,
  data,
  renderItem,
  itemWidth = 158,
  itemGap = spacing.sm,
  contentContainerStyle,
  onEndReached,
  onScroll,
}: CarouselProps<T>) {
  return (
    <View style={styles.section}>
      {title ? <Text style={styles.title}>{title}</Text> : null}
      {subtitle ? <Text style={styles.subtitle}>{subtitle}</Text> : null}

      <FlatList
        data={data}
        horizontal
        keyExtractor={(item) => item.id}
        renderItem={renderItem}
        showsHorizontalScrollIndicator={false}
        snapToAlignment="start"
        decelerationRate="fast"
        scrollEventThrottle={16}
        contentContainerStyle={[styles.listContent, contentContainerStyle]}
        ItemSeparatorComponent={() => <View style={{ width: itemGap }} />}
        onEndReached={onEndReached}
        onScroll={onScroll}
        getItemLayout={(_, index) => ({
          length: itemWidth + itemGap,
          offset: (itemWidth + itemGap) * index,
          index,
        })}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  section: {
    gap: spacing.sm,
  },
  title: {
    color: colors.textStrong,
    ...typography.title,
    fontSize: 24,
    lineHeight: 30,
  },
  subtitle: {
    color: colors.textMuted,
    ...typography.body,
  },
  listContent: {
    paddingRight: spacing.lg,
  },
});