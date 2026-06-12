import React, { useMemo, useState } from 'react';
import { Image, ImageSourcePropType, Pressable, StyleSheet, Text, View } from 'react-native';
import { colors, spacing, typography } from '@/frontend/theme';

type FilterDropdownProps = {
	iconSource: ImageSourcePropType;
	options: readonly string[];
	value: string | null;
	onChange: (value: string | null) => void;
};

export function FilterDropdown<TOption extends string>({
	iconSource,
	options,
	value,
	onChange,
}: {
	iconSource: ImageSourcePropType;
	options: readonly TOption[];
	value: TOption | null;
	onChange: (value: TOption | null) => void;
}) {
	const [open, setOpen] = useState(false);
	const label = useMemo(() => value ?? 'Filtrar por', [value]);

	return (
		<View style={styles.wrapper}>
			<Pressable onPress={() => setOpen((current) => !current)} style={styles.button} accessibilityRole="button">
				<Image source={iconSource} style={styles.icon} />
				<Text style={styles.buttonLabel}>{label}</Text>
				<View style={[styles.chevron, open && styles.chevronOpen]} />
				<View style={[styles.chevronSecond, open && styles.chevronSecondOpen]} />
			</Pressable>
			{open ? (
				<View style={styles.menu}>
					{options.map((option) => (
						<Pressable
							key={option}
							onPress={() => {
								onChange(value === option ? null : option);
								setOpen(false);
							}}
							style={({ pressed }) => [styles.menuItem, pressed && styles.menuItemPressed]}
						>
							<Text style={styles.menuItemText}>{option}</Text>
						</Pressable>
					))}
				</View>
			) : null}
		</View>
	);
}

const styles = StyleSheet.create({
	wrapper: {
		alignSelf: 'flex-start',
		position: 'relative',
		zIndex: 20,
	},
	button: {
		alignItems: 'center',
		alignSelf: 'flex-start',
		backgroundColor: colors.background,
		borderColor: '#c7c7c7',
		borderRadius: 10,
		borderWidth: 1.5,
		flexDirection: 'row',
		gap: 6,
		minHeight: 34,
		paddingLeft: 12,
		paddingRight: 28,
	},
	icon: {
		height: 17,
		width: 17,
		resizeMode: 'contain',
	},
	buttonLabel: {
		color: '#111412',
		...typography.button,
		fontSize: 13,
		lineHeight: 16,
	},
	chevron: {
		position: 'absolute',
		right: 12,
		top: 13,
		width: 9,
		height: 2,
		backgroundColor: '#666',
		transform: [{ rotate: '45deg' }],
	},
	chevronSecond: {
		position: 'absolute',
		right: 6,
		top: 13,
		width: 9,
		height: 2,
		backgroundColor: '#666',
		transform: [{ rotate: '-45deg' }],
	},
	chevronOpen: {
		top: 15,
	},
	chevronSecondOpen: {
		top: 15,
	},
	menu: {
		backgroundColor: '#f4fbf8',
		borderColor: '#c3c8c5',
		borderRadius: 10,
		borderWidth: 1,
		elevation: 4,
		marginTop: -1,
		overflow: 'hidden',
		position: 'absolute',
		top: 34,
		width: 156,
		zIndex: 30,
	},
	menuItem: {
		alignItems: 'center',
		backgroundColor: '#f4fbf8',
		borderBottomColor: '#d2d6d3',
		borderBottomWidth: 1,
		minHeight: 30,
		justifyContent: 'center',
		paddingHorizontal: 10,
	},
	menuItemPressed: {
		opacity: 0.85,
	},
	menuItemText: {
		color: '#111412',
		...typography.label,
		fontSize: 12,
		lineHeight: 14,
	},
});
