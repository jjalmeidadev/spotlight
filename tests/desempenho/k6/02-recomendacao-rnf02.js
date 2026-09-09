// RNF02 — recomendacao personalizada em ate 2s apos interacao.
// Mantemos carga moderada (20 VUs / 2min) para que a latencia medida reflita
// o caminho critico /recomendacao/me e nao a contencao da maquina de testes.

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend, Rate } from 'k6/metrics';
import { signJwt } from './lib/jwt.js';
import { SEED_USERS, pickUser } from './lib/users.js';

const GATEWAY = __ENV.GATEWAY_URL || 'http://localhost:5000';
const SECRET  = __ENV.JWT_SECRET;

const recomendacaoLatency = new Trend('recomendacao_latency_ms', true);
const rnf02_dentro_de_2s  = new Rate('rnf02_dentro_de_2s');

export const options = {
  scenarios: {
    rnf02: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '30s', target: 10 },
        { duration: '90s', target: 20 },
        { duration: '30s', target: 0 },
      ],
      gracefulRampDown: '15s',
    },
  },
  thresholds: {
    recomendacao_latency_ms: ['p(95)<2000', 'avg<1000'],
    rnf02_dentro_de_2s:      ['rate>0.99'],
    http_req_failed:         ['rate<0.01'],
  },
};

// Cache de tokens por VU evita reassinar JWT a cada iteracao.
const tokensPorVU = {};

function tokenParaVU(vu) {
  if (!tokensPorVU[vu]) {
    const user = pickUser(vu);
    tokensPorVU[vu] = signJwt(SECRET, { sub: user.sub, email: user.email });
  }
  return tokensPorVU[vu];
}

export default function () {
  const token = tokenParaVU(__VU);
  const params = {
    headers: { Authorization: `Bearer ${token}` },
    tags:    { endpoint: 'recomendacao_me' },
  };
  const res = http.get(`${GATEWAY}/recomendacao/me?limite=10`, params);
  recomendacaoLatency.add(res.timings.duration);
  rnf02_dentro_de_2s.add(res.timings.duration <= 2000);
  check(res, {
    'status 200': (r) => r.status === 200,
    'tem campo data': (r) => {
      try { return Array.isArray(r.json('data')); } catch (e) { return false; }
    },
  });
  sleep(1);
}
