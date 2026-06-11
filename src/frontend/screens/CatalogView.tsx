import React from 'react';
import { ActivityIndicator, ScrollView, StyleSheet, Text, View, useWindowDimensions } from 'react-native';
import { CATALOG_WHITE_PLANE_TOP, CatalogContentShell } from '../components/CatalogContentShell';
import { CATALOG_HERO_HEIGHT, CatalogHero } from '../components/CatalogHero';
import { HeroFloatingArtwork } from '../components/HeroFloatingArtwork';
import { CatalogRail } from '../components/CatalogRail';
import { FilterDropdown } from '../components/FilterDropdown';
import { SearchBar } from '../components/SearchBar';
import type { CatalogItem } from '../models/CatalogItem';
import { useCatalogViewModel } from '../viewmodels/useCatalogViewModel';

type CatalogViewProps = {
	accessToken?: string | null;
	profileName?: string;
	onItemPress?: (item: CatalogItem) => void;
};

const HERO_HEIGHT = CATALOG_HERO_HEIGHT;
const CONTENT_SHELL_MARGIN_TOP = 80;
const HERO_ARTWORK_BASELINE_OFFSET = -152;
const SECTION_VERTICAL_GAP = 56;

export default function CatalogView({ accessToken, profileName, onItemPress }: CatalogViewProps) {
	const { width } = useWindowDimensions();
	const vm = useCatalogViewModel(accessToken);
	const horizontalPadding = Math.max(16, Math.round(width * 0.044));
	const firstSection = vm.sections[0];
	const remainingSections = vm.sections.slice(1);
	const heroTitle = vm.heroEspetaculo?.titulo ?? '';
	const heroSubtitle = vm.heroEspetaculo?.genero
		? `${vm.heroEspetaculo.genero} em destaque`
		: '';
	const heroArtworkWidth = Math.round(width * 0.30);
	const heroArtworkHeight = Math.round(width * 0.60);
	const heroArtworkRight = Math.max(8, horizontalPadding - 8);
	const heroArtworkBaseline = HERO_HEIGHT + CONTENT_SHELL_MARGIN_TOP + CATALOG_WHITE_PLANE_TOP + HERO_ARTWORK_BASELINE_OFFSET;
	const heroArtworkTop = heroArtworkBaseline - heroArtworkHeight;

	if (vm.loading) {
		return (
			<View style={[styles.container, styles.centeredState]}>
				<ActivityIndicator color="#303030" size="large" />
			</View>
		);
	}

	if (vm.error) {
		return (
			<View style={[styles.container, styles.centeredState]}>
				<Text style={styles.stateText}>{vm.error}</Text>
			</View>
		);
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
						subtitle={heroSubtitle}
						title={heroTitle}
					/>

					<CatalogContentShell topBackgroundSource={vm.assets.whiteShell} logoSource={vm.assets.logo}>
						<View style={[styles.controls, { paddingHorizontal: horizontalPadding }]}>
							<SearchBar
								iconSource={vm.assets.searchIcon}
								onChangeText={vm.setQuery}
								placeholder=""
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
								<CatalogRail items={firstSection.items} title={firstSection.title} onItemPress={onItemPress} />
							) : null}
						</View>

						<View style={[styles.expansiveWhiteArea, { paddingHorizontal: horizontalPadding }]}>
							{remainingSections.map((section, index) => (
								<View key={section.id} style={index === 0 ? undefined : styles.sectionGap}>
									<CatalogRail items={section.items} title={section.title} onItemPress={onItemPress} />
								</View>
							))}
						</View>
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
		</View>
	);
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
	centeredState: {
		alignItems: 'center',
		justifyContent: 'center',
		paddingHorizontal: 24,
	},
	stateText: {
		color: '#303030',
		fontSize: 14,
		lineHeight: 20,
		textAlign: 'center',
	},
	heroArtworkOverlay: {
		zIndex: 50,
		elevation: 50,
	},
});
