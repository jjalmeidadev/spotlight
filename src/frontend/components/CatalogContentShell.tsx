import React from 'react';
import { Image, ImageSourcePropType, StyleSheet, View } from 'react-native';
import { CurvedWhiteTop } from './CurvedWhiteTop';

export const CATALOG_WHITE_PLANE_TOP = 72;

type CatalogContentShellProps = {
	topBackgroundSource: ImageSourcePropType;
	logoSource: ImageSourcePropType;
	children: React.ReactNode;
};

export function CatalogContentShell({ topBackgroundSource, logoSource, children }: CatalogContentShellProps) {
	void topBackgroundSource;

	return (
		<View style={styles.shell}>
			<View style={styles.whiteBackground} />
			<CurvedWhiteTop style={styles.topBackground} side="left" radius={180} />
			<View style={styles.content}>
				<View style={styles.logoRow}>
					<Image source={logoSource} style={styles.logo} />
				</View>
				{children}
			</View>
		</View>
	);
}

const styles = StyleSheet.create({
	shell: {
		backgroundColor: 'transparent',
		borderTopLeftRadius: 0,
		borderTopRightRadius: 0,
		marginTop: 80,
		overflow: 'visible',
		paddingBottom: 0,
		paddingTop: -20,
		position: 'relative',
		zIndex: 20,
	},
	whiteBackground: {
		backgroundColor: '#FFFFFF',
		bottom: 0,
		left: 0,
		position: 'absolute',
		right: 0,
		top: CATALOG_WHITE_PLANE_TOP,
		zIndex: 5,
	},
	topBackground: {
		height: 250,
		left: 0,
		position: 'absolute',
		resizeMode: 'cover',
		top: -158,
		width: '100%',
		zIndex: 10,
	},
	content: {
		backgroundColor: 'transparent',
		paddingBottom: 4,
		paddingTop: 0,
		marginTop: -150,
		position: 'relative',
		zIndex: 20,
	},
	logoRow: {
		alignSelf: 'flex-start',
		alignItems: 'center',
		marginBottom: 4,
		marginLeft: -18,
		width: '56%',
		zIndex: 26,
	},
	logo: {
		height: 124,
		width: '100%',
		resizeMode: 'contain',
	},

});
