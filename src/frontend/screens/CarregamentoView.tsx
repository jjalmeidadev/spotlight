import React from 'react';
import { Image, StyleSheet, View } from 'react-native';
import { useCarregamentoViewModel } from '../viewmodels/useCarregamentoViewModel';

type CarregamentoViewProps = {
  onPronto?: () => void;
};

export default function CarregamentoView({ onPronto }: CarregamentoViewProps) {
  useCarregamentoViewModel(onPronto);

  return (
    <View style={styles.container}>
      <Image
        source={require('../../../assets/images/logo-carregamento.png')}
        style={styles.logo}
        resizeMode="contain"
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#f2f2f2',
    alignItems: 'center',
    justifyContent: 'center',
  },
  logo: {
    width: 220,
    height: 220,
  },
});
