export const BASE_CURRENCY = 'KES';
export const SUPPORTED_CURRENCIES = ['KES', 'USD', 'EUR', 'GBP', 'UGX', 'TZS', 'ZAR'];

const ZERO_DECIMAL_CURRENCIES = new Set(['KES', 'UGX', 'TZS']);

const getFractionDigits = (currency: string) => (ZERO_DECIMAL_CURRENCIES.has(currency) ? 0 : 2);

export const formatCurrency = (
  amountKes: number,
  currency: string,
  rates?: Record<string, number>
) => {
  if (!Number.isFinite(amountKes)) {
    return '';
  }

  const normalizedCurrency = currency?.toUpperCase() || BASE_CURRENCY;
  const hasRate = normalizedCurrency === BASE_CURRENCY || !!rates?.[normalizedCurrency];
  const effectiveCurrency = hasRate ? normalizedCurrency : BASE_CURRENCY;
  const rate = effectiveCurrency === BASE_CURRENCY ? 1 : rates?.[effectiveCurrency] ?? 1;
  const converted = amountKes * rate;
  const digits = getFractionDigits(effectiveCurrency);

  try {
    return new Intl.NumberFormat('en', {
      style: 'currency',
      currency: effectiveCurrency,
      minimumFractionDigits: digits,
      maximumFractionDigits: digits,
    }).format(converted);
  } catch {
    return `${effectiveCurrency} ${converted.toLocaleString()}`;
  }
};
