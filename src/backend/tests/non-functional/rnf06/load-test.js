import http from "k6/http";
import { check, group, sleep } from "k6";
import { Counter, Rate, Trend } from "k6/metrics";

const testScenario = (__ENV.RNF06_SCENARIO || "positive").toLowerCase();

const catalogBaseUrl = (
  __ENV.CATALOG_BASE_URL || "http://localhost:5001"
).replace(/\/$/, "");

const recommendationBaseUrl = (
  __ENV.RECOMMENDATION_BASE_URL || "http://localhost:5003"
).replace(/\/$/, "");

const catalogPath =
  __ENV.CATALOG_PATH || "/espetaculos?page=1&pageSize=20";

const searchPath =
  __ENV.SEARCH_PATH ||
  "/espetaculos?q=Hamlet&page=1&pageSize=20";

const recommendationPath =
  __ENV.RECOMMENDATION_PATH || "/me?limite=10";

const catalogHealthPath =
  __ENV.CATALOG_HEALTH_PATH || "/health/ready";

const recommendationHealthPath =
  __ENV.RECOMMENDATION_HEALTH_PATH || "/health/ready";

const accessToken = __ENV.ACCESS_TOKEN || "";

const requireRecommendation =
  (__ENV.REQUIRE_RECOMMENDATION || "false").toLowerCase() === "true";

const allowUnhealthyStart =
  (__ENV.ALLOW_UNHEALTHY_START || "false").toLowerCase() === "true";

const maxAverageDurationMs = Number(
  __ENV.K6_MAX_AVG_DURATION_MS || 3000
);

const maxErrorRate = Number(
  __ENV.K6_MAX_ERROR_RATE || 0.01
);

const thinkTimeSeconds = Number(
  __ENV.K6_THINK_TIME_SECONDS || 1
);

const applicationErrors = new Rate("application_errors");

const catalogDuration = new Trend(
  "catalog_duration",
  true
);

const searchDuration = new Trend(
  "search_duration",
  true
);

const recommendationDuration = new Trend(
  "recommendation_duration",
  true
);

const catalogRequests = new Counter(
  "catalog_requests"
);

const searchRequests = new Counter(
  "search_requests"
);

const recommendationRequests = new Counter(
  "recommendation_requests"
);

const positiveScenario = {
  executor: "constant-vus",
  vus: Number(__ENV.K6_VUS || 200),
  duration: __ENV.K6_DURATION || "10m",
  gracefulStop: "30s",
};

const negativeScenario = {
  executor: "ramping-vus",
  startVUs: 0,
  stages: [
    {
      duration: __ENV.K6_NEGATIVE_RAMP_UP || "1m",
      target: Number(__ENV.K6_VUS || 200),
    },
    {
      duration: __ENV.K6_NEGATIVE_BASELINE || "2m",
      target: Number(__ENV.K6_VUS || 200),
    },
    {
      duration:
        __ENV.K6_NEGATIVE_OVERLOAD_RAMP || "1m",
      target: Number(__ENV.K6_NEGATIVE_VUS || 300),
    },
    {
      duration:
        __ENV.K6_NEGATIVE_OVERLOAD_DURATION || "5m",
      target: Number(__ENV.K6_NEGATIVE_VUS || 300),
    },
    {
      duration:
        __ENV.K6_NEGATIVE_RECOVERY_RAMP || "1m",
      target: Number(__ENV.K6_RECOVERY_VUS || 50),
    },
    {
      duration:
        __ENV.K6_NEGATIVE_RECOVERY_DURATION || "2m",
      target: Number(__ENV.K6_RECOVERY_VUS || 50),
    },
    {
      duration: "30s",
      target: 0,
    },
  ],
  gracefulRampDown: "30s",
  gracefulStop: "30s",
};

const positiveThresholds = {
  "http_req_duration{rnf:RNF06}": [
    `avg<${maxAverageDurationMs}`,
  ],

  "http_req_failed{rnf:RNF06}": [
    `rate<${maxErrorRate}`,
  ],

  application_errors: [
    `rate<${maxErrorRate}`,
  ],

  "http_req_duration{name:catalogo}": [
    `avg<${maxAverageDurationMs}`,
  ],

  "http_req_duration{name:busca}": [
    `avg<${maxAverageDurationMs}`,
  ],

  "http_req_duration{name:recomendacao}": [
    `avg<${maxAverageDurationMs}`,
  ],
};

export const options = {
  discardResponseBodies: false,

  summaryTrendStats: [
    "count",
    "avg",
    "min",
    "med",
    "max",
    "p(90)",
    "p(95)",
  ],

  scenarios: {
    rnf06:
      testScenario === "negative"
        ? negativeScenario
        : positiveScenario,
  },

  thresholds:
    testScenario === "positive"
      ? positiveThresholds
      : {},
};

function validateResponse(
  response,
  operation,
  durationMetric
) {
  durationMetric.add(response.timings.duration);

  const valid = check(response, {
    [`${operation}: status HTTP 2xx`]: (res) =>
      res.status >= 200 && res.status < 300,

    [`${operation}: resposta possui conteúdo`]: (res) =>
      Boolean(res.body && res.body.length > 0),
  });

  applicationErrors.add(!valid);
}

export function setup() {
  if (requireRecommendation && !accessToken) {
    throw new Error(
      "ACCESS_TOKEN é obrigatório quando REQUIRE_RECOMMENDATION=true."
    );
  }

  const healthResponses = http.batch([
    [
      "GET",
      `${catalogBaseUrl}${catalogHealthPath}`,
      null,
      {
        tags: {
          phase: "setup",
          service: "catalogo",
        },
        timeout: "10s",
      },
    ],
    [
      "GET",
      `${recommendationBaseUrl}${recommendationHealthPath}`,
      null,
      {
        tags: {
          phase: "setup",
          service: "recomendacao",
        },
        timeout: "10s",
      },
    ],
  ]);

  const catalogHealthStatus =
    healthResponses[0].status;

  const recommendationHealthStatus =
    healthResponses[1].status;

  const servicesAreHealthy =
    catalogHealthStatus >= 200 &&
    catalogHealthStatus < 300 &&
    recommendationHealthStatus >= 200 &&
    recommendationHealthStatus < 300;

  if (!servicesAreHealthy && !allowUnhealthyStart) {
    throw new Error(
      `Health check inicial falhou: catálogo=${catalogHealthStatus}, recomendação=${recommendationHealthStatus}.`
    );
  }

  return {
    recommendationHeaders: accessToken
      ? {
          Authorization: `Bearer ${accessToken}`,
          Accept: "application/json",
        }
      : {
          Accept: "application/json",
        },
  };
}

export default function (data) {
  group("RNF06 - catálogo", () => {
    const response = http.get(
      `${catalogBaseUrl}${catalogPath}`,
      {
        tags: {
          name: "catalogo",
          rnf: "RNF06",
          scenario: testScenario,
        },
        timeout: "10s",
      }
    );

    catalogRequests.add(1);

    validateResponse(
      response,
      "Catálogo",
      catalogDuration
    );
  });

  group("RNF06 - busca", () => {
    const response = http.get(
      `${catalogBaseUrl}${searchPath}`,
      {
        tags: {
          name: "busca",
          rnf: "RNF06",
          scenario: testScenario,
        },
        timeout: "10s",
      }
    );

    searchRequests.add(1);

    validateResponse(
      response,
      "Busca",
      searchDuration
    );
  });

  if (accessToken) {
    group("RNF06 - recomendação", () => {
      const response = http.get(
        `${recommendationBaseUrl}${recommendationPath}`,
        {
          headers:
            data.recommendationHeaders,

          tags: {
            name: "recomendacao",
            rnf: "RNF06",
            scenario: testScenario,
          },

          timeout: "10s",
        }
      );

      recommendationRequests.add(1);

      validateResponse(
        response,
        "Recomendação",
        recommendationDuration
      );
    });
  }

  sleep(thinkTimeSeconds);
}