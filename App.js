import React from 'react';
import { Linking, Pressable, StyleSheet, Text, View } from 'react-native';

import AdminDashboardScreen from './src/frontend/screens/AdminDashboardScreen';
import CadastroView from './src/frontend/screens/CadastroView';
import DetalhesView from './src/frontend/screens/DetalhesView';
import LoginView from './src/frontend/screens/LoginView';
import PreferenceSelectionScreen from './src/frontend/screens/PreferenceSelectionScreen';

export default function App() {
	const [screen, setScreen] = React.useState('login');
	const [espetaculoId, setEspetaculoId] = React.useState('16');

	function handleLogin(usuario) {
		setScreen(usuario.tipoUsuario === 'administrador' ? 'dashboard' : 'preferencias');
	}

	if (screen === 'cadastro') {
		return (
			<CadastroView
				onBackPress={() => setScreen('login')}
				onCadastro={() => setScreen('preferencias')}
			/>
		);
	}

	if (screen === 'detalhes') {
		return (
			<DetalhesView
				espetaculoId={espetaculoId}
				onClose={() => setScreen('preferencias')}
				onEspetaculoSemelhantePress={(id) => setEspetaculoId(id)}
				onComprarIngresso={(link) => Linking.openURL(link)}
			/>
		);
	}

	if (screen === 'preferencias') {
		return (
			<View style={styles.preferenciasWrapper}>
				<PreferenceSelectionScreen />
				<Pressable
					accessibilityRole="button"
					onPress={() => setScreen('detalhes')}
					style={({ pressed }) => [styles.detalhesEntry, pressed && styles.detalhesEntryPressed]}
				>
					<Text style={styles.detalhesEntryLabel}>Ver detalhes do espetáculo</Text>
				</Pressable>
			</View>
		);
	}

	if (screen === 'dashboard') {
		return <AdminDashboardScreen />;
	}

	return (
		<LoginView
			onCadastroPress={() => setScreen('cadastro')}
			onLogin={handleLogin}
		/>
	);
}

const styles = StyleSheet.create({
	preferenciasWrapper: {
		flex: 1,
	},
	detalhesEntry: {
		position: 'absolute',
		left: 24,
		right: 24,
		bottom: 24,
		minHeight: 44,
		borderRadius: 999,
		alignItems: 'center',
		justifyContent: 'center',
		backgroundColor: '#eff4ef',
		paddingHorizontal: 16,
	},
	detalhesEntryPressed: {
		opacity: 0.85,
	},
	detalhesEntryLabel: {
		color: '#111412',
		fontSize: 12,
		fontWeight: '700',
	},
});
