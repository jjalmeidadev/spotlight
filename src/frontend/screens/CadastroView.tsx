import React from 'react';
import {
  ActivityIndicator,
  KeyboardAvoidingView,
  Platform,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  View,
} from 'react-native';
import { Genero, Usuario } from '../models/Usuario';
import { useCadastroViewModel } from '../viewmodels/useCadastroViewModel';

type CadastroViewProps = {
  onCadastro?: (usuario: Usuario) => void;
  onBackPress?: () => void;
};

export default function CadastroView({ onCadastro, onBackPress }: CadastroViewProps) {
  const vm = useCadastroViewModel(onCadastro);

  return (
    <KeyboardAvoidingView
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      style={styles.container}
    >
      <View style={styles.header}>
        <Text style={styles.title}>Cadastro</Text>
        <Text style={styles.subtitle}>Preencha seus dados para continuar</Text>
      </View>

      <ScrollView
        contentContainerStyle={styles.scrollContent}
        keyboardShouldPersistTaps="handled"
        showsVerticalScrollIndicator={false}
      >
        <View style={styles.form}>
          <Field
            label="Nome completo"
            onChangeText={vm.setNome}
            placeholder="seu nome"
            value={vm.nome}
          />
          <Field
            autoCapitalize="none"
            keyboardType="email-address"
            label="E-mail"
            onChangeText={vm.setEmail}
            placeholder="seu@email.com"
            value={vm.email}
          />
          <Field
            keyboardType="number-pad"
            label="Data de nascimento"
            onChangeText={vm.setDataNascimento}
            placeholder="DD/MM/AAAA"
            value={vm.dataNascimento}
          />
          <Field
            label="Senha"
            onChangeText={vm.setSenha}
            placeholder="sua senha"
            secureTextEntry
            value={vm.senha}
          />

          <Text style={styles.groupLabel}>Gênero</Text>
          <View style={styles.optionGrid}>
            {vm.generos.map((genero) => (
              <Option
                active={vm.genero === genero}
                key={genero}
                label={genero}
                onPress={() => vm.setGenero(genero)}
              />
            ))}
          </View>

          <Text style={styles.groupLabel}>Compartilhar localização?</Text>
          <View style={styles.segmented}>
            <Option
              active={vm.compartilharLocalizacao}
              label="Sim"
              onPress={() => vm.setCompartilharLocalizacao(true)}
            />
            <Option
              active={!vm.compartilharLocalizacao}
              label="Não"
              onPress={() => vm.setCompartilharLocalizacao(false)}
            />
          </View>

          {vm.error ? <Text style={styles.error}>{vm.error}</Text> : null}
          {vm.success ? <Text style={styles.success}>{vm.success}</Text> : null}

          <Pressable
            accessibilityRole="button"
            disabled={vm.loading}
            onPress={vm.criarConta}
            style={({ pressed }) => [
              styles.primaryButton,
              pressed && styles.pressed,
              vm.loading && styles.disabled,
            ]}
          >
            {vm.loading ? <ActivityIndicator color="#FFFFFF" /> : <Text style={styles.primaryText}>Criar conta</Text>}
          </Pressable>

          <Pressable accessibilityRole="button" onPress={onBackPress} style={styles.loginLinkButton}>
            <Text style={styles.footerText}>
              Já tem conta? <Text style={styles.footerLink}>Entrar</Text>
            </Text>
          </Pressable>
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
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
        placeholderTextColor="rgba(255,255,255,0.18)"
        style={styles.input}
        {...props}
      />
    </View>
  );
}

type OptionProps = {
  active: boolean;
  label: Genero | 'Sim' | 'Não';
  onPress: () => void;
};

function Option({ active, label, onPress }: OptionProps) {
  return (
    <Pressable
      accessibilityRole="button"
      onPress={onPress}
      style={({ pressed }) => [styles.option, active && styles.optionActive, pressed && styles.pressed]}
    >
      <Text style={[styles.optionText, active && styles.optionTextActive]} numberOfLines={1}>
        {label}
      </Text>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#0A0A0A',
  },
  header: {
    backgroundColor: '#E8420A',
    justifyContent: 'flex-end',
    minHeight: 112,
    paddingBottom: 18,
    paddingHorizontal: 16,
    paddingTop: 34,
  },
  title: {
    color: '#FFFFFF',
    fontFamily: 'Gotham-Bold',
    fontSize: 26,
    fontWeight: '700',
    letterSpacing: 1.32,
    lineHeight: 32,
    textTransform: 'uppercase',
  },
  subtitle: {
    color: 'rgba(255,255,255,0.6)',
    fontFamily: 'Gotham-Book',
    fontSize: 11,
    fontWeight: '300',
    lineHeight: 16,
  },
  scrollContent: {
    flexGrow: 1,
    paddingBottom: 20,
    paddingHorizontal: 16,
    paddingTop: 16,
  },
  form: {
    flexGrow: 1,
    gap: 14,
    justifyContent: 'flex-end',
  },
  field: {
    gap: 3,
  },
  label: {
    color: 'rgba(255,255,255,0.35)',
    fontFamily: 'Gotham-Book',
    fontSize: 10,
    fontWeight: '500',
    letterSpacing: 0.64,
    lineHeight: 13,
    textTransform: 'uppercase',
  },
  input: {
    borderBottomColor: 'rgba(255,255,255,0.12)',
    borderBottomWidth: 1,
    color: '#FFFFFF',
    fontFamily: 'Gotham-Book',
    fontSize: 14,
    fontStyle: 'italic',
    fontWeight: '300',
    minHeight: 36,
    paddingHorizontal: 0,
    paddingVertical: 6,
  },
  groupLabel: {
    color: 'rgba(255,255,255,0.35)',
    fontFamily: 'Gotham-Book',
    fontSize: 10,
    fontWeight: '500',
    letterSpacing: 0.64,
    lineHeight: 13,
    marginTop: 2,
    textTransform: 'uppercase',
  },
  optionGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
  },
  segmented: {
    flexDirection: 'row',
    gap: 8,
  },
  option: {
    alignItems: 'center',
    borderColor: 'rgba(255,255,255,0.12)',
    borderRadius: 6,
    borderWidth: 1,
    justifyContent: 'center',
    minHeight: 32,
    paddingHorizontal: 12,
  },
  optionActive: {
    backgroundColor: '#E8420A',
    borderColor: '#E8420A',
  },
  optionText: {
    color: 'rgba(255,255,255,0.55)',
    fontFamily: 'Gotham-Book',
    fontSize: 12,
    fontWeight: '500',
  },
  optionTextActive: {
    color: '#FFFFFF',
  },
  error: {
    color: '#FFFFFF',
    fontFamily: 'Gotham-Book',
    fontSize: 12,
    lineHeight: 16,
  },
  success: {
    color: '#FFFFFF',
    fontFamily: 'Gotham-Book',
    fontSize: 12,
    lineHeight: 16,
  },
  primaryButton: {
    alignItems: 'center',
    backgroundColor: '#E8420A',
    borderRadius: 6,
    height: 52,
    justifyContent: 'center',
    marginTop: 'auto',
    width: '100%',
  },
  primaryText: {
    color: '#FFFFFF',
    fontFamily: 'Gotham-Bold',
    fontSize: 13,
    fontWeight: '700',
    letterSpacing: 1.56,
    textTransform: 'uppercase',
  },
  loginLinkButton: {
    alignItems: 'center',
    minHeight: 22,
  },
  footerText: {
    color: 'rgba(255,255,255,0.45)',
    fontFamily: 'Gotham-Book',
    fontSize: 11,
    lineHeight: 16,
    textAlign: 'center',
  },
  footerLink: {
    color: 'rgba(255,255,255,0.7)',
    textDecorationLine: 'underline',
  },
  disabled: {
    opacity: 0.72,
  },
  pressed: {
    opacity: 0.84,
  },
});
