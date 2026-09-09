import React from 'react';
import { Image, ImageSourcePropType, StyleSheet, TextInput, TextInputProps, View } from 'react-native';
import { colors, spacing, typography } from '@/frontend/theme';

type SearchBarProps = TextInputProps & {
	iconSource: ImageSourcePropType;
};

export function SearchBar({ iconSource, style, ...props }: SearchBarProps) {
	return (
		<View style={styles.wrapper}>
			<TextInput
				placeholderTextColor={colors.textSubtle}
				style={[styles.input, style]}
				{...props}
			/>
			<Image source={iconSource} style={styles.icon} />
		</View>
	);
}

const styles = StyleSheet.create({
	wrapper: {
		width: '56%',
		justifyContent: 'center',
		alignItems: 'flex-start',
		alignSelf: 'flex-start',
		marginLeft: 1,
	},
	input: {
		width: '100%',
		height: 28,
		borderRadius: 999,
		backgroundColor: '#343434',
		color: colors.background,
		paddingLeft: 14,
		paddingRight: 52,
		...typography.body,
	},
	icon: {
		position: 'absolute',
		right: 14,
		width: 20,
		height: 20,
		resizeMode: 'contain',
	},
});