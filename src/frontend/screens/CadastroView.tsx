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
      <ScrollView
        contentContainerStyle={styles.scrollContent}
        keyboardShouldPersistTaps="handled"
        showsVerticalScrollIndicator={false}
      >
        <View style={styles.headerShape}>
          <Pressable
            accessibilityLabel="Voltar para o login"
            accessibilityRole="button"
            onPress={onBackPress}
            style={({ pressed }) => [styles.backButton, pressed && styles.pressed]}
          >
            <Text style={styles.backIcon}>‹</Text>
          </Pressable>
          <Text style={styles.title}>Cadastro</Text>
          <Text style={styles.subtitle}>Nos conte sobre você!</Text>
        </View>

        <View style={styles.form}>
          <Field label="Nome" onChangeText={vm.setNome} value={vm.nome} />
          <Field
            autoCapitalize="none"
            keyboardType="email-address"
            label="Seu melhor e-mail"
            onChangeText={vm.setEmail}
            value={vm.email}
          />
          <Field label="Senha" onChangeText={vm.setSenha} secureTextEntry value={vm.senha} />
          <Field
            keyboardType="number-pad"
            label="Data de nascimento"
            onChangeText={vm.setDataNascimento}
            placeholder="DD/MM/AAAA"
            placeholderTextColor="#a7afa9"
            value={vm.dataNascimento}
          />

          <Text style={styles.label}>Gênero</Text>
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

          <Text style={styles.label}>Compartilhar Localização?</Text>
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
            {vm.loading ? <ActivityIndicator color="#eef7f2" /> : <Text style={styles.primaryText}>Criar Conta</Text>}
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
      <TextInput style={styles.input} {...props} />
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
    backgroundColor: '#eef7f2',
  },
  scrollContent: {
    flexGrow: 1,
    paddingBottom: 40,
  },
  headerShape: {
    minHeight: 156,
    justifyContent: 'center',
    paddingHorizontal: 32,
    paddingTop: 24,
    backgroundColor: '#050505',
    borderBottomRightRadius: 220,
  },
  backButton: {
    alignItems: 'center',
    justifyContent: 'center',
    left: 24,
    minHeight: 44,
    minWidth: 44,
    position: 'absolute',
    top: 18,
  },
  backIcon: {
    color: '#eef7f2',
    fontSize: 42,
    fontWeight: '500',
    lineHeight: 44,
  },
  title: {
    color: '#eef7f2',
    fontSize: 50,
    fontWeight: '800',
    letterSpacing: 0,
    lineHeight: 58,
  },
  subtitle: {
    color: '#eef7f2',
    fontSize: 16,
    lineHeight: 22,
  },
  form: {
    alignSelf: 'center',
    width: '70%',
    maxWidth: 360,
    paddingTop: 26,
    gap: 16,
  },
  field: {
    gap: 8,
  },
  label: {
    color: '#202421',
    fontSize: 15,
    lineHeight: 18,
    textAlign: 'center',
  },
  input: {
    height: 22,
    borderRadius: 999,
    backgroundColor: '#303030',
    color: '#eef7f2',
    paddingHorizontal: 12,
    textAlign: 'center',
  },
  optionGrid: {
    gap: 8,
  },
  segmented: {
    flexDirection: 'row',
    gap: 28,
    justifyContent: 'center',
  },
  option: {
    alignItems: 'center',
    justifyContent: 'center',
    minHeight: 22,
    minWidth: 110,
    borderRadius: 999,
    backgroundColor: '#a3a3a3',
    paddingHorizontal: 12,
  },
  optionActive: {
    backgroundColor: '#303030',
  },
  optionText: {
    color: '#111412',
    fontSize: 11,
    fontWeight: '700',
  },
  optionTextActive: {
    color: '#eef7f2',
  },
  error: {
    color: '#c94f3a',
    fontSize: 12,
    lineHeight: 16,
    textAlign: 'center',
  },
  success: {
    color: '#2f6d45',
    fontSize: 12,
    lineHeight: 16,
    textAlign: 'center',
  },
  primaryButton: {
    alignItems: 'center',
    alignSelf: 'center',
    justifyContent: 'center',
    minHeight: 24,
    minWidth: 156,
    borderRadius: 999,
    backgroundColor: '#303030',
    marginTop: 10,
    paddingHorizontal: 18,
  },
  primaryText: {
    color: '#eef7f2',
    fontSize: 11,
    fontWeight: '700',
  },
  disabled: {
    opacity: 0.65,
  },
  pressed: {
    opacity: 0.84,
  },
});
