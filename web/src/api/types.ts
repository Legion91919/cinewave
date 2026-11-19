export interface TitleArt { palette: string[]; pattern: string }
export interface TitleSummary {
  id: string; name: string; year: number; country: string; runtimeMinutes: number;
  genres: string[]; synopsis: string; director: string; art: TitleArt;
}
export interface CatalogueRow { id: string; title: string; items: TitleSummary[] }
export interface BrowsePage { territory: string; hero: TitleSummary | null; rows: CatalogueRow[] }
export interface Profile { id: string; accountId: string; displayName: string; territory: string; avatarColour: string }
export interface ProgressView {
  profileId: string; titleId: string; positionSeconds: number; durationSeconds: number;
  deviceId: string; deviceName: string; clientTimestamp: string; receivedAt: string;
}
export interface ContinueItem {
  titleId: string; title: TitleSummary; positionSeconds: number; durationSeconds: number;
  remainingSeconds: number; deviceId: string; deviceName: string; updatedAt: string;
}
export interface TitleDetail { title: TitleSummary; progress: ProgressView | null }
export interface PlayResponse {
  titleId: string; titleName: string; manifestUrl: string; resumeAtSeconds: number;
  resumeFromDevice: string | null; stoppedAt: string | null;
}
export interface ProgressUpdate {
  positionSeconds: number; durationSeconds: number; deviceId: string; deviceName: string; clientTimestamp: string;
}
export interface ProgressResponse {
  applied: boolean; positionSeconds: number; durationSeconds: number; deviceId: string; deviceName: string; clientTimestamp: string;
}
export interface Problem { title?: string; status?: number; detail?: string; renewUrl?: string; errors?: Record<string, string[]> }
