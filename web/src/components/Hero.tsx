import type { TitleSummary } from '../api/types';
import { runtimeLabel } from '../lib/time';
import { posterStyle } from './art';

export function Hero({ title, onPlay, onMore }: { title: TitleSummary; onPlay: () => void; onMore: () => void }) {
  const art = posterStyle(title.art);
  return (
    <section className="hero" aria-labelledby="hero-title"
      style={{ background: `linear-gradient(90deg, var(--color-bg) 25%, transparent 70%), linear-gradient(0deg, var(--color-bg), transparent 40%), ${art.background}` }}>
      <span className="eyebrow">New this week</span>
      <h1 id="hero-title">{title.name}</h1>
      <span className="meta">{title.year} · {title.country} · {runtimeLabel(title.runtimeMinutes)} · {title.genres.join(', ')}</span>
      <p>{title.synopsis}</p>
      <div className="actions">
        <button className="btn primary" onClick={onPlay}>▶ Play</button>
        <button className="btn" onClick={onMore}>More info</button>
      </div>
    </section>
  );
}
