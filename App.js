import React from 'react';
import CadastroView from './src/frontend/screens/CadastroView';
import LoginView from './src/frontend/screens/LoginView';
import PreferenceSelectionScreen from './src/frontend/screens/PreferenceSelectionScreen';

export default function App() {
	const [screen, setScreen] = React.useState('login');

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