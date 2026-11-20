// Scripted walkthrough for the README demo. Run: node demo/record.mjs .
// Story: browse what is licensed here, stream from the CDN on a phone, resume on the TV, and an expired account gets a 402 renew notice.
const device = (page, name) => page.getByRole("combobox", { name: "Device" }).selectOption({ label: `On ${name}` });
const profile = (page, name) => page.getByRole("combobox", { name: "Profile" }).selectOption({ label: name });

export default async ({ page, step, shot, pause }) => {
  let manifest = "/cdn/…";
  page.on("request", r => { const u = new URL(r.url()); if (u.pathname.startsWith("/cdn/") && u.pathname.endsWith("master.m3u8")) manifest = u.pathname; });

  await step("Cinewave: Adaeze in Nigeria only sees films licensed in Nigeria today", async () => {
    await page.getByRole("heading", { name: "Continue watching" }).waitFor();
    await device(page, "phone");
  });
  await shot("browse");

  await step("Play checks the licence and the subscription, then hands back a CDN manifest URL", async () => {
    await page.getByRole("button", { name: "▶ Play" }).first().click();
    await page.locator("video").waitFor();
    await page.waitForFunction(() => { const v = document.querySelector("video"); return v && v.currentTime > 1.5 && !v.paused; }, null, { timeout: 15000 });
  });
  await step(`The phone streams HLS straight from ${manifest}; the API never touches the video`, async () => {
    await pause(1500);
  });
  await shot("playing");

  await step("Pausing at 12:30 saves progress with one small idempotent PUT", async () => {
    await page.evaluate(() => new Promise(done => {
      const v = document.querySelector("video");
      v.pause();
      const stopAt1230 = () => { if (v.currentTime >= 750.3) { v.removeEventListener("timeupdate", stopAt1230); v.pause(); done(); } };
      v.addEventListener("seeked", () => { v.addEventListener("timeupdate", stopAt1230); v.play(); }, { once: true });
      v.currentTime = 750.1;
    }));
    await page.getByRole("status").filter({ hasText: "Progress saved at 12:30" }).waitFor();
  });

  await step("Same profile, now on the TV: Continue watching says where it stopped and on which device", async () => {
    await page.getByRole("button", { name: "Back to browse" }).click();
    await device(page, "TV");
    await page.getByRole("heading", { name: "Continue watching" }).waitFor();
    await page.getByRole("heading", { name: "Continue watching" }).scrollIntoViewIfNeeded();
  });
  await shot("continue-watching");

  await step("Proof: the TV resumes at 12:30, the position the phone saved", async () => {
    await page.getByRole("link", { name: /^Resume The Salt Road at 12:30/ }).click();
    await page.getByRole("heading", { name: "Resume at 12:30?" }).waitFor();
    await pause(1800);
    await page.getByRole("button", { name: "▶ Resume at 12:30" }).click();
    await page.waitForFunction(() => { const v = document.querySelector("video"); return v && v.currentTime > 751 && !v.paused; }, null, { timeout: 15000 });
  });

  await step("Wanjiru's subscription has lapsed: Play returns 402 with a renew link, and her place is kept", async () => {
    await page.getByRole("button", { name: "Back to browse" }).click();
    await profile(page, "Wanjiru (KE)");
    await page.getByRole("button", { name: "▶ Play" }).first().click();
    await page.getByRole("alertdialog", { name: "Your subscription has ended" }).waitFor();
    await pause(1200);
  });
  await shot("renew");
};

export async function mobile({ page, shot }) {
  await page.getByRole("heading", { name: "Continue watching" }).waitFor();
  await shot("mobile");
}
