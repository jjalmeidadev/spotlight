import React from 'react';
import {
  ActivityIndicator,
  ImageBackground,
  KeyboardAvoidingView,
  Platform,
  Pressable,
  StyleSheet,
  Text,
  TextInput,
  View,
} from 'react-native';
import { Usuario } from '../models/Usuario';
import { useLoginViewModel } from '../viewmodels/useLoginViewModel';

type LoginViewProps = {
  onLogin?: (usuario: Usuario) => void;
  onCadastroPress?: () => void;
};

export default function LoginView({ onLogin, onCadastroPress }: LoginViewProps) {
  const { email, senha, loading, error, setEmail, setSenha, entrar } = useLoginViewModel(onLogin);

  return (
    <ImageBackground
      resizeMode="cover"
      source={require('../../../assets/images/login-bg.png')}
      style={styles.background}
    >
      <KeyboardAvoidingView
        behavior={Platform.OS === 'ios' ? 'padding' : undefined}
        style={styles.container}
      >
        <View style={styles.content}>
          <Text style={styles.title}>Login</Text>
          <Text style={styles.subtitle}>Viva novas experiências!</Text>

          <View style={styles.form}>
            <Text style={styles.label}>E-mail</Text>
            <TextInput
              autoCapitalize="none"
              keyboardType="email-address"
              onChangeText={setEmail}
              style={styles.input}
              value={email}
            />

            <Text style={styles.label}>Senha</Text>
            <TextInput
              onChangeText={setSenha}
              secureTextEntry
              style={styles.input}
              value={senha}
            />

            {error ? <Text style={styles.error}>{error}</Text> : null}

            <Pressable
              accessibilityRole="button"
              disabled={loading}
              onPress={entrar}
              style={({ pressed }) => [styles.button, pressed && styles.pressed, loading && styles.disabled]}
            >
              {loading ? <ActivityIndicator color="#111412" /> : <Text style={styles.buttonText}>Entrar</Text>}
            </Pressable>

            <Pressable accessibilityRole="button" onPress={onCadastroPress}>
              <Text style={styles.link}>Não possuo conta</Text>
            </Pressable>
          </View>
        </View>
      </KeyboardAvoidingView>
    </ImageBackground>
  );
}

const styles = StyleSheet.create({
  background: {
    flex: 1,
  },
  container: {
    flex: 1,
  },
  content: {
    flex: 1,
    paddingTop: 56,
    paddingHorizontal: 32,
  },
  title: {
    color: '#eef7f2',
    fontSize: 46,
    fontWeight: '800',
    letterSpacing: 0,
    lineHeight: 54,
    marginTop: 4,
  },
  subtitle: {
    color: '#eef7f2',
    fontSize: 16,
    lineHeight: 22,
  },
  form: {
    marginTop: 14,
    width: '55%',
    minWidth: 178,
    gap: 10,
  },
  label: {
    color: '#eef7f2',
    fontSize: 12,
    marginTop: 10,
  },
  input: {
    height: 22,
    borderRadius: 999,
    backgroundColor: '#eef7f2',
    color: '#111412',
    paddingHorizontal: 12,
    fontSize: 12,
  },
  error: {
    color: '#ffffff',
    fontSize: 12,
    lineHeight: 16,
  },
  button: {
    alignItems: 'center',
    justifyContent: 'center',
    minHeight: 24,
    borderRadius: 999,
    backgroundColor: '#eef7f2',
    marginTop: 36,
  },
  buttonText: {
    color: '#111412',
    fontSize: 11,
    fontWeight: '700',
  },
  disabled: {
    opacity: 0.7,
  },
  pressed: {
    opacity: 0.85,
  },
  link: {
    color: '#eef7f2',
    fontSize: 11,
    marginLeft: 8,
  },
});
