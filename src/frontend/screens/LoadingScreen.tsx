import React, { useEffect, useRef, useState } from 'react';
import {
  Animated,
  Easing,
  Image,
  SafeAreaView,
  StatusBar,
  StyleSheet,
  useWindowDimensions,
  View,
} from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';

import { colors, spacing } from '@/frontend/theme';
import {
  SessionStatus,
  useLoadingScreenViewModel,
} from '@/frontend/viewmodels/useLoadingScreenViewModel';

const logoAventura = require('../../../assets/images/logo-aventura-branca.png');

/**
 * Paleta das cortinas — alternando luz/sombra simula pregas verticais de veludo.
 * Cores são derivadas do colors.primary (#ff5a2f) clareando e escurecendo.
 */
const PLEAT_HIGHLIGHT = '#ff7a50';
const PLEAT_BASE = colors.primary;
const PLEAT_SHADOW = '#b03b1f';
const PLEAT_DEEP_EDGE = '#5a1c0f';
const PLEAT_MEETING_EDGE = '#3a0f08';

/** 4 pregas com sombra profunda no encontro central (lado direito da cortina esquerda). */
const PLEAT_COLORS_ESQ = [
  PLEAT_DEEP_EDGE,
  PLEAT_HIGHLIGHT,
  PLEAT_SHADOW,
  PLEAT_HIGHLIGHT,
  PLEAT_SHADOW,
  PLEAT_HIGHLIGHT,
  PLEAT_SHADOW,
  PLEAT_BASE,
  PLEAT_MEETING_EDGE,
] as const;

/** Espelhado: sombra profunda no encontro (lado esquerdo da cortina direita). */
const PLEAT_COLORS_DIR = [
  PLEAT_MEETING_EDGE,
  PLEAT_BASE,
  PLEAT_SHADOW,
  PLEAT_HIGHLIGHT,
  PLEAT_SHADOW,
  PLEAT_HIGHLIGHT,
  PLEAT_SHADOW,
  PLEAT_HIGHLIGHT,
  PLEAT_DEEP_EDGE,
] as const;

/** Tempo (ms) que a logo fica visível após entrar antes de chamar onFinish. */
const MS_ADMIRACAO = 900;
/** Duração (ms) do fade-in + scale da logo. */
const MS_LOGO_ENTRA = 650;

/** Fases da animação — separadas da checagem real da sessão pra orquestrar a coreografia. */
type Fase = 'checking' | 'opening' | 'revealing' | 'done';

interface LoadingScreenProps {
  /**
   * Chamado uma única vez quando a coreografia termina, com o status da sessão.
   * O consumidor (root do app) decide pra qual rota navegar.
   */
  onFinish?: (status: SessionStatus) => void;
}

/**
 * Splash com encenação de teatro:
 *  1. Cortinas laranja fechadas + 3 dots brancos pulsando enquanto a sessão é verificada;
 *  2. Cortinas abrem pros lados revelando o palco escuro;
 *  3. Logo da Aventura aparece com fade + scale suave;
 *  4. Pausa curta e chama <c>onFinish</c>.
 *
 * Animações usam a API <c>Animated</c> nativa (sem Reanimated).
 */
export default function LoadingScreen({ onFinish }: LoadingScreenProps) {
  const viewModel = useLoadingScreenViewModel();
  const { width: larguraTela } = useWindowDimensions();

  const [fase, setFase] = useState<Fase>('checking');

  // Dots: 3 valores em loop escalonado (visíveis só na fase "checking").
  const dot1 = useRef(new Animated.Value(0)).current;
  const dot2 = useRef(new Animated.Value(0)).current;
  const dot3 = useRef(new Animated.Value(0)).current;

  // Cortinas: 0 = fechadas (cobrem o palco), 1 = abertas (saíram pras laterais).
  const cortinasAbertura = useRef(new Animated.Value(0)).current;

  // Entrada da logo: 0 = invisível, 1 = totalmente visível e no tamanho final.
  const logoEntrada = useRef(new Animated.Value(0)).current;

  const timeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  // 1) Dots pulsando enquanto checking
  useEffect(() => {
    if (fase !== 'checking') return;
    const ciclo = Animated.parallel([
      criarPulsoEmLoop(dot1, 0),
      criarPulsoEmLoop(dot2, 180),
      criarPulsoEmLoop(dot3, 360),
    ]);
    ciclo.start();
    return () => ciclo.stop();
  }, [fase, dot1, dot2, dot3]);

  // 2) Quando a checagem real termina, dispara a abertura das cortinas
  useEffect(() => {
    if (viewModel.status !== 'checking' && fase === 'checking') {
      setFase('opening');
    }
  }, [viewModel.status, fase]);

  // 3) Coreografia da abertura das cortinas
  useEffect(() => {
    if (fase !== 'opening') return;
    Animated.timing(cortinasAbertura, {
      toValue: 1,
      duration: 900,
      easing: Easing.inOut(Easing.cubic),
      useNativeDriver: true,
    }).start(({ finished }) => {
      if (finished) setFase('revealing');
    });
  }, [fase, cortinasAbertura]);

  // 4) Logo da Aventura entrando com fade + scale, depois aguarda admiração e finaliza
  useEffect(() => {
    if (fase !== 'revealing') return;
    Animated.timing(logoEntrada, {
      toValue: 1,
      duration: MS_LOGO_ENTRA,
      easing: Easing.out(Easing.cubic),
      useNativeDriver: true,
    }).start(({ finished }) => {
      if (!finished) return;
      timeoutRef.current = setTimeout(() => setFase('done'), MS_ADMIRACAO);
    });
    return () => {
      if (timeoutRef.current) clearTimeout(timeoutRef.current);
    };
  }, [fase, logoEntrada]);

  // 5) Fim da coreografia
  useEffect(() => {
    if (fase === 'done') onFinish?.(viewModel.status);
  }, [fase, onFinish, viewModel.status]);

  // Cada cortina cobre metade da tela e desliza pra fora (esquerda → -W/2, direita → +W/2).
  const metade = larguraTela / 2;
  const cortinaEsquerdaStyle = {
    transform: [
      {
        translateX: cortinasAbertura.interpolate({
          inputRange: [0, 1],
          outputRange: [0, -metade],
        }),
      },
    ],
  };
  const cortinaDireitaStyle = {
    transform: [
      {
        translateX: cortinasAbertura.interpolate({
          inputRange: [0, 1],
          outputRange: [0, metade],
        }),
      },
    ],
  };

  const logoStyle = {
    opacity: logoEntrada,
    transform: [
      { scale: logoEntrada.interpolate({ inputRange: [0, 1], outputRange: [0.85, 1] }) },
    ],
  };

  const dotsVisiveis = fase === 'checking';
  const palcoVisivel = fase === 'revealing' || fase === 'done';

  return (
    <SafeAreaView style={styles.safeArea}>
      <StatusBar barStyle="light-content" backgroundColor={colors.surface} />

      {/* PALCO — fundo escuro com a logo aparecendo (só visível quando cortinas abrem) */}
      <View style={styles.palco}>
        {palcoVisivel && (
          <Animated.Image
            source={logoAventura}
            resizeMode="contain"
            accessibilityLabel="Aventura"
            style={[styles.logo, logoStyle]}
          />
        )}
      </View>

      {/* CORTINAS — duas metades laterais com pregas verticais (gradient) que abrem do centro pros lados */}
      <Animated.View
        pointerEvents="none"
        style={[styles.cortinaEsquerda, cortinaEsquerdaStyle]}
      >
        <LinearGradient
          colors={PLEAT_COLORS_ESQ}
          start={{ x: 0, y: 0 }}
          end={{ x: 1, y: 0 }}
          style={StyleSheet.absoluteFill}
        />
        {/* Sombra sutil no rodapé pra dar peso ao tecido */}
        <LinearGradient
          colors={['rgba(0,0,0,0)', 'rgba(0,0,0,0.25)']}
          start={{ x: 0, y: 0.7 }}
          end={{ x: 0, y: 1 }}
          style={StyleSheet.absoluteFill}
        />
      </Animated.View>

      <Animated.View
        pointerEvents="none"
        style={[styles.cortinaDireita, cortinaDireitaStyle]}
      >
        <LinearGradient
          colors={PLEAT_COLORS_DIR}
          start={{ x: 0, y: 0 }}
          end={{ x: 1, y: 0 }}
          style={StyleSheet.absoluteFill}
        />
        <LinearGradient
          colors={['rgba(0,0,0,0)', 'rgba(0,0,0,0.25)']}
          start={{ x: 0, y: 0.7 }}
          end={{ x: 0, y: 1 }}
          style={StyleSheet.absoluteFill}
        />
      </Animated.View>

      {/* DOTS — overlay no centro, visível só enquanto as cortinas estão fechadas */}
      {dotsVisiveis && (
        <View pointerEvents="none" style={styles.dotsOverlay}>
          <View style={styles.dotsRow} accessibilityLabel="Carregando">
            <Animated.View style={[styles.dot, estiloDoDot(dot1)]} />
            <Animated.View style={[styles.dot, estiloDoDot(dot2)]} />
            <Animated.View style={[styles.dot, estiloDoDot(dot3)]} />
          </View>
        </View>
      )}
    </SafeAreaView>
  );
}

/** Pulso de cada dot: 0 → 1 → 0 em ~900 ms, com atraso inicial pra escalonar. */
function criarPulsoEmLoop(valor: Animated.Value, delay: number): Animated.CompositeAnimation {
  return Animated.loop(
    Animated.sequence([
      Animated.delay(delay),
      Animated.timing(valor, {
        toValue: 1,
        duration: 450,
        easing: Easing.inOut(Easing.quad),
        useNativeDriver: true,
      }),
      Animated.timing(valor, {
        toValue: 0,
        duration: 450,
        easing: Easing.inOut(Easing.quad),
        useNativeDriver: true,
      }),
    ]),
  );
}

/** Map do valor 0..1 para opacity e scale do dot. */
function estiloDoDot(valor: Animated.Value) {
  return {
    opacity: valor.interpolate({ inputRange: [0, 1], outputRange: [0.4, 1] }),
    transform: [
      { scale: valor.interpolate({ inputRange: [0, 1], outputRange: [0.75, 1.15] }) },
    ],
  };
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: colors.surface,
  },
  palco: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
  },
  logo: {
    width: '60%',
    height: 120,
  },
  cortinaEsquerda: {
    position: 'absolute',
    left: 0,
    top: 0,
    bottom: 0,
    width: '50%',
    overflow: 'hidden',
  },
  cortinaDireita: {
    position: 'absolute',
    right: 0,
    top: 0,
    bottom: 0,
    width: '50%',
    overflow: 'hidden',
  },
  dotsOverlay: {
    ...StyleSheet.absoluteFillObject,
    alignItems: 'center',
    justifyContent: 'center',
  },
  dotsRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.sm,
  },
  dot: {
    width: 12,
    height: 12,
    borderRadius: 6,
    backgroundColor: colors.textStrong,
  },
});
