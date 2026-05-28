import React from 'react';
import { StyleSheet, View, ViewStyle } from 'react-native';
import Svg, { Path } from 'react-native-svg';

type CurvedWhiteTopProps = {
  style?: ViewStyle;
  /** side controls which side the curve is on: 'left' | 'right' */
  side?: 'left' | 'right';
  radius?: number;
};

export function CurvedWhiteTop({ style, side = 'left', radius = 160 }: CurvedWhiteTopProps) {
  return (
    <View style={[styles.top, style]}>
      <Svg
        height="100%"
        width="90%"
        viewBox="0 0 365 611"
        preserveAspectRatio="xMidYMin slice"
        style={side === 'right' ? styles.mirroredSvg : undefined}
      >
        <Path d="M259 178.896C259 128.911 239.53 1.40311 122.228 0H0V178.896H259Z" fill="#FFFFFF" />
      </Svg>
    </View>
  );
}

const styles = StyleSheet.create({
  top: {
    position: 'absolute',
    left: 0,
    right: 0,
    height: 320,
    backgroundColor: 'transparent',
    overflow: 'hidden',
    transform: [{ translateY: -20 }],
  },
  mirroredSvg: {
    transform: [{ scaleX: -1 }],
  },
});
