import React from 'react';
import { ScrollView, StyleSheet, View, useWindowDimensions } from 'react-native';
import { colors } from '@/frontend/theme';
import { CatalogContentShell } from '../components/CatalogContentShell';
import { CatalogHero } from '../components/CatalogHero';
import { HeroFloatingArtwork } from '../components/HeroFloatingArtwork';
import { CatalogRail } from '../components/CatalogRail';
import { FilterDropdown } from '../components/FilterDropdown';
import { SearchBar } from '../components/SearchBar';
import type { CatalogItem } from '../models/CatalogItem';
import { emitTelemetryEvent } from '../repositories/TelemetryRepository';
import { useCatalogViewModel } from '../viewmodels/useCatalogViewModel';

type CatalogViewProps = {
	accessToken?: string | null;
	profileName?: string;
	onItemPress?: (item: CatalogItem) => void;
};

export default function CatalogView({ accessToken, profileName, onItemPress }: CatalogViewProps) {
	const { width } = useWindowDimensions();
	const vm = useCatalogViewModel();
	const horizontalPadding = Math.max(16, Math.round(width * 0.044));
	const firstSection = vm.sections[0];
	const remainingSections = vm.sections.slice(1);

	function handleSearchSubmit() {
		const query = vm.query.trim();
		if (!query) return;

		emitTelemetryEvent(
			{
				eventType: 'search_performed',
				screen: 'CatalogView',
				entity: { type: 'busca', id: getSearchEntityId(query) },
				properties: {
					query,
					resultsCount: getSearchResultsCount(vm.sections, query),
				},
			},
			accessToken,
		);
	}

	function handleItemPress(
		item: CatalogItem,
		context: { railId?: string; railTitle: string; position: number },
	) {
		emitTelemetryEvent(
			{
				eventType: 'show_card_clicked',
				screen: 'CatalogView',
				entity: { type: 'espetaculo', id: item.id },
				properties: {
					origin: context.railId ?? context.railTitle,
					position: context.position,
				},
			},
			accessToken,
		);

		onItemPress?.(item);
	}

	return (
		<View style={styles.container}>
			<ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.scrollContent}>
				<View style={styles.heroStack}>
					<CatalogHero
						actionLabel="Comprar"
						heroSource={vm.assets.hero}
						overSource={vm.assets.heroArtwork}
						profileLabel={profileName ?? vm.userName}
						profileSource={null}
						subtitle="Um dos maiores clássicos da broadway"
						title="A Noviça Rebelde"
					/>

					<CatalogContentShell topBackgroundSource={vm.assets.whiteShell} logoSource={vm.assets.logo}>
						<View style={[styles.controls, { paddingHorizontal: horizontalPadding }]}>
							<SearchBar
								iconSource={vm.assets.searchIcon}
								onChangeText={vm.setQuery}
								onSubmitEditing={handleSearchSubmit}
								placeholder=""
								returnKeyType="search"
								value={vm.query}
							/>
							<View style={styles.filterWrap}>
								<FilterDropdown
									iconSource={vm.assets.filterIcon}
									options={vm.filterOptions}
									value={vm.selectedFilter}
									onChange={vm.setSelectedFilter}
								/>
							</View>
						</View>

						<View style={[styles.preExpansiveArea, { paddingHorizontal: horizontalPadding }]}>
							{firstSection ? (
								<CatalogRail
									id={firstSection.id}
									items={firstSection.items}
									title={firstSection.title}
									onItemPress={handleItemPress}
								/>
							) : null}
						</View>

						<View style={[styles.expansiveWhiteArea, { paddingHorizontal: horizontalPadding }]}>
							{remainingSections.map((section) => (
								<View key={section.id} style={styles.sectionGap}>
									<CatalogRail
										id={section.id}
										items={section.items}
										title={section.title}
										onItemPress={handleItemPress}
									/>
								</View>
							))}
						</View>
					</CatalogContentShell>

					<HeroFloatingArtwork source={vm.assets.heroArtwork} style={styles.heroArtworkOverlay} />
				</View>
			</ScrollView>
		</View>
	);
}

function getSearchEntityId(query: string) {
	return `search_${query.toLowerCase().replace(/\s+/g, '_')}`.slice(0, 96);
}

function getSearchResultsCount(
	sections: ReturnType<typeof useCatalogViewModel>['sections'],
	query: string,
) {
	const normalizedQuery = query.toLowerCase();

	return sections
		.flatMap((section) => section.items)
		.filter((item) => item.title.toLowerCase().includes(normalizedQuery))
		.length;
}

const styles = StyleSheet.create({
	container: {
		flex: 1,
		backgroundColor: '#FFFFFF',
	},
	scrollContent: {
		backgroundColor: '#FFFFFF',
		paddingBottom: 18,
	},
	heroStack: {
		position: 'relative',
		overflow: 'visible',
	},
	controls: {
		gap: 18,
		paddingTop: 6,
	},
	filterWrap: {
		alignItems: 'flex-start',
	},
	preExpansiveArea: {
		backgroundColor: '#FFFFFF',
		paddingTop: 26,
	},
	expansiveWhiteArea: {
		backgroundColor: '#FFFFFF',
		paddingBottom: 18,
		paddingTop: 26,
		minHeight: 420,
	},
	sectionGap: {
		marginTop: 0,
	},
	heroArtworkOverlay: {
		height: 400,
		right: -88,
		top: 127.5,
		width: 384,
		zIndex: 50,
		elevation: 50,
	},
});
