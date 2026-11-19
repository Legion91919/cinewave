import type { ContinueItem } from '../api/types';
import type { AsyncState } from '../lib/useAsync';
import { formatTimecode, minutesLeft } from '../lib/time';
import { posterStyle } from './art';
import { RowSkeleton } from './CatalogueRow';

/** First row under the hero. Loading shows skeletons; empty and error hide the row so the catalogue still works. */
export function ContinueWatchingRow({ state, onResume }: { state: AsyncState<ContinueItem[]>; onResume: (titleId: string) => void }) {
  if (state.status === 'loading') return <RowSkeleton still />;
  if (state.status === 'error' || state.data.length === 0) return null;
  return (
    <section className="row" aria-labelledby="row-continue">
      <h2 id="row-continue">Continue watching</h2>
      <ul className="strip">
        {state.data.map((c) => {
          const at = formatTimecode(c.positionSeconds);
          const left = minutesLeft(c.positionSeconds, c.durationSeconds);
          const pct = Math.min(100, Math.round((c.positionSeconds / c.durationSeconds) * 100));
          return (
            <li key={c.titleId}>
              <a href={`/watch/${c.titleId}`} className="still" style={posterStyle(c.title.art)}
                aria-label={`Resume ${c.title.name} at ${at}, ${left} minutes left`}
                onClick={(e) => { e.preventDefault(); onResume(c.titleId); }}>
                <span className="pill">Resume at {at}</span>
                <span className="t">{c.title.name}</span>
                <span className="bar"><i style={{ width: `${pct}%` }} /></span>
              </a>
              <div className="cap"><strong>{left} min left</strong> · stopped on your {c.deviceName}</div>
            </li>
          );
        })}
      </ul>
    </section>
  );
}
