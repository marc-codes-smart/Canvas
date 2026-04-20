import { test, expect } from "@playwright/test";

test("health page loads and displays API status", async ({ page }) => {
  await page.route("**/health", (route) =>
    route.fulfill({
      status: 200,
      body: "Healthy",
      contentType: "text/plain",
    })
  );

  await page.goto("/");

  const status = page.getByTestId("health-status");
  await expect(status).toBeVisible();
  await expect(status).toHaveText("Healthy");
});
