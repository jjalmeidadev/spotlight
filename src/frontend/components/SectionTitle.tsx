import React from 'react';
import { StyleSheet, Text } from 'react-native';
import { typography } from '@/frontend/theme';

type SectionTitleProps = {
	title: string;
};

export function SectionTitle({ title }: SectionTitleProps) {
	return <Text style={styles.title}>{title}</Text>;
}

const styles = StyleSheet.create({
	title: {
		color: '#111412',
		...typography.title,
		fontSize: 20,
		lineHeight: 24,
	},
});