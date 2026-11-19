import type { CatalogueRow as Row, TitleSummary } from '../api/types';
import { posterStyle } from './art';

export function PosterCard({ title, onOpen }: { title: TitleSummary; onOpen: (id: string) => void }) {
  return (
    <li>
      <a href={`/title/${title.id}`} className="poster" style={posterStyle(title.art)} aria-label={`${title.name}, ${title.year}, ${title.country}`}
        onClick={(e) => { e.preventDefault(); onOpen(title.id); }}>{title.name}</a>
      <div className="cap">{title.year} · {title.country}</div>
    </li>
  );
}

export function CatalogueRow({ row, onOpen }: { row: Row; onOpen: (id: string) => void }) {
  if (row.items.length === 0) return null;
  return (
    <section className="row" aria-labelledby={`row-${row.id}`}>
      <h2 id={`row-${row.id}`}>{row.title}</h2>
      <ul className="strip">{row.items.map((t) => <PosterCard key={t.id} title={t} onOpen={onOpen} />)}</ul>
    </section>
  );
}

export function RowSkeleton({ still = false }: { still?: boolean }) {
  const style = still ? { width: 'var(--still-w)', height: 'var(--still-h)' } : { width: 'var(--poster-w)', height: 'var(--poster-h)' };
  return (
    <section className="row" aria-hidden="true">
      <div className="skel" style={{ width: 220, height: 22, marginBottom: 12 }} />
      <ul className="strip">{[0, 1, 2, 3].map((i) => <li key={i} className="skel" style={style} />)}</ul>
    </section>
  );
}
