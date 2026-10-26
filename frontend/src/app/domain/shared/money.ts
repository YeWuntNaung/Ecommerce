export const MMK = 'MMK';

const kyatFormatter = new Intl.NumberFormat('en-US', { maximumFractionDigits: 0 });

/** MMK is displayed in whole kyat, matching how the API stores it. */
export function formatKyat(amount: number): string {
  return `${kyatFormatter.format(amount)} Ks`;
}

/** The API stores grams; the UI talks kilograms, as the shipping rules are defined in kg. */
export function formatWeight(grams: number): string {
  return `${(grams / 1000).toFixed(grams % 1000 === 0 ? 0 : 3)} kg`;
}
