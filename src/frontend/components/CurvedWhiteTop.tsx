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
  void radius;

  return (
    <View style={[styles.top, style]}>
      <Svg
        height="100%"
        width="100%"
        viewBox="0 0 365 210"
        preserveAspectRatio="none"
        style={side === 'right' ? styles.mirroredSvg : undefined}
      >
        <Path d="M0 0H124C220 0 288 70 330 132H365V210H0V0Z" fill="#FFFFFF" />
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
