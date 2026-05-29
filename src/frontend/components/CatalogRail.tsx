import React from 'react';
import { FlatList, StyleSheet, View } from 'react-native';
import { spacing } from '@/frontend/theme';
import { CatalogItem } from '../models/CatalogItem';
import { CatalogPosterCard } from './CatalogPosterCard';
import { SectionTitle } from './SectionTitle';

type CatalogRailProps = {
	title: string;
	items: CatalogItem[];
	onItemPress?: (item: CatalogItem) => void;
};

export function CatalogRail({ title, items, onItemPress }: CatalogRailProps) {
	return (
		<View style={styles.section}>
			<SectionTitle title={title} />
			<FlatList
				horizontal
				data={items}
				keyExtractor={(item) => item.id}
				showsHorizontalScrollIndicator={false}
				contentContainerStyle={styles.listContent}
				ItemSeparatorComponent={() => <View style={styles.separator} />}
				renderItem={({ item }) => (
					<CatalogPosterCard
						title={item.title}
						subtitle={item.subtitle}
						imageSource={item.image}
						onPress={() => onItemPress?.(item)}
					/>
				)}
			/>
		</View>
	);
}

const styles = StyleSheet.create({
	section: {
		gap: 14,
	},
	listContent: {
		paddingRight: 16,
	},
	separator: {
		width: 10,
	},
});