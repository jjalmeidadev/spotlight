// Stress focado em endpoint publico (similares item-item).
// Util para validar a saude do servico de recomendacao sem custo de auth.

import http from 'k6/http';
import { check } from 'k6';
import { Trend, Rate } from 'k6/metrics';

const GATEWAY = __ENV.GATEWAY_URL || 'http://localhost:5000';

const similaresLatency = new Trend('similares_latency_ms', true);
const similaresErros   = new Rate('similares_erro');

export const options = {
  scenarios: {
    similares: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '30s', target: 50 },
        { duration: '60s', target: 50 },
        { duration: '15s', target: 0 },
      ],
    },
  },
  thresholds: {
    similares_latency_ms: ['avg<2000', 'p(95)<3500'],
    similares_erro:       ['rate<0.02'],
  },
};

export default function () {
  const espetaculoId = 1 + Math.floor(Math.random() * 100);
  const res = http.get(`${GATEWAY}/recomendacao/espetaculos/${espetaculoId}/similares?limite=10`);
  similaresLatency.add(res.timings.duration);
  similaresErros.add(res.status >= 400);
  check(res, { 'status < 500': (r) => r.status < 500 });
}
