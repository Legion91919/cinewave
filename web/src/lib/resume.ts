/** Matches the API rules: under 30s isn't worth asking about, 95%+ counts as finished. */
export const MIN_RESUME_SECONDS = 30;
export const FINISHED_SHARE = 0.95;

export function shouldOfferResume(position: number, duration: number | null): boolean {
  if (position < MIN_RESUME_SECONDS) return false;
  if (duration && position >= duration * FINISHED_SHARE) return false;
  return true;
}

/**
 * Decides whether a progress report should be sent now: every `intervalSeconds` of playback,
 * or immediately on pause/exit (force). Keeps writes small and infrequent.
 */
export function shouldReport(lastReportedAt: number | null, position: number, force: boolean, intervalSeconds = 10): boolean {
  if (force) return true;
  if (lastReportedAt === null) return position >= 1;
  return Math.abs(position - lastReportedAt) >= intervalSeconds;
}
