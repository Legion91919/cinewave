import { render, screen } from '@testing-library/react';
import type { ContinueItem } from '../api/types';
import { ContinueWatchingRow } from './ContinueWatchingRow';

const item: ContinueItem = {
  titleId: 'nightbus-to-bulawayo',
  title: { id: 'nightbus-to-bulawayo', name: 'Nightbus to Bulawayo', year: 2022, country: 'Zimbabwe', runtimeMinutes: 98, genres: ['Drama'], synopsis: '', director: '', art: { palette: ['#7a2e1f', '#a5532a'], pattern: 'stripes' } },
  positionSeconds: 750, durationSeconds: 1200, remainingSeconds: 450, deviceId: 'phone-1', deviceName: 'phone', updatedAt: '2026-11-19T19:00:00Z',
};

describe('ContinueWatchingRow', () => {
  it('success: shows resume time and the device it was stopped on', () => {
    render(<ContinueWatchingRow state={{ status: 'success', data: [item] }} onResume={() => {}} />);
    expect(screen.getByRole('link', { name: /Resume Nightbus to Bulawayo at 12:30/ })).toBeTruthy();
    expect(screen.getByText(/stopped on your phone/)).toBeTruthy();
  });

  it('empty and error: the row is hidden so browsing still works', () => {
    const { container, rerender } = render(<ContinueWatchingRow state={{ status: 'success', data: [] }} onResume={() => {}} />);
    expect(container.innerHTML).toBe('');
    rerender(<ContinueWatchingRow state={{ status: 'error', error: new Error('x') }} onResume={() => {}} />);
    expect(container.innerHTML).toBe('');
  });

  it('loading: shows skeletons, hidden from screen readers', () => {
    const { container } = render(<ContinueWatchingRow state={{ status: 'loading' }} onResume={() => {}} />);
    expect(container.querySelector('[aria-hidden="true"] .skel')).toBeTruthy();
  });
});
