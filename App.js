import React from 'react';
import { ActivityIndicator, StyleSheet, View } from 'react-native';
import { useFonts } from 'expo-font';
import CadastroView from './src/frontend/screens/CadastroView';
import CatalogView from './src/frontend/screens/CatalogView';
import LoginView from './src/frontend/screens/LoginView';

export default function App() {
	const [screen, setScreen] = React.useState('login');
	const [userName, setUserName] = React.useState('Leonardo');
	const [fontsLoaded] = useFonts({
		'Gotham-Bold': require('./assets/fonts/Gotham Bold.otf'),
		'Gotham-Medium': require('./assets/fonts/Gotham Medium.otf'),
		'Gotham-Book': require('./assets/fonts/Gotham Book.otf'),
	});

	if (!fontsLoaded) {
		return (
			<View style={styles.loading}>
				<ActivityIndicator color="#111412" />
			</View>
		);
	}

	if (screen === 'cadastro') {
		return <CadastroView onBackPress={() => setScreen('login')} onCadastro={() => setScreen('login')} />;
	}

	if (screen === 'catalogo') {
		return <CatalogView profileName={userName} />;
	}

	return (
		<LoginView
			onCadastroPress={() => setScreen('cadastro')}
			onLogin={(usuario) => {
				setUserName(usuario.nome);
				setScreen('catalogo');
			}}
		/>
	);
}

const styles = StyleSheet.create({
	loading: {
		flex: 1,
		alignItems: 'center',
		justifyContent: 'center',
		backgroundColor: '#f4fbf8',
	},
});
