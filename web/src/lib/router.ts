import { useEffect, useState } from 'react';

export type Route =
  | { name: 'browse' }
  | { name: 'title'; titleId: string }
  | { name: 'watch'; titleId: string }
  | { name: 'renew' };

export function parseRoute(pathname: string): Route {
  const [, a, b] = pathname.split('/');
  if (a === 'title' && b) return { name: 'title', titleId: decodeURIComponent(b) };
  if (a === 'watch' && b) return { name: 'watch', titleId: decodeURIComponent(b) };
  if (a === 'account' && b === 'renew') return { name: 'renew' };
  return { name: 'browse' };
}

export function navigate(path: string) {
  window.history.pushState(null, '', path);
  window.dispatchEvent(new PopStateEvent('popstate'));
}

export function useRoute(): Route {
  const [route, setRoute] = useState(() => parseRoute(window.location.pathname));
  useEffect(() => {
    const on = () => { setRoute(parseRoute(window.location.pathname)); window.scrollTo?.(0, 0); };
    window.addEventListener('popstate', on);
    return () => window.removeEventListener('popstate', on);
  }, []);
  return route;
}
