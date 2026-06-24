import React, { useEffect, useState } from 'react';
import {
  ActivityIndicator,
  Modal,
  Pressable,
  StyleSheet,
  Text,
  TextInput,
  View,
} from 'react-native';

import { colors, spacing, typography } from '@/frontend/theme';
import { atualizarPerfilUsuario } from '@/frontend/repositories/UserRepository';

type ProfileSheetProps = {
  accessToken?: string | null;
  email?: string;
  name: string;
  visible: boolean;
  onClose: () => void;
  onLogout: () => void;
  onProfileUpdated?: (name: string) => void;
};

export function ProfileSheet({
  accessToken,
  email,
  name,
  visible,
  onClose,
  onLogout,
  onProfileUpdated,
}: ProfileSheetProps) {
  const [editing, setEditing] = useState(false);
  const [draftName, setDraftName] = useState(name);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (visible) {
      setDraftName(name);
      setEditing(false);
      setError(null);
    }
  }, [name, visible]);

  async function handleSave() {
    try {
      setSaving(true);
      setError(null);
      const updated = await atualizarPerfilUsuario(accessToken, { nome: draftName.trim() });
      onProfileUpdated?.(updated.nome);
      setEditing(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Não foi possível editar o perfil.');
    } finally {
      setSaving(false);
    }
  }

  function handleLogout() {
    onClose();
    onLogout();
  }

  return (
    <Modal animationType="slide" transparent visible={visible} onRequestClose={onClose}>
      <View style={styles.modalRoot}>
        <Pressable accessibilityRole="button" onPress={onClose} style={styles.backdrop} />
        <View style={styles.sheet}>
          <View style={styles.handle} />

          <View style={styles.header}>
            <View style={styles.avatar}>
              <Text style={styles.avatarText}>{getInitial(name)}</Text>
            </View>
            <View style={styles.identity}>
              <Text style={styles.name} numberOfLines={1}>
                {name || 'Usuário'}
              </Text>
              <Text style={styles.email} numberOfLines={1}>
                {email || 'Conta Spotlight'}
              </Text>
            </View>
            <Pressable accessibilityRole="button" onPress={onClose} style={styles.closeButton}>
              <Text style={styles.closeText}>x</Text>
            </Pressable>
          </View>

          <View style={styles.divider} />

          {editing ? (
            <View style={styles.editBox}>
              <Text style={styles.inputLabel}>Nome</Text>
              <TextInput
                autoCapitalize="words"
                onChangeText={setDraftName}
                placeholder="Seu nome"
                placeholderTextColor="rgba(245,241,236,0.35)"
                style={styles.input}
                value={draftName}
              />
              {error ? <Text style={styles.error}>{error}</Text> : null}
              <View style={styles.editActions}>
                <Pressable
                  accessibilityRole="button"
                  disabled={saving}
                  onPress={() => setEditing(false)}
                  style={({ pressed }) => [styles.secondaryButton, pressed && styles.pressed]}
                >
                  <Text style={styles.secondaryLabel}>Cancelar</Text>
                </Pressable>
                <Pressable
                  accessibilityRole="button"
                  disabled={saving || !draftName.trim()}
                  onPress={handleSave}
                  style={({ pressed }) => [
                    styles.primaryButton,
                    pressed && styles.pressed,
                    (saving || !draftName.trim()) && styles.disabled,
                  ]}
                >
                  {saving ? <ActivityIndicator color="#111111" /> : <Text style={styles.primaryLabel}>Salvar</Text>}
                </Pressable>
              </View>
            </View>
          ) : (
            <View style={styles.actions}>
              <Pressable
                accessibilityRole="button"
                onPress={() => setEditing(true)}
                style={({ pressed }) => [styles.actionItem, pressed && styles.pressed]}
              >
                <View style={styles.iconBox}>
                  <Text style={styles.iconText}>E</Text>
                </View>
                <Text style={styles.actionLabel}>Editar perfil</Text>
                <Text style={styles.chevron}>{'>'}</Text>
              </Pressable>

              <Pressable
                accessibilityRole="button"
                onPress={handleLogout}
                style={({ pressed }) => [styles.actionItem, styles.logoutItem, pressed && styles.pressed]}
              >
                <View style={[styles.iconBox, styles.logoutIcon]}>
                  <Text style={[styles.iconText, styles.logoutText]}>S</Text>
                </View>
                <Text style={[styles.actionLabel, styles.logoutText]}>Sair</Text>
              </Pressable>
            </View>
          )}

          <Text style={styles.version}>Spotlight · v1.0</Text>
        </View>
      </View>
    </Modal>
  );
}

function getInitial(value: string) {
  return value.trim().charAt(0).toUpperCase() || 'U';
}

const sheetColors = {
  background: '#1a1a1a',
  line: '#2B2825',
  orange: '#FF5A1F',
  text: '#F5F1EC',
  muted: '#9A938A',
};

const styles = StyleSheet.create({
  modalRoot: {
    flex: 1,
    justifyContent: 'flex-end',
  },
  backdrop: {
    ...StyleSheet.absoluteFillObject,
    backgroundColor: 'rgba(0,0,0,0.55)',
  },
  sheet: {
    backgroundColor: sheetColors.background,
    borderTopLeftRadius: 24,
    borderTopRightRadius: 24,
    paddingBottom: spacing.xl,
    paddingHorizontal: spacing.xl,
    paddingTop: spacing.md,
  },
  handle: {
    alignSelf: 'center',
    backgroundColor: sheetColors.line,
    borderRadius: 4,
    height: 4,
    marginBottom: spacing.lg,
    width: 40,
  },
  header: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: spacing.md,
  },
  avatar: {
    alignItems: 'center',
    backgroundColor: sheetColors.orange,
    borderRadius: 28,
    height: 56,
    justifyContent: 'center',
    width: 56,
  },
  avatarText: {
    color: sheetColors.text,
    fontFamily: typography.fontFamily.bold,
    fontSize: 22,
    lineHeight: 26,
    letterSpacing: 0,
  },
  identity: {
    flex: 1,
  },
  name: {
    color: sheetColors.text,
    fontFamily: typography.fontFamily.bold,
    fontSize: 17,
    lineHeight: 22,
    letterSpacing: 0,
  },
  email: {
    color: sheetColors.muted,
    ...typography.caption,
    fontSize: 12,
    lineHeight: 16,
    letterSpacing: 0,
  },
  closeButton: {
    alignItems: 'center',
    borderColor: sheetColors.line,
    borderRadius: 15,
    borderWidth: 1,
    height: 30,
    justifyContent: 'center',
    width: 30,
  },
  closeText: {
    color: sheetColors.muted,
    fontFamily: typography.fontFamily.bold,
    fontSize: 14,
    lineHeight: 16,
  },
  divider: {
    backgroundColor: sheetColors.line,
    height: 1,
    marginBottom: spacing.sm,
    marginTop: spacing.lg,
  },
  actions: {
    gap: spacing.xs,
  },
  actionItem: {
    alignItems: 'center',
    borderBottomColor: sheetColors.line,
    borderBottomWidth: 1,
    flexDirection: 'row',
    gap: spacing.md,
    minHeight: 64,
    paddingHorizontal: spacing.xs,
  },
  logoutItem: {
    borderBottomWidth: 0,
    marginTop: spacing.xs,
  },
  iconBox: {
    alignItems: 'center',
    backgroundColor: '#262220',
    borderRadius: 9,
    height: 34,
    justifyContent: 'center',
    width: 34,
  },
  logoutIcon: {
    backgroundColor: 'rgba(255,90,31,0.12)',
  },
  iconText: {
    color: sheetColors.text,
    fontFamily: typography.fontFamily.bold,
    fontSize: 13,
    letterSpacing: 0,
  },
  actionLabel: {
    color: sheetColors.text,
    flex: 1,
    fontFamily: typography.fontFamily.bold,
    fontSize: 14,
    lineHeight: 18,
    letterSpacing: 0,
  },
  chevron: {
    color: sheetColors.muted,
    fontFamily: typography.fontFamily.bold,
    fontSize: 18,
    lineHeight: 20,
  },
  logoutText: {
    color: sheetColors.orange,
  },
  editBox: {
    gap: spacing.sm,
    paddingTop: spacing.sm,
  },
  inputLabel: {
    color: sheetColors.muted,
    fontFamily: typography.fontFamily.bold,
    fontSize: 11,
    letterSpacing: 0,
    textTransform: 'uppercase',
  },
  input: {
    borderBottomColor: sheetColors.line,
    borderBottomWidth: 1,
    color: sheetColors.text,
    fontFamily: typography.fontFamily.book,
    fontSize: 15,
    minHeight: 40,
    paddingHorizontal: 0,
    paddingVertical: spacing.xs,
  },
  error: {
    color: colors.primary,
    ...typography.caption,
    letterSpacing: 0,
  },
  editActions: {
    flexDirection: 'row',
    gap: spacing.sm,
    marginTop: spacing.sm,
  },
  primaryButton: {
    alignItems: 'center',
    backgroundColor: sheetColors.text,
    borderRadius: 8,
    flex: 1,
    justifyContent: 'center',
    minHeight: 44,
  },
  primaryLabel: {
    color: '#111111',
    fontFamily: typography.fontFamily.bold,
    fontSize: 13,
    letterSpacing: 0,
  },
  secondaryButton: {
    alignItems: 'center',
    borderColor: sheetColors.line,
    borderRadius: 8,
    borderWidth: 1,
    flex: 1,
    justifyContent: 'center',
    minHeight: 44,
  },
  secondaryLabel: {
    color: sheetColors.text,
    fontFamily: typography.fontFamily.bold,
    fontSize: 13,
    letterSpacing: 0,
  },
  disabled: {
    opacity: 0.64,
  },
  pressed: {
    opacity: 0.78,
  },
  version: {
    color: sheetColors.muted,
    fontFamily: typography.fontFamily.book,
    fontSize: 11,
    lineHeight: 14,
    letterSpacing: 0,
    marginTop: spacing.md,
    textAlign: 'center',
  },
});
