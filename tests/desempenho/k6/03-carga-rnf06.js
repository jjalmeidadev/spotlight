// RNF06 — 200 usuarios simultaneos por 10 minutos contínuos.
// Mix realista: 60% navegacao publica (catalogo/categorias/teatros/detalhe),
// 30% recomendacao personalizada (auth), 10% similares (publico).
// Aceitacao: tempo medio <= 3s, taxa de erro <= 1%, sem falhas criticas.

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';
import { signJwt } from './lib/jwt.js';
import { pickUser } from './lib/users.js';

const GATEWAY = __ENV.GATEWAY_URL || 'http://localhost:5000';
const SECRET  = __ENV.JWT_SECRET;
const VUS     = parseInt(__ENV.LOAD_VUS      || '200', 10);
const DURATION = __ENV.LOAD_DURATION || '10m';
const RAMP_UP  = __ENV.LOAD_RAMP_UP  || '1m';
const RAMP_DOWN = __ENV.LOAD_RAMP_DOWN || '30s';

const rnf06_erros          = new Rate('rnf06_erro');
const rnf06_latency        = new Trend('rnf06_latency_ms', true);
const recomendacaoLatency  = new Trend('recomendacao_latency_ms', true);
const catalogoLatency      = new Trend('catalogo_latency_ms', true);

export const options = {
  scenarios: {
    carga: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: RAMP_UP,   target: VUS },
        { duration: DURATION,  target: VUS },
        { duration: RAMP_DOWN, target: 0 },
      ],
      gracefulRampDown: '30s',
    },
  },
  thresholds: {
    rnf06_erro:           ['rate<0.01'],
    rnf06_latency_ms:     ['avg<3000', 'p(95)<5000'],
    recomendacao_latency_ms: ['avg<3000'],
    http_req_failed:      ['rate<0.01'],
  },
};

const tokensPorVU = {};
function tokenParaVU(vu) {
  if (!tokensPorVU[vu]) {
    const user = pickUser(vu);
    tokensPorVU[vu] = signJwt(SECRET, { sub: user.sub, email: user.email });
  }
  return tokensPorVU[vu];
}

function navegacaoPublica() {
  const espetaculoId = 1 + Math.floor(Math.random() * 50);
  const res = http.batch([
    ['GET', `${GATEWAY}/catalogo/espetaculos`,                  null, { tags: { name: 'catalogo_lista' } }],
    ['GET', `${GATEWAY}/catalogo/categorias`,                   null, { tags: { name: 'categorias' } }],
    ['GET', `${GATEWAY}/catalogo/teatros`,                      null, { tags: { name: 'teatros' } }],
    ['GET', `${GATEWAY}/catalogo/espetaculos/${espetaculoId}`,  null, { tags: { name: 'catalogo_detalhe' } }],
  ]);
  res.forEach((r) => {
    rnf06_erros.add(r.status >= 400);
    rnf06_latency.add(r.timings.duration);
    catalogoLatency.add(r.timings.duration);
  });
  check(res[0], { 'lista 2xx': (r) => r.status >= 200 && r.status < 300 });
}

function recomendacaoMe(token) {
  const res = http.get(`${GATEWAY}/recomendacao/me?limite=10`, {
    headers: { Authorization: `Bearer ${token}` },
    tags:    { name: 'recomendacao_me' },
  });
  rnf06_erros.add(res.status >= 400);
  rnf06_latency.add(res.timings.duration);
  recomendacaoLatency.add(res.timings.duration);
  check(res, { 'recomendacao 200': (r) => r.status === 200 });
}

function similaresPublico() {
  const espetaculoId = 1 + Math.floor(Math.random() * 50);
  const res = http.get(`${GATEWAY}/recomendacao/espetaculos/${espetaculoId}/similares?limite=10`, {
    tags: { name: 'similares' },
  });
  rnf06_erros.add(res.status >= 400);
  rnf06_latency.add(res.timings.duration);
  check(res, { 'similares 2xx ou 404': (r) => r.status < 500 });
}

export default function () {
  const r = Math.random();
  const token = tokenParaVU(__VU);
  if (r < 0.60)      navegacaoPublica();
  else if (r < 0.90) recomendacaoMe(token);
  else               similaresPublico();
  sleep(0.5 + Math.random() * 0.5);
}
