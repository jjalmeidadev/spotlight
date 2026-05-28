import React from 'react';
import AdminDashboardScreen from './src/frontend/screens/AdminDashboardScreen';
import CadastroView from './src/frontend/screens/CadastroView';
import LoginView from './src/frontend/screens/LoginView';
import PreferenceSelectionScreen from './src/frontend/screens/PreferenceSelectionScreen';

export default function App() {
	const [screen, setScreen] = React.useState('login');

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

	if (screen === 'preferencias') {
		return <PreferenceSelectionScreen />;
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
