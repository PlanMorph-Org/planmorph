'use client';

import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import api from '@/src/lib/api';
import { BASE_CURRENCY, SUPPORTED_CURRENCIES } from '@/src/lib/currency';

interface CurrencyRatesResponse {
  base: string;
  asOf: string;
  source: string;
  supported: string[];
  rates: Record<string, number>;
}

interface CurrencyState {
  currency: string;
  rates: Record<string, number>;
  lastUpdated?: string;
  isLoading: boolean;
  setCurrency: (currency: string) => void;
  loadRates: () => Promise<void>;
}

export const useCurrencyStore = create<CurrencyState>()(
  persist(
    (set, get) => ({
      currency: BASE_CURRENCY,
      rates: { [BASE_CURRENCY]: 1 },
      lastUpdated: undefined,
      isLoading: false,
      setCurrency: (currency: string) => {
        const normalized = currency.toUpperCase();
        if (!SUPPORTED_CURRENCIES.includes(normalized)) {
          return;
        }
        set({ currency: normalized });
      },
      loadRates: async () => {
        if (get().isLoading) return;
        set({ isLoading: true });
        try {
          const response = await api.get<CurrencyRatesResponse>('/currency/rates');
          const data = response.data;
          const normalizedRates: Record<string, number> = { [BASE_CURRENCY]: 1 };

          if (data?.rates) {
            Object.entries(data.rates).forEach(([code, rate]) => {
              normalizedRates[code.toUpperCase()] = Number(rate);
            });
          }

          set({
            rates: normalizedRates,
            lastUpdated: data?.asOf,
            isLoading: false,
          });
        } catch {
          set({ isLoading: false });
        }
      },
    }),
    {
      name: 'currency-storage',
      partialize: (state) => ({
        currency: state.currency,
        rates: state.rates,
        lastUpdated: state.lastUpdated,
      }),
    }
  )
);
