import React from 'react';
import { ActivityIndicator, Pressable, ScrollView, StyleSheet, Text, View, useWindowDimensions } from 'react-native';
import { colors } from '@/frontend/theme';
import { CATALOG_WHITE_PLANE_TOP, CatalogContentShell } from '../components/CatalogContentShell';
import { CATALOG_HERO_HEIGHT, CatalogHero } from '../components/CatalogHero';
import { HeroFloatingArtwork } from '../components/HeroFloatingArtwork';
import { CatalogRail } from '../components/CatalogRail';
import { FilterDropdown } from '../components/FilterDropdown';
import { ProfileSheet } from '../components/ProfileSheet';
import { SearchBar } from '../components/SearchBar';
import type { CatalogItem } from '../models/CatalogItem';
import { emitTelemetryEvent } from '../repositories/TelemetryRepository';
import { useCatalogViewModel } from '../viewmodels/useCatalogViewModel';

type CatalogViewProps = {
	accessToken?: string | null;
	profileEmail?: string;
	profileName?: string;
	userLocationAddress?: string | null;
	onItemPress?: (item: CatalogItem) => void;
	onLogout?: () => void;
	onProfileUpdated?: (name: string) => void;
};

const HERO_HEIGHT = CATALOG_HERO_HEIGHT;
const CONTENT_SHELL_MARGIN_TOP = 80;
const HERO_ARTWORK_BASELINE_OFFSET = -152;
const SECTION_VERTICAL_GAP = 56;

export default function CatalogView({
	accessToken,
	profileEmail,
	profileName,
	userLocationAddress,
	onItemPress,
	onLogout,
	onProfileUpdated,
}: CatalogViewProps) {
	const [profileOpen, setProfileOpen] = React.useState(false);
	const { width } = useWindowDimensions();
	const vm = useCatalogViewModel({ accessToken, userLocationAddress });
	const horizontalPadding = Math.max(16, Math.round(width * 0.044));
	const firstSection = vm.sections[0];
	const remainingSections = vm.sections.slice(1);
	const heroArtworkWidth = Math.round(width * 0.30);
	const heroArtworkHeight = Math.round(width * 0.60);
	const heroArtworkRight = Math.max(8, horizontalPadding - 8);
	const heroArtworkBaseline = HERO_HEIGHT + CONTENT_SHELL_MARGIN_TOP + CATALOG_WHITE_PLANE_TOP + HERO_ARTWORK_BASELINE_OFFSET;
	const heroArtworkTop = heroArtworkBaseline - heroArtworkHeight;

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
					recommendationAlgorithm: item.recommendation?.algorithm,
					recommendationScore: item.recommendation?.score,
					recommendationReason: item.recommendation?.reason,
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
						onActionPress={() => {
							if (vm.heroItem) {
								handleItemPress(vm.heroItem, {
									railId: 'hero',
									railTitle: 'Hero',
									position: 0,
								});
							}
						}}
						overSource={vm.assets.heroArtwork}
						onProfilePress={() => setProfileOpen(true)}
						profileLabel={profileName ?? vm.userName}
						profileSource={null}
						subtitle={vm.heroItem?.subtitle ?? 'Um dos maiores classicos da broadway'}
						title={vm.heroItem?.title ?? 'A Novica Rebelde'}
					/>

					<CatalogContentShell topBackgroundSource={vm.assets.whiteShell} logoSource={vm.assets.logo}>
						<View style={[styles.controls, { paddingHorizontal: horizontalPadding }]}>
							<SearchBar
								iconSource={vm.assets.searchIcon}
								onChangeText={vm.setQuery}
								onSubmitEditing={handleSearchSubmit}
								placeholder="Buscar espetaculo"
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
							{vm.catalogLoading ? (
								<View style={styles.stateBox}>
									<ActivityIndicator color={colors.textStrong} />
									<Text style={styles.stateText}>Carregando espetaculos...</Text>
								</View>
							) : vm.catalogError ? (
								<View style={styles.stateBox}>
									<Text style={styles.stateText}>{vm.catalogError}</Text>
									<Pressable
										accessibilityRole="button"
										onPress={vm.recarregarCatalogo}
										style={({ pressed }) => [styles.retryButton, pressed && styles.pressed]}
									>
										<Text style={styles.retryLabel}>Tentar novamente</Text>
									</Pressable>
								</View>
							) : firstSection ? (
								<CatalogRail
									id={firstSection.id}
									items={firstSection.items}
									title={firstSection.title}
									onItemPress={handleItemPress}
								/>
							) : (
								<View style={styles.stateBox}>
									<Text style={styles.stateText}>Nenhum espetaculo encontrado no momento.</Text>
								</View>
							)}
						</View>

						{!vm.catalogLoading && !vm.catalogError ? (
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
						) : null}
					</CatalogContentShell>

					{vm.assets.heroArtwork ? (
						<HeroFloatingArtwork
							source={vm.assets.heroArtwork}
							style={[
								styles.heroArtworkOverlay,
								{
									height: heroArtworkHeight,
									right: heroArtworkRight,
									top: heroArtworkTop,
									width: heroArtworkWidth,
								},
							]}
						/>
					) : null}
				</View>
			</ScrollView>
			<ProfileSheet
				accessToken={accessToken}
				email={profileEmail}
				name={profileName ?? vm.userName}
				onClose={() => setProfileOpen(false)}
				onLogout={onLogout ?? (() => undefined)}
				onProfileUpdated={onProfileUpdated}
				visible={profileOpen}
			/>
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
		paddingTop: SECTION_VERTICAL_GAP,
	},
	expansiveWhiteArea: {
		backgroundColor: '#FFFFFF',
		paddingBottom: 18,
		paddingTop: SECTION_VERTICAL_GAP,
		minHeight: 420,
	},
	sectionGap: {
		marginTop: SECTION_VERTICAL_GAP,
	},
	stateBox: {
		alignItems: 'flex-start',
		gap: 12,
		minHeight: 150,
		justifyContent: 'center',
	},
	stateText: {
		color: colors.text,
		fontFamily: 'Gotham-Medium',
		fontSize: 16,
		lineHeight: 22,
	},
	retryButton: {
		backgroundColor: colors.surface,
		borderRadius: 24,
		paddingHorizontal: 22,
		paddingVertical: 12,
	},
	retryLabel: {
		color: colors.textStrong,
		fontFamily: 'Gotham-Bold',
		fontSize: 14,
	},
	pressed: {
		opacity: 0.72,
	},
	heroArtworkOverlay: {
		zIndex: 50,
		elevation: 50,
	},
});
