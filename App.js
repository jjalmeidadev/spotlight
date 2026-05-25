import React from 'react';
import { Alert } from 'react-native';
import CadastroView from './src/frontend/screens/CadastroView';
import LoginView from './src/frontend/screens/LoginView';

export default function App() {
	const [screen, setScreen] = React.useState('login');

	if (screen === 'cadastro') {
		return <CadastroView onBackPress={() => setScreen('login')} onCadastro={() => setScreen('login')} />;
	}

	return (
		<LoginView
			onCadastroPress={() => setScreen('cadastro')}
			onLogin={() => Alert.alert('Login realizado', 'Bem-vinda ao Aventura!')}
		/>
	);
}
