import React from 'react';
import { ActivityIndicator, Alert, Linking, Pressable, StyleSheet, Text, View } from 'react-native';
import { useFonts } from 'expo-font';
import { SafeAreaProvider } from 'react-native-safe-area-context';

import AdminDashboardScreen from './src/frontend/screens/AdminDashboardScreen';
import SignupView from './src/frontend/screens/SignupView';
import CatalogView from './src/frontend/screens/CatalogView';
import DetailsView from './src/frontend/screens/DetailsView';
import LoadingScreen from './src/frontend/screens/LoadingScreen';
import LoginView from './src/frontend/screens/LoginView';
import PreferenceSelectionScreen from './src/frontend/screens/PreferenceSelectionScreen';
import { useTelemetrySession } from './src/frontend/hooks/useTelemetrySession';
import {
	obterPreferenciasUsuario,
	preferenciasEstaoCompletas,
} from './src/frontend/repositories/PreferenceRepository';

export default function App() {
	return (
		<SafeAreaProvider>
			<AppContent />
		</SafeAreaProvider>
	);
}

function AppContent() {
	const [screen, setScreen] = React.useState('loading');
	const [espetaculoId, setEspetaculoId] = React.useState('16');
	const [userName, setUserName] = React.useState('Leonardo');
	const [userEmail, setUserEmail] = React.useState('');
	const [userType, setUserType] = React.useState('cadastrado');
	const [accessToken, setAccessToken] = React.useState(null);
	const [userLocationAddress, setUserLocationAddress] = React.useState(null);

	const [fontsLoaded] = useFonts({
		'Gotham-Bold': require('./assets/fonts/Gotham Bold.otf'),
		'Gotham-Medium': require('./assets/fonts/Gotham Medium.otf'),
		'Gotham-Book': require('./assets/fonts/Gotham Book.otf'),
	});

	useTelemetrySession({ screen: getTelemetryScreenName(screen), accessToken });

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

	async function handleLogin(usuario) {
		setUserName(usuario.nome);
		setUserEmail(usuario.email);
		setUserType(usuario.tipoUsuario ?? 'cadastrado');
		setAccessToken(usuario.accessToken ?? null);
		setUserLocationAddress(usuario.localizacao?.formattedAddress ?? null);
		await direcionarAposAutenticacao(usuario);
	}

	function handleLogout() {
		setAccessToken(null);
		setUserName('');
		setUserEmail('');
		setUserType('cadastrado');
		setUserLocationAddress(null);
		setScreen('login');
	}

	async function direcionarAposAutenticacao(usuario) {
		if (usuario.tipoUsuario === 'administrador') {
			setScreen('dashboard');
			return;
		}

		if (!usuario.accessToken) {
			setScreen('preferencias');
			return;
		}

		setScreen('verificando-preferencias');

		try {
			const preferencias = await obterPreferenciasUsuario(usuario.accessToken);
			setScreen(preferenciasEstaoCompletas(preferencias) ? 'catalogo' : 'preferencias');
		} catch (error) {
			console.log('Nao foi possivel verificar preferencias salvas:', error);
			setScreen('catalogo');
		}
	}

	if (screen === 'loading') {
		return (
			<LoadingScreen
				onFinish={async (status, usuario) => {
					if (status === 'authenticated' && usuario) {
						await handleLogin(usuario);
						return;
					}

					setScreen('login');
				}}
			/>
		);
	}

	if (!fontsLoaded) {
		return (
			<View style={styles.loading}>
				<ActivityIndicator color="#111412" />
			</View>
		);
	}

	if (screen === 'cadastro') {
		return (
			<SignupView
				onBackPress={() => setScreen('login')}
				onCadastro={async (usuario) => {
					if (usuario.requerConfirmacaoEmail) {
						setScreen('login');
						Alert.alert(
							'Confirme seu e-mail',
							'Enviamos um link de confirmação para o seu e-mail. Confirme a conta antes de fazer login.',
						);
						return;
					}

					setUserName(usuario.nome);
					setUserEmail(usuario.email);
					setUserType(usuario.tipoUsuario ?? 'cadastrado');
					setAccessToken(usuario.accessToken ?? null);
					setUserLocationAddress(usuario.localizacao?.formattedAddress ?? null);
					await direcionarAposAutenticacao(usuario);
				}}
			/>
		);
	}

	if (screen === 'preferencias') {
		return <PreferenceSelectionScreen accessToken={accessToken} onComplete={() => setScreen('catalogo')} />;
	}

	if (screen === 'verificando-preferencias') {
		return (
			<View style={styles.loading}>
				<ActivityIndicator color="#111412" />
			</View>
		);
	}

	if (screen === 'catalogo') {
		return (
			<CatalogView
				accessToken={accessToken}
				profileEmail={userEmail}
				profileName={userName}
				isAdmin={userType === 'administrador'}
				userLocationAddress={userLocationAddress}
				onAdminPress={() => setScreen('dashboard')}
				onLogout={handleLogout}
				onProfileUpdated={setUserName}
				onItemPress={(item) => {
					setEspetaculoId(item.id);
					setScreen('detalhes');
				}}
			/>
		);
	}

	if (screen === 'detalhes') {
		return (
			<DetailsView
				accessToken={accessToken}
				espetaculoId={espetaculoId}
				onClose={() => setScreen('catalogo')}
				onEspetaculoSemelhantePress={(id) => setEspetaculoId(id)}
				onComprarIngresso={(link) => Linking.openURL(link)}
			/>
		);
	}

	if (screen === 'dashboard') {
		return (
			<AdminDashboardScreen
				accessToken={accessToken}
				profileEmail={userEmail}
				profileName={userName}
				onOpenCatalog={() => setScreen('catalogo')}
				onLogout={handleLogout}
				onProfileUpdated={setUserName}
			/>
		);
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

	return (
		<LoginView
			onCadastroPress={() => setScreen('cadastro')}
			onLogin={handleLogin}
		/>
	);
}

function getTelemetryScreenName(screen) {
	const screenNames = {
		cadastro: 'CadastroView',
		catalogo: 'CatalogView',
		dashboard: 'AdminDashboardScreen',
		detalhes: 'DetalhesView',
		'email-confirmado': 'EmailConfirmedView',
		loading: 'LoadingScreen',
		login: 'LoginView',
		preferencias: 'PreferenceSelectionScreen',
		'verificando-preferencias': 'PreferenceCheckView',
	};

	return screenNames[screen] ?? screen;
}

const styles = StyleSheet.create({
	loading: {
		alignItems: 'center',
		backgroundColor: '#eef7f2',
		flex: 1,
		justifyContent: 'center',
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
