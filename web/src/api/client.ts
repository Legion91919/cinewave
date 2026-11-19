import type { BrowsePage, ContinueItem, PlayResponse, Problem, Profile, ProgressResponse, ProgressUpdate, TitleDetail } from './types';

/** Any non-2xx answer from the API, carrying its RFC 9457 problem body. */
export class ApiError extends Error {
  constructor(public readonly status: number, public readonly problem: Problem) {
    super(problem.detail ?? problem.title ?? `Request failed (${status})`);
  }
}

/** 402 from play: the subscription has lapsed. `renewUrl` leads to the hosted payment page. */
export class PaymentRequiredError extends ApiError {
  get renewUrl(): string { return this.problem.renewUrl ?? '/account/renew'; }
}

type Fetch = typeof fetch;

export function createClient(fetchImpl: Fetch = (...a) => fetch(...a)) {
  async function call<T>(method: string, path: string, body?: unknown, signal?: AbortSignal): Promise<T> {
    const res = await fetchImpl(path, {
      method,
      signal,
      headers: body === undefined ? { accept: 'application/json' } : { accept: 'application/json', 'content-type': 'application/json' },
      body: body === undefined ? undefined : JSON.stringify(body),
    });
    const text = await res.text();
    const json = text ? JSON.parse(text) : undefined;
    if (!res.ok) {
      const problem: Problem = json ?? { status: res.status };
      throw res.status === 402 ? new PaymentRequiredError(402, problem) : new ApiError(res.status, problem);
    }
    return json as T;
  }
  const p = (id: string) => `/api/profiles/${encodeURIComponent(id)}`;
  return {
    profiles: (s?: AbortSignal) => call<Profile[]>('GET', '/api/profiles', undefined, s),
    browse: (profileId: string, s?: AbortSignal) => call<BrowsePage>('GET', `${p(profileId)}/browse`, undefined, s),
    continueWatching: (profileId: string, s?: AbortSignal) => call<ContinueItem[]>('GET', `${p(profileId)}/continue-watching`, undefined, s),
    title: (profileId: string, titleId: string, s?: AbortSignal) => call<TitleDetail>('GET', `${p(profileId)}/titles/${encodeURIComponent(titleId)}`, undefined, s),
    play: (profileId: string, titleId: string) => call<PlayResponse>('POST', `${p(profileId)}/titles/${encodeURIComponent(titleId)}/play`),
    saveProgress: (profileId: string, titleId: string, update: ProgressUpdate) =>
      call<ProgressResponse>('PUT', `${p(profileId)}/progress/${encodeURIComponent(titleId)}`, update),
  };
}

export type ApiClient = ReturnType<typeof createClient>;
export const api = createClient();
