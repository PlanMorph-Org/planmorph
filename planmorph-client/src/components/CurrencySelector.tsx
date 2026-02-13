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
      <span className="text-xs text-white/40">Currency</span>
      <select
        value={currency}
        onChange={(e) => setCurrency(e.target.value)}
        className="border border-white/10 bg-white/10 backdrop-blur-sm rounded-lg text-xs px-2 py-1.5 text-white focus:outline-none focus:ring-1 focus:ring-white/30"
        aria-label="Select currency"
        disabled={isLoading}
      >
        {SUPPORTED_CURRENCIES.map((code) => (
          <option key={code} value={code} className="text-gray-900 bg-white">
            {code}
          </option>
        ))}
      </select>
    </div>
  );
}
