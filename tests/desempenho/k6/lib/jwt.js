// HS256 JWT signer para k6.
// Backend (AuthSetup.cs) valida JWT com SUPABASE_JWT_SECRET; aceita o segredo como
// UTF-8 OU base64. Aqui usamos UTF-8 — a chave esta em texto claro.

import crypto from 'k6/crypto';
import encoding from 'k6/encoding';

function base64url(bytes) {
  return encoding.b64encode(bytes, 'rawstd')
    .replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
}

function base64urlString(str) {
  return base64url(str);
}

export function signJwt(secret, payloadOverrides) {
  const header = base64urlString(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
  const now = Math.floor(Date.now() / 1000);
  const payload = Object.assign({
    iat: now,
    exp: now + 3600,
    aud: 'authenticated',
    role: 'authenticated',
    app_metadata: { tipoUsuario: 'cadastrado' },
  }, payloadOverrides || {});
  const body = base64urlString(JSON.stringify(payload));
  const toSign = header + '.' + body;
  // crypto.hmac retorna binario quando output != 'hex'/'base64'; usamos 'binary'.
  const sig = crypto.hmac('sha256', secret, toSign, 'binary');
  return toSign + '.' + base64url(sig);
}
