import React from 'react';
import { ActivityIndicator, Alert, Linking, Pressable, StyleSheet, Text, View } from 'react-native';
import { useFonts } from 'expo-font';

import AdminDashboardScreen from './src/frontend/screens/AdminDashboardScreen';
import CadastroView from './src/frontend/screens/CadastroView';
import CatalogView from './src/frontend/screens/CatalogView';
import DetalhesView from './src/frontend/screens/DetalhesView';
import LoginView from './src/frontend/screens/LoginView';
import PreferenceSelectionScreen from './src/frontend/screens/PreferenceSelectionScreen';

export default function App() {
	const [screen, setScreen] = React.useState('login');
	const [espetaculoId, setEspetaculoId] = React.useState('16');
	const [userName, setUserName] = React.useState('Leonardo');

	const [fontsLoaded] = useFonts({
		'Gotham-Bold': require('./assets/fonts/Gotham Bold.otf'),
		'Gotham-Medium': require('./assets/fonts/Gotham Medium.otf'),
		'Gotham-Book': require('./assets/fonts/Gotham Book.otf'),
	});

	React.useEffect(() => {
		function handleUrl(url) {
			if (url.includes('email-confirmado')) {
				setScreen('email-confirmado');
			}
		}

		Linking.getInitialURL().then((url) => {
			if (url) {
				handleUrl(url);
			}
		});

		const subscription = Linking.addEventListener('url', ({ url }) => handleUrl(url));
		return () => subscription.remove();
	}, []);

	if (!fontsLoaded) {
		return (
			<View style={styles.loading}>
				<ActivityIndicator color="#111412" />
			</View>
		);
	}

	function handleLogin(usuario) {
		setUserName(usuario.nome);
		setScreen(usuario.tipoUsuario === 'administrador' ? 'dashboard' : 'preferencias');
	}


	if (screen === 'cadastro') {
		return (
			<CadastroView
				onBackPress={() => setScreen('login')}
				onCadastro={(usuario) => {
					if (usuario.requerConfirmacaoEmail) {
						setScreen('login');
						Alert.alert(
							'Confirme seu e-mail',
							'Enviamos um link de confirmação para o seu e-mail. Confirme a conta antes de fazer login.',
						);
						return;
					}

					setScreen('preferencias');
				}}
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

	if (screen === 'email-confirmado') {
		return (
			<View style={styles.confirmedContainer}>
				<Text style={styles.confirmedTitle}>E-mail confirmado</Text>
				<Text style={styles.confirmedText}>Sua conta foi confirmada. Volte para o login para entrar.</Text>
				<Pressable
					accessibilityRole="button"
					onPress={() => setScreen('login')}
					style={({ pressed }) => [styles.confirmedButton, pressed && styles.pressed]}
				>
					<Text style={styles.confirmedButtonText}>Voltar ao login</Text>
				</Pressable>
			</View>
		);
	}

	if (screen === 'catalogo') {
		return <CatalogView profileName={userName} />;
	}

	return (
		<LoginView
			onCadastroPress={() => setScreen('cadastro')}
			onLogin={handleLogin}
		/>
	);
}

const styles = StyleSheet.create({
	loading: {
		alignItems: 'center',
		backgroundColor: '#eef7f2',
		flex: 1,
		justifyContent: 'center',
	},
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
	confirmedContainer: {
		alignItems: 'center',
		backgroundColor: '#eef7f2',
		flex: 1,
		justifyContent: 'center',
		paddingHorizontal: 32,
	},
	confirmedTitle: {
		color: '#111412',
		fontSize: 34,
		fontWeight: '800',
		lineHeight: 40,
		textAlign: 'center',
	},
	confirmedText: {
		color: '#303030',
		fontSize: 15,
		lineHeight: 22,
		marginTop: 12,
		textAlign: 'center',
	},
	confirmedButton: {
		alignItems: 'center',
		backgroundColor: '#303030',
		borderRadius: 999,
		justifyContent: 'center',
		marginTop: 28,
		minHeight: 36,
		minWidth: 160,
		paddingHorizontal: 18,
	},
	confirmedButtonText: {
		color: '#eef7f2',
		fontSize: 13,
		fontWeight: '700',
	},
	pressed: {
		opacity: 0.84,
	},
});
