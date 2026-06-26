// Smoke test: 1 VU, 30s. Confirma que cada endpoint critico responde 2xx
// antes de carregar volume. Falha aqui significa setup do stack quebrado.

import http from 'k6/http';
import { check, sleep } from 'k6';
import { signJwt } from './lib/jwt.js';
import { pickUser } from './lib/users.js';

const GATEWAY = __ENV.GATEWAY_URL || 'http://localhost:5000';
const SECRET  = __ENV.JWT_SECRET;

export const options = {
  vus: 1,
  duration: '30s',
  thresholds: {
    http_req_failed:   ['rate<0.01'],
    http_req_duration: ['p(95)<2000'],
    checks:            ['rate>0.99'],
  },
};

export default function () {
  const user = pickUser(0);
  const token = signJwt(SECRET, { sub: user.sub, email: user.email });
  const auth = { headers: { Authorization: `Bearer ${token}` } };

  const r1 = http.get(`${GATEWAY}/health`);
  check(r1, { 'gateway /health 200': (r) => r.status === 200 });

  const r2 = http.get(`${GATEWAY}/catalogo/espetaculos`);
  check(r2, { 'catalogo lista 200': (r) => r.status === 200 });

  const r3 = http.get(`${GATEWAY}/catalogo/espetaculos/1`);
  check(r3, { 'catalogo detalhe 200 ou 404': (r) => r.status === 200 || r.status === 404 });

  const r4 = http.get(`${GATEWAY}/catalogo/categorias`);
  check(r4, { 'categorias 200': (r) => r.status === 200 });

  const r5 = http.get(`${GATEWAY}/recomendacao/me?limite=10`, auth);
  check(r5, { 'recomendacao/me 200': (r) => r.status === 200 });

  const r6 = http.get(`${GATEWAY}/recomendacao/espetaculos/1/similares?limite=10`);
  check(r6, { 'similares 200 ou 404': (r) => r.status === 200 || r.status === 404 });

  sleep(1);
}
