import { test, expect } from "@playwright/test";

const API_URL = process.env.API_URL ?? "http://localhost:5050";

test("web: health status renders from live API", async ({ page }) => {
  await page.goto("/");
  await expect(page.getByTestId("health-status")).toHaveText("Healthy", {
    timeout: 15_000,
  });
});

test("api: stub ping confirms DB round-trip", async ({ request }) => {
  const response = await request.get(`${API_URL}/stub/ping`);
  expect(response.ok()).toBeTruthy();
  const body = await response.json();
  expect(body.pong).toBe(true);
  expect(body.messageId).toBeTruthy();
});
