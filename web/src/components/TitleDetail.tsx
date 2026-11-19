import { api, ApiError } from '../api/client';
import { navigate } from '../lib/router';
import { formatTimecode, runtimeLabel } from '../lib/time';
import { shouldOfferResume } from '../lib/resume';
import { useAsync } from '../lib/useAsync';
import { posterStyle } from './art';
import { Notice } from './Notice';

export function TitleDetail({ profileId, titleId }: { profileId: string; titleId: string }) {
  const state = useAsync((s) => api.title(profileId, titleId, s), [profileId, titleId]);
  if (state.status === 'loading') {
    return <main className="detail" aria-busy="true"><div className="art skel" /><div><div className="skel" style={{ height: 48, width: '60%' }} /></div></main>;
  }
  if (state.status === 'error') {
    const notLicensed = state.error instanceof ApiError && state.error.status === 451;
    return <main><Notice kind="error" title={notLicensed ? 'Not available in your country' : "We couldn't load this film"} message={state.error.message}
      action={<button className="btn" onClick={() => navigate('/')}>Back to browse</button>} /></main>;
  }
  const { title, progress } = state.data;
  const resume = progress && shouldOfferResume(progress.positionSeconds, progress.durationSeconds) ? progress.positionSeconds : null;
  return (
    <main className="detail">
      <div className="art" style={posterStyle(title.art)} role="img" aria-label={`Poster art for ${title.name}`} />
      <div>
        <h1>{title.name}</h1>
        <p className="meta">{title.year} · {title.country} · {runtimeLabel(title.runtimeMinutes)} · {title.genres.join(', ')} · Directed by {title.director}</p>
        <p>{title.synopsis}</p>
        <div className="actions">
          <button className="btn primary" onClick={() => navigate(`/watch/${title.id}`)}>{resume !== null ? `▶ Resume at ${formatTimecode(resume)}` : '▶ Play'}</button>
          <button className="btn" onClick={() => navigate('/')}>Back</button>
        </div>
      </div>
    </main>
  );
}
