import http from "k6/http";
import { check } from "k6";
import { Trend, Rate } from "k6/metrics";

const maxDuration = Number(__ENV.RNF02_MAX_DURATION_MS || 2000);
const baseUrl = (__ENV.RECOMMENDATION_BASE_URL || "http://localhost:5003").replace(/\/$/, "");
const path = __ENV.RECOMMENDATION_PATH || "/me?limite=10";
const tokens = [__ENV.PROFILE_1_TOKEN, __ENV.PROFILE_2_TOKEN, __ENV.PROFILE_3_TOKEN];

const perceivedHttpDuration = new Trend("recommendation_http_duration", true);
const applicationErrors = new Rate("recommendation_application_errors");

export const options = {
  scenarios: {
    three_profiles: {
      executor: "per-vu-iterations",
      vus: 3,
      iterations: 10,
      maxDuration: "5m",
    },
  },
  thresholds: {
    recommendation_http_duration: [`max<=${maxDuration}`],
    recommendation_application_errors: ["rate==0"],
    http_req_failed: ["rate==0"],
  },
  summaryTrendStats: ["count", "avg", "min", "med", "max", "p(90)", "p(95)"],
};

export function setup() {
  const missing = tokens
    .map((token, index) => ({ token, profile: index + 1 }))
    .filter((item) => !item.token)
    .map((item) => item.profile);
  if (missing.length) {
    throw new Error(`Tokens ausentes para os perfis: ${missing.join(", ")}`);
  }
}

export default function () {
  const profileIndex = (__VU - 1) % 3;
  const profileName = `perfil-${profileIndex + 1}`;
  const response = http.get(`${baseUrl}${path}`, {
    headers: {
      Authorization: `Bearer ${tokens[profileIndex]}`,
      Accept: "application/json",
    },
    tags: {
      name: "recomendacao-me",
      profile: profileName,
      rnf: "RNF02",
    },
    timeout: "10s",
  });

  perceivedHttpDuration.add(response.timings.duration, { profile: profileName });
  const valid = check(response, {
    "status HTTP 2xx": (res) => res.status >= 200 && res.status < 300,
    "resposta em até 2 segundos": (res) => res.timings.duration <= maxDuration,
    "corpo não vazio": (res) => Boolean(res.body && res.body.length > 0),
  }, { profile: profileName });
  applicationErrors.add(!valid, { profile: profileName });
}

export function handleSummary(data) {
  return {
    stdout: JSON.stringify({
      rnf: "RNF02",
      criterio_ms: maxDuration,
      metricas: data.metrics,
    }, null, 2) + "\n",
    "/results/rnf02-summary.json": JSON.stringify(data, null, 2),
  };
}
