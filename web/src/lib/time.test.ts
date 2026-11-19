import { formatTimecode, minutesLeft, runtimeLabel } from './time';
import { shouldOfferResume, shouldReport } from './resume';

describe('timecodes', () => {
  it('formats 12:30 and hour-long positions', () => {
    expect(formatTimecode(750)).toBe('12:30');
    expect(formatTimecode(3725)).toBe('1:02:05');
    expect(formatTimecode(-4)).toBe('0:00');
  });
  it('rounds minutes left and runtime labels', () => {
    expect(minutesLeft(750, 5880)).toBe(86);
    expect(runtimeLabel(112)).toBe('1h 52m');
  });
});

describe('resume prompt rules', () => {
  it('offers resume only between 30s and 95%', () => {
    expect(shouldOfferResume(750, 1200)).toBe(true);
    expect(shouldOfferResume(12, 1200)).toBe(false);
    expect(shouldOfferResume(1180, 1200)).toBe(false);
    expect(shouldOfferResume(750, null)).toBe(true);
  });
  it('reports progress every 10 seconds or when forced', () => {
    expect(shouldReport(null, 0.2, false)).toBe(false);
    expect(shouldReport(null, 3, false)).toBe(true);
    expect(shouldReport(100, 105, false)).toBe(false);
    expect(shouldReport(100, 110, false)).toBe(true);
    expect(shouldReport(100, 101, true)).toBe(true);
  });
});
