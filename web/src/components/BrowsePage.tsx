import { api } from '../api/client';
import { navigate } from '../lib/router';
import { useAsync } from '../lib/useAsync';
import { CatalogueRow, RowSkeleton } from './CatalogueRow';
import { ContinueWatchingRow } from './ContinueWatchingRow';
import { Hero } from './Hero';
import { Notice } from './Notice';

export function BrowsePage({ profileId }: { profileId: string }) {
  const browse = useAsync((s) => api.browse(profileId, s), [profileId]);
  const cw = useAsync((s) => api.continueWatching(profileId, s), [profileId]);

  if (browse.status === 'loading') {
    return (
      <main aria-busy="true" aria-label="Loading catalogue">
        <section className="hero"><div className="skel" style={{ width: 420, maxWidth: '80%', height: 48 }} /><div className="skel" style={{ width: 520, maxWidth: '90%', height: 40 }} /></section>
        <RowSkeleton still /><RowSkeleton />
      </main>
    );
  }
  if (browse.status === 'error') {
    return <main><Notice kind="error" title="We couldn't load the catalogue" message={`Check your connection and try again. (${browse.error.message})`}
      action={<button className="btn primary" onClick={browse.retry}>Try again</button>} /></main>;
  }
  const page = browse.data;
  if (page.rows.length === 0 || !page.hero) {
    return <main><Notice kind="empty" title="Nothing licensed here yet" message="We don't have streaming rights for any films in your country right now. New licences arrive every week." /></main>;
  }
  const hero = page.hero;
  return (
    <main>
      <Hero title={hero} onPlay={() => navigate(`/watch/${hero.id}`)} onMore={() => navigate(`/title/${hero.id}`)} />
      <ContinueWatchingRow state={cw} onResume={(id) => navigate(`/watch/${id}`)} />
      {page.rows.map((r) => <CatalogueRow key={r.id} row={r} onOpen={(id) => navigate(`/title/${id}`)} />)}
    </main>
  );
}
