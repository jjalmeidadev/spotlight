import { NativeModules, Platform } from 'react-native';

const GATEWAY_PORT = '5000';
const LOCAL_API_BASE_URL = `http://localhost:${GATEWAY_PORT}`;

export function getApiBaseUrl() {
  const expoDevUrl = getExpoDevelopmentApiBaseUrl();

  if (expoDevUrl) {
    return expoDevUrl;
  }

  return firstNotBlank(process.env.EXPO_PUBLIC_API_BASE_URL) ?? LOCAL_API_BASE_URL;
}

function getExpoDevelopmentApiBaseUrl() {
  if (Platform.OS === 'web') {
    return null;
  }

  const scriptUrl = NativeModules.SourceCode?.scriptURL;
  if (typeof scriptUrl !== 'string' || !scriptUrl.trim()) {
    return null;
  }

  const host = parseHost(scriptUrl);
  if (!host || isLoopbackHost(host)) {
    return null;
  }

  return `http://${host}:${GATEWAY_PORT}`;
}

function parseHost(rawUrl: string) {
  try {
    return new URL(rawUrl).hostname;
  } catch {
    const match = rawUrl.match(/^[a-z]+:\/\/([^/:]+)/i);
    return match?.[1] ?? null;
  }
}

function isLoopbackHost(host: string) {
  return host === 'localhost' || host === '127.0.0.1' || host === '::1';
}

function firstNotBlank(...values: Array<string | undefined>) {
  return values.find((value) => value && value.trim())?.trim();
}
