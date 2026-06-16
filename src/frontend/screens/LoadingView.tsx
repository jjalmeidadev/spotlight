import React from 'react';
import { Image, StyleSheet, View } from 'react-native';
import { useLoadingViewModel } from '../viewmodels/useLoadingViewModel';

type LoadingViewProps = {
  onPronto?: () => void;
};

export default function LoadingView({ onPronto }: LoadingViewProps) {
  useLoadingViewModel(onPronto);

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
