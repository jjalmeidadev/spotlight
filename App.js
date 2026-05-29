import React from 'react';
import CadastroView from './src/frontend/screens/CadastroView';
import LoadingScreen from './src/frontend/screens/LoadingScreen';
import LoginView from './src/frontend/screens/LoginView';
import PreferenceSelectionScreen from './src/frontend/screens/PreferenceSelectionScreen';

export default function App() {
	// Inicia no splash; quando a checagem da sessão termina, vai pro login.
	const [screen, setScreen] = React.useState('loading');

	if (screen === 'loading') {
		return (
			<LoadingScreen
				onFinish={(status) => {
					// Hoje a checagem sempre cai em 'unauthenticated' (mock no viewmodel).
					// Quando integrar o JWT salvo, manda pra 'home' quando status === 'authenticated'.
					if (status === 'authenticated') {
						setScreen('preferencias');
					} else {
						setScreen('login');
					}
				}}
			/>
		);
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

	return (
		<LoginView
			onCadastroPress={() => setScreen('cadastro')}
			onLogin={() => setScreen('preferencias')}
		/>
	);
}
