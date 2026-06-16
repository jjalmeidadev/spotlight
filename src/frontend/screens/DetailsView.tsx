import React from 'react';
import {
  ActivityIndicator,
  Alert,
  Image,
  ImageBackground,
  ListRenderItemInfo,
  Pressable,
  SafeAreaView,
  ScrollView,
  StatusBar,
  StyleSheet,
  Text,
  View,
} from 'react-native';


import { Carousel, CarouselItem } from '@/frontend/components/Carousel';
import { ShowcaseCard } from '@/frontend/components/ShowcaseCard';
import { colors, spacing, typography } from '@/frontend/theme';
import type { EspetaculoSemelhante } from '../models/DetailsModels';
import { emitTelemetryEvent } from '../repositories/TelemetryRepository';
import { useDetailsViewModel } from '../viewmodels/DetailsViewModels';


type DetailsViewProps = {
  accessToken?: string | null;
  espetaculoId: string;
  onClose?: () => void;
  onEspetaculoSemelhantePress?: (espetaculoId: string) => void;
  onComprarIngresso?: (linkCompra: string) => void;
};


const STAR_COUNT = 5;
const HERO_HEIGHT = 280;
const NOVICA_REBELDE_CAPA = require('../../../assets/images/novicarebelde.png');
const novicaRebeldeCapaSource = Image.resolveAssetSource(NOVICA_REBELDE_CAPA);
const NOVICA_REBELDE_CAPA_ASPECT_RATIO =
  novicaRebeldeCapaSource.width / novicaRebeldeCapaSource.height;


export default function DetailsView({
  accessToken,
  espetaculoId,
  onClose,
  onEspetaculoSemelhantePress,
  onComprarIngresso,
}: DetailsViewProps) {
  const viewModel = useDetailsViewModel(espetaculoId, { accessToken, onComprarIngresso });
  const lastViewedIdRef = React.useRef<string | null>(null);


  React.useEffect(() => {
    if (!viewModel.detalhe || lastViewedIdRef.current === viewModel.detalhe.id) {
      return;
    }


    lastViewedIdRef.current = viewModel.detalhe.id;
    emitTelemetryEvent(
      {
        eventType: 'show_viewed',
        screen: 'DetalhesView',
        entity: { type: 'espetaculo', id: viewModel.detalhe.id },
        properties: { origin: 'catalog' },
      },
      accessToken,
    );
  }, [accessToken, viewModel.detalhe]);


  if (viewModel.loading) {
    return (
      <SafeAreaView style={styles.safeArea}>
        <StatusBar barStyle="light-content" backgroundColor={colors.surface} />
        <View style={styles.centeredState}>
          <ActivityIndicator color={colors.textStrong} size="large" />
        </View>
      </SafeAreaView>
    );
  }


  if (viewModel.error) {
    return (
      <SafeAreaView style={styles.safeArea}>
        <StatusBar barStyle="light-content" backgroundColor={colors.surface} />
        <View style={styles.centeredState}>
          <Text style={styles.errorMessage}>{viewModel.error}</Text>
          <Pressable
            accessibilityRole="button"
            onPress={viewModel.recarregar}
            style={({ pressed }: { pressed: boolean }) => [
              styles.retryButton,
              pressed && styles.pressed,
            ]}
          >
            <Text style={styles.retryLabel}>Tentar novamente</Text>
          </Pressable>
          {onClose ? (
            <Pressable
              accessibilityLabel="Fechar detalhes do espetáculo"
              accessibilityRole="button"
              onPress={onClose}
              style={({ pressed }: { pressed: boolean }) => [
                styles.closeLink,
                pressed && styles.pressed,
              ]}
            >
              <Text style={styles.closeLinkLabel}>Fechar</Text>
            </Pressable>
          ) : null}
        </View>
      </SafeAreaView>
    );
  }


  const espetaculo = viewModel.detalhe;


  if (!espetaculo) {
    return null;
  }

  const detalheAtual = espetaculo;

  const similarCarouselData: Array<CarouselItem<EspetaculoSemelhante>> =
    viewModel.espetaculosSemelhantes.map((item: EspetaculoSemelhante) => ({
      ...item,
      id: item.id,
    }));


  const renderSimilarItem = ({
    item,
    index,
  }: ListRenderItemInfo<CarouselItem<EspetaculoSemelhante>>) => (
    <ShowcaseCard
      title={item.titulo}
      imageUri={item.imagemUrl}
      onPress={() => {
        emitTelemetryEvent(
          {
            eventType: 'show_card_clicked',
            screen: 'DetalhesView',
            entity: { type: 'espetaculo', id: item.id },
            properties: {
              origin: 'similar',
              position: index,
              recommendationAlgorithm: item.algoritmo,
              recommendationScore: item.score,
            },
          },
          accessToken,
        );


        onEspetaculoSemelhantePress?.(item.id);
      }}
    />
  );


  const heroImageSource =
    espetaculo.titulo === 'A Noviça Rebelde'
      ? NOVICA_REBELDE_CAPA
      : espetaculo.imagemUrl
        ? { uri: espetaculo.imagemUrl }
        : undefined;
  const usaCapaLocal = espetaculo.titulo === 'A Noviça Rebelde';


  function handleBuyPress() {
    if (!detalheAtual.linkCompra) {
      return;
    }


    emitTelemetryEvent(
      {
        eventType: 'purchase_clicked',
        screen: 'DetalhesView',
        entity: { type: 'espetaculo', id: detalheAtual.id },
        properties: {
          provider: getPurchaseProvider(detalheAtual.linkCompra),
        },
      },
      accessToken,
    );


    viewModel.comprarIngresso();
  }

  async function salvarAvaliacao(nota: number) {
    try {
      await viewModel.avaliar(nota);
      emitTelemetryEvent(
        {
          eventType: 'show_card_clicked',
          screen: 'DetalhesView',
          entity: { type: 'espetaculo', id: detalheAtual.id },
          properties: {
            origin: 'rating',
            rating: nota,
          },
        },
        accessToken,
      );
    } catch (error) {
      Alert.alert(
        'Avaliacao nao salva',
        error instanceof Error ? error.message : 'Nao foi possivel salvar sua avaliacao.',
      );
    }
  }


  return (
    <SafeAreaView style={styles.safeArea}>
      <StatusBar barStyle="light-content" backgroundColor={colors.surface} />


      <ScrollView
        bounces={false}
        showsVerticalScrollIndicator={false}
        contentContainerStyle={styles.scrollContent}
      >
        <View
          style={[
            styles.heroWrapper,
            usaCapaLocal && {
              height: undefined,
              aspectRatio: NOVICA_REBELDE_CAPA_ASPECT_RATIO,
            },
          ]}
        >
          {heroImageSource ? (
            <ImageBackground
              source={heroImageSource}
              resizeMode={usaCapaLocal ? 'contain' : 'cover'}
              style={[styles.heroImage, usaCapaLocal && styles.heroImageLocalCapa]}
            >
              <View style={styles.heroOverlay} />
            </ImageBackground>
          ) : (
            <View style={[styles.heroImage, styles.heroPlaceholder]}>
              <View style={styles.heroOverlay} />
            </View>
          )}


          {onClose ? (
            <Pressable
              accessibilityLabel="Fechar detalhes do espetáculo"
              accessibilityRole="button"
              hitSlop={8}
              onPress={onClose}
              style={({ pressed }: { pressed: boolean }) => [
                styles.closeButton,
                pressed && styles.pressed,
              ]}
            >
              <Text style={styles.closeIcon}>×</Text>
            </Pressable>
          ) : null}
        </View>


        <View style={styles.content}>
          <Text style={styles.title}>{espetaculo.titulo}</Text>


          <View style={styles.metaRow}>
            <Text style={styles.approvalText}>
              Aclamado por {espetaculo.percentualAprovacao}% do público
            </Text>


            <View style={styles.tagsRow}>
              {espetaculo.tags.map((tag: string) => (
                <View key={tag} style={styles.tag}>
                  <Text style={styles.tagLabel}>{tag}</Text>
                </View>
              ))}
            </View>
          </View>


          <View style={styles.actionsRow}>
            <StarRating
              disabled={viewModel.avaliando}
              onChange={async (nota) => {
                if (viewModel.avaliacaoUsuario !== null) {
                  Alert.alert(
                    'Alterar avaliacao?',
                    `Voce ja avaliou este espetaculo. Deseja alterar sua nota para ${nota}?`,
                    [
                      { text: 'Cancelar', style: 'cancel' },
                      { text: 'Alterar', onPress: () => void salvarAvaliacao(nota) },
                    ],
                  );
                  return;
                }

                await salvarAvaliacao(nota);
              }}
              value={viewModel.avaliacaoUsuario ?? espetaculo.notaMedia}
            />


            <View style={styles.actionsRight}>
              <Pressable
                accessibilityRole="button"
                onPress={handleBuyPress}
                style={({ pressed }: { pressed: boolean }) => [
                  styles.buyButton,
                  pressed && styles.pressed,
                ]}
              >
                <Text style={styles.buyLabel}>Comprar Ingresso</Text>
              </Pressable>
            </View>
          </View>


          <View style={styles.section}>
            <Text style={styles.sectionTitle}>Descrição</Text>
            <Text style={styles.description}>{espetaculo.descricao}</Text>
            <View style={styles.infoGrid}>
              <InfoPill label="Classificação" value={espetaculo.classificacaoIndicativa ?? espetaculo.faixaEtaria} />
              <InfoPill label="Status" value={formatStatus(espetaculo.status)} />
              <InfoPill label="Próxima sessão" value={espetaculo.proximaSessao} />
              <InfoPill label="Teatro" value={espetaculo.teatro} />
              <InfoPill label="Preço" value={espetaculo.preco} />
              <InfoPill label="Produtora" value={espetaculo.produtora} />
            </View>
          </View>


          {similarCarouselData.length > 0 ? (
            <View style={styles.similarSection}>
              <Carousel
                title="Eventos semelhantes"
                data={similarCarouselData}
                itemWidth={160}
                itemGap={spacing.sm}
                contentContainerStyle={styles.similarListContent}
                renderItem={renderSimilarItem}
              />
            </View>
          ) : null}
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}


function getPurchaseProvider(linkCompra: string) {
  try {
    return new URL(linkCompra).hostname.replace(/^www\./, '');
  } catch {
    return 'unknown';
  }
}

function formatStatus(status?: string) {
  if (!status) return null;

  return status
    .replace(/_/g, ' ')
    .replace(/\b\w/g, (letter) => letter.toUpperCase());
}

function InfoPill({ label, value }: { label: string; value?: string | null }) {
  if (!value) {
    return null;
  }

  return (
    <View style={styles.infoPill}>
      <Text style={styles.infoLabel}>{label}</Text>
      <Text style={styles.infoValue}>{value}</Text>
    </View>
  );
}


type StarRatingProps = {
  disabled?: boolean;
  onChange?: (value: number) => void;
  value: number;
};


function StarRating({ disabled, onChange, value }: StarRatingProps) {
  const filledStars = Math.max(0, Math.min(STAR_COUNT, Math.round(value)));


  return (
    <View
      style={styles.starsRow}
      accessibilityRole="image"
      accessibilityLabel={`${value} de 5 estrelas`}
    >
      {Array.from({ length: STAR_COUNT }, (_, index) => {
        const filled = index < filledStars;


        return (
          <Pressable
            accessibilityLabel={`Avaliar com ${index + 1} de 5 estrelas`}
            accessibilityRole="button"
            disabled={disabled || !onChange}
            hitSlop={6}
            key={index}
            onPress={() => onChange?.(index + 1)}
            style={({ pressed }: { pressed: boolean }) => pressed && styles.pressed}
          >
            <Text style={[styles.star, filled ? styles.starFilled : styles.starEmpty]}>
              {filled ? '★' : '☆'}
            </Text>
          </Pressable>
        );
      })}
    </View>
  );
}


const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: colors.surface,
  },
  scrollContent: {
    flexGrow: 1,
    paddingBottom: spacing.xxxl,
  },
  centeredState: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: spacing.xl,
    gap: spacing.md,
  },
  errorMessage: {
    color: colors.textStrong,
    ...typography.body,
    textAlign: 'center',
  },
  retryButton: {
    minHeight: spacing.touchMin,
    minWidth: 168,
    borderRadius: 999,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: colors.background,
    paddingHorizontal: spacing.lg,
  },
  retryLabel: {
    color: colors.text,
    ...typography.button,
    fontSize: 13,
  },
  closeLink: {
    minHeight: spacing.touchMin,
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: spacing.md,
  },
  closeLinkLabel: {
    color: colors.textSubtle,
    ...typography.body,
    textDecorationLine: 'underline',
  },
  heroWrapper: {
    position: 'relative',
    height: HERO_HEIGHT,
    backgroundColor: colors.borderStrong,
  },
  heroImage: {
    width: '100%',
    height: HERO_HEIGHT,
  },
  heroImageLocalCapa: {
    height: '100%',
  },
  heroPlaceholder: {
    backgroundColor: colors.borderStrong,
  },
  heroOverlay: {
    ...StyleSheet.absoluteFillObject,
    backgroundColor: 'rgba(17, 20, 18, 0.42)',
  },
  closeButton: {
    position: 'absolute',
    top: spacing.md,
    right: spacing.md,
    minWidth: spacing.touchMin,
    minHeight: spacing.touchMin,
    alignItems: 'center',
    justifyContent: 'center',
  },
  closeIcon: {
    color: colors.textStrong,
    fontSize: 32,
    lineHeight: 34,
    fontWeight: '300',
  },
  content: {
    paddingHorizontal: spacing.md,
    paddingTop: spacing.md,
    gap: spacing.md,
  },
  title: {
    color: colors.textStrong,
    ...typography.title,
    fontSize: 24,
    lineHeight: 30,
    textTransform: 'none',
  },
  metaRow: {
    flexDirection: 'row',
    alignItems: 'flex-start',
    justifyContent: 'space-between',
    gap: spacing.sm,
  },
  approvalText: {
    flex: 1,
    color: colors.textStrong,
    ...typography.body,
    fontSize: 13,
    lineHeight: 18,
  },
  tagsRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    justifyContent: 'flex-end',
    gap: spacing.xs,
    maxWidth: '52%',
  },
  tag: {
    borderRadius: 999,
    borderWidth: 1,
    borderColor: colors.textStrong,
    paddingHorizontal: spacing.sm,
    paddingVertical: 4,
  },
  tagLabel: {
    color: colors.textStrong,
    ...typography.caption,
    fontSize: 11,
    lineHeight: 14,
  },
  actionsRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    gap: spacing.sm,
  },
  starsRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 2,
    minHeight: spacing.touchMin,
  },
  star: {
    fontSize: 18,
    lineHeight: 20,
  },
  starFilled: {
    color: colors.textStrong,
  },
  starEmpty: {
    color: colors.textSubtle,
  },
  actionsRight: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.sm,
    flexShrink: 1,
  },
  buyButton: {
    minHeight: 36,
    borderRadius: 999,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: colors.background,
    paddingHorizontal: spacing.md,
  },
  buyLabel: {
    color: colors.text,
    ...typography.button,
    fontSize: 12,
    lineHeight: 16,
  },
  section: {
    gap: spacing.xs,
  },
  sectionTitle: {
    color: colors.textStrong,
    ...typography.label,
    fontSize: 16,
    lineHeight: 20,
  },
  description: {
    color: colors.textStrong,
    ...typography.body,
    fontSize: 14,
    lineHeight: 21,
  },
  infoGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: spacing.xs,
    marginTop: spacing.xs,
  },
  infoPill: {
    borderColor: 'rgba(255, 255, 255, 0.42)',
    borderRadius: 12,
    borderWidth: 1,
    maxWidth: '100%',
    paddingHorizontal: spacing.sm,
    paddingVertical: 6,
  },
  infoLabel: {
    color: colors.textSubtle,
    ...typography.caption,
    fontSize: 9,
    lineHeight: 11,
    textTransform: 'uppercase',
  },
  infoValue: {
    color: colors.textStrong,
    ...typography.caption,
    fontSize: 11,
    lineHeight: 14,
    marginTop: 2,
  },
  similarSection: {
    marginTop: spacing.xs,
  },
  similarListContent: {
    paddingLeft: 0,
  },
  pressed: {
    opacity: 0.82,
  },
});
