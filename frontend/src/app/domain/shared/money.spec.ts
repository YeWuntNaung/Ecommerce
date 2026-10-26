import { formatKyat, formatWeight } from './money';

describe('formatKyat', () => {
  it('formats whole kyat with separators', () => {
    expect(formatKyat(85_000)).toBe('85,000 Ks');
    expect(formatKyat(0)).toBe('0 Ks');
    expect(formatKyat(1_250_000)).toBe('1,250,000 Ks');
  });
});

describe('formatWeight', () => {
  it('renders grams as kilograms', () => {
    expect(formatWeight(200)).toBe('0.200 kg');
    expect(formatWeight(999)).toBe('0.999 kg');
    expect(formatWeight(1_000)).toBe('1 kg');
    expect(formatWeight(1_500)).toBe('1.500 kg');
  });
});
