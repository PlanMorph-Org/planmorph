'use client';

import { useEffect } from 'react';
import { SUPPORTED_CURRENCIES } from '@/src/lib/currency';
import { useCurrencyStore } from '@/src/store/currencyStore';

interface CurrencySelectorProps {
  className?: string;
}

export default function CurrencySelector({ className }: CurrencySelectorProps) {
  const { currency, setCurrency, loadRates, isLoading } = useCurrencyStore();

  useEffect(() => {
    loadRates();
  }, [loadRates]);

  return (
    <div className={`flex items-center gap-2 ${className ?? ''}`}>
      <span className="text-xs text-gray-500">Currency</span>
      <select
        value={currency}
        onChange={(e) => setCurrency(e.target.value)}
        className="border border-gray-300 rounded-md text-xs px-2 py-1 text-gray-700 focus:outline-none focus:ring-1 focus:ring-blue-500"
        aria-label="Select currency"
        disabled={isLoading}
      >
        {SUPPORTED_CURRENCIES.map((code) => (
          <option key={code} value={code}>
            {code}
          </option>
        ))}
      </select>
    </div>
  );
}
