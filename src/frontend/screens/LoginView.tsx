import { LinearGradient } from 'expo-linear-gradient';
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
import { Usuario } from '../models/User';
import { useLoginViewModel } from '../viewmodels/useLoginViewModel';

type LoginViewProps = {
  onLogin?: (usuario: Usuario) => void;
  onCadastroPress?: () => void;
  onDevModePress?: () => void;
};

const backgroundImage = require('../../../assets/images/login-bg.png');

export default function LoginView({ onLogin, onCadastroPress, onDevModePress }: LoginViewProps) {
  const { email, senha, loading, error, setEmail, setSenha, entrar } = useLoginViewModel(onLogin);

  return (
    <ImageBackground
      resizeMode="cover"
      source={backgroundImage}
      style={styles.background}
      imageStyle={styles.backgroundImage}
    >
      <LinearGradient
        colors={['rgba(0,0,0,0)', 'rgba(0,0,0,0.65)', 'rgba(0,0,0,0.96)']}
        locations={[0.3, 0.65, 1]}
        style={styles.gradient}
      />

      <KeyboardAvoidingView
        behavior={Platform.OS === 'ios' ? 'padding' : undefined}
        style={styles.container}
      >
        <View style={styles.content}>
          <Text style={styles.title}>Login</Text>

          <View style={styles.form}>
            <Field
              autoCapitalize="none"
              keyboardType="email-address"
              label="E-mail"
              onChangeText={setEmail}
              placeholder="seu@email.com"
              value={email}
            />

            <Field
              label="Senha"
              onChangeText={setSenha}
              placeholder="sua senha"
              secureTextEntry
              value={senha}
            />

            {error ? <Text style={styles.error}>{error}</Text> : null}

            <Pressable
              accessibilityRole="button"
              disabled={loading}
              onPress={entrar}
              style={({ pressed }) => [styles.button, pressed && styles.pressed, loading && styles.disabled]}
            >
              {loading ? <ActivityIndicator color="#111111" /> : <Text style={styles.buttonText}>Entrar</Text>}
            </Pressable>

            <View style={styles.linksRow}>
              <Text style={styles.linkText}>Não tem conta? </Text>
              <Pressable accessibilityRole="button" onPress={onCadastroPress} hitSlop={8}>
                <Text style={styles.link}>Cadastrar</Text>
              </Pressable>
            </View>

            {onDevModePress ? (
              <Pressable
                accessibilityRole="button"
                onPress={onDevModePress}
                style={({ pressed }) => [styles.devButton, pressed && styles.pressed]}
              >
                <Text style={styles.devButtonText}>Modo dev</Text>
              </Pressable>
            ) : null}
          </View>
        </View>
      </KeyboardAvoidingView>
    </ImageBackground>
  );
}

type FieldProps = React.ComponentProps<typeof TextInput> & {
  label: string;
};

function Field({ label, ...props }: FieldProps) {
  return (
    <View style={styles.field}>
      <Text style={styles.label}>{label}</Text>
      <TextInput
        placeholderTextColor="rgba(255,255,255,0.25)"
        style={styles.input}
        {...props}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  background: {
    flex: 1,
    backgroundColor: '#000000',
  },
  backgroundImage: {
    alignSelf: 'stretch',
  },
  gradient: {
    ...StyleSheet.absoluteFillObject,
  },
  container: {
    flex: 1,
  },
  content: {
    flex: 1,
    justifyContent: 'flex-end',
    paddingBottom: 22,
    paddingHorizontal: 20,
  },
  title: {
    color: '#FFFFFF',
    fontFamily: 'Gotham-Bold',
    fontSize: 32,
    fontWeight: '700',
    letterSpacing: 1.28,
    lineHeight: 38,
    marginBottom: 18,
  },
  form: {
    gap: 16,
    width: '100%',
  },
  field: {
    gap: 4,
  },
  label: {
    color: 'rgba(255,255,255,0.55)',
    fontFamily: 'Gotham-Book',
    fontSize: 11,
    fontWeight: '500',
    letterSpacing: 0.7,
    lineHeight: 14,
    textTransform: 'uppercase',
  },
  input: {
    borderBottomColor: 'rgba(255,255,255,0.4)',
    borderBottomWidth: 1,
    color: '#FFFFFF',
    fontFamily: 'Gotham-Book',
    fontSize: 15,
    fontStyle: 'italic',
    fontWeight: '300',
    minHeight: 34,
    paddingHorizontal: 0,
    paddingVertical: 6,
  },
  error: {
    color: '#FFFFFF',
    fontFamily: 'Gotham-Book',
    fontSize: 12,
    lineHeight: 16,
  },
  button: {
    alignItems: 'center',
    backgroundColor: '#FFFFFF',
    borderRadius: 6,
    height: 52,
    justifyContent: 'center',
    marginTop: 10,
    width: '100%',
  },
  buttonText: {
    color: '#111111',
    fontFamily: 'Gotham-Bold',
    fontSize: 13,
    fontWeight: '700',
    letterSpacing: 1.56,
    textTransform: 'uppercase',
  },
  disabled: {
    opacity: 0.72,
  },
  pressed: {
    opacity: 0.86,
  },
  linksRow: {
    alignItems: 'center',
    flexDirection: 'row',
    flexWrap: 'wrap',
    justifyContent: 'center',
    marginTop: 2,
  },
  linkText: {
    color: 'rgba(255,255,255,0.45)',
    fontFamily: 'Gotham-Book',
    fontSize: 11,
    lineHeight: 16,
  },
  link: {
    color: 'rgba(255,255,255,0.7)',
    fontFamily: 'Gotham-Book',
    fontSize: 11,
    lineHeight: 16,
    textDecorationLine: 'underline',
  },
  devButton: {
    alignItems: 'center',
    alignSelf: 'center',
    borderColor: 'rgba(255,255,255,0.18)',
    borderRadius: 6,
    borderWidth: 1,
    minHeight: 30,
    paddingHorizontal: 12,
    justifyContent: 'center',
  },
  devButtonText: {
    color: 'rgba(255,255,255,0.45)',
    fontFamily: 'Gotham-Book',
    fontSize: 10,
    letterSpacing: 0.8,
    lineHeight: 14,
    textTransform: 'uppercase',
  },
});
