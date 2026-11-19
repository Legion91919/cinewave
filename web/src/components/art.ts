import type { TitleArt } from '../api/types';

/** Generated poster art (no photos): a CSS gradient from the title's palette and pattern. */
export function posterStyle(art: TitleArt): React.CSSProperties {
  const [a, b, c = b] = art.palette;
  switch (art.pattern) {
    case 'stripes': return { background: `repeating-linear-gradient(115deg, ${a} 0 24px, ${b} 24px 48px, ${c} 48px 72px)` };
    case 'rings': return { background: `repeating-radial-gradient(circle at 30% 30%, ${a} 0 14px, ${b} 14px 28px)` };
    case 'grid': return { background: `linear-gradient(90deg, ${a}cc 50%, transparent 0) 0 0/32px 32px, linear-gradient(${b} 50%, ${c} 0) 0 0/32px 32px` };
    default: return { background: `linear-gradient(160deg, ${a}, ${b} 60%, ${c})` };
  }
}
