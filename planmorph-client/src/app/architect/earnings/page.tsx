'use client';

import { useEffect, useState } from 'react';
import ArchitectLayout from '@/src/components/ArchitectLayout';
// import api from '@/src/lib/api';
import toast, { Toaster } from 'react-hot-toast';
import { useRoleGuard } from '@/src/hooks/useRoleGuard';
import { useCurrencyStore } from '@/src/store/currencyStore';
import { formatCurrency } from '@/src/lib/currency';

interface Sale {
  id: string;
  orderNumber: string;
  designTitle: string;
  amount: number;
  date: string;
  commission: number;
}

export default function ArchitectEarningsPage() {
  const { isAuthorized } = useRoleGuard({
    requiredRole: 'Architect',
    redirectTo: '/architect/login',
    message: 'Access denied. Architect account required.',
  });
  const [sales, setSales] = useState<Sale[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [stats, setStats] = useState({
    totalEarnings: 0,
    thisMonth: 0,
    totalSales: 0,
    pending: 0,
  });
  const { currency, rates } = useCurrencyStore();

  useEffect(() => {
    if (isAuthorized) {
      loadEarnings();
    }
  }, [isAuthorized]);

  const loadEarnings = async () => {
    setIsLoading(true);
    try {
      // TODO: Create endpoint to get architect sales
      // For now, using mock data
      const mockSales: Sale[] = [
        {
          id: '1',
          orderNumber: 'PM-20250208-12345',
          designTitle: 'Modern 3-Bedroom Bungalow',
          amount: 50000,
          date: '2025-02-01',
          commission: 50000 * 0.7, // 70% to architect
        },
        {
          id: '2',
          orderNumber: 'PM-20250207-67890',
          designTitle: 'Luxury 4-Bedroom Mansion',
          amount: 120000,
          date: '2025-01-28',
          commission: 120000 * 0.7,
        },
      ];

      setSales(mockSales);
      
      const total = mockSales.reduce((sum, sale) => sum + sale.commission, 0);
      const currentMonth = new Date().getMonth();
      const thisMonth = mockSales
        .filter(sale => new Date(sale.date).getMonth() === currentMonth)
        .reduce((sum, sale) => sum + sale.commission, 0);

      setStats({
        totalEarnings: total,
        thisMonth: thisMonth,
        totalSales: mockSales.length,
        pending: 0,
      });
    } catch (error) {
      toast.error('Failed to load earnings data');
    } finally {
      setIsLoading(false);
    }
  };

  if (isLoading) {
    return (
      <ArchitectLayout>
        <div className="flex justify-center items-center h-screen">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        </div>
      </ArchitectLayout>
    );
  }

  return (
    <ArchitectLayout>
      <Toaster position="top-right" />
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Earnings</h1>
          <p className="text-gray-600 mt-2">Track your design sales and commissions</p>
          <p className="text-sm text-gray-500 mt-1">
            Amounts shown in {currency}. Payouts are processed in KES.
          </p>
        </div>

        {/* Stats Grid */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
          <div className="bg-white rounded-lg shadow p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0 bg-green-100 rounded-md p-3">
                <svg className="h-6 w-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-500">Total Earnings</p>
                <p className="text-2xl font-semibold text-gray-900">
                  {formatCurrency(stats.totalEarnings, currency, rates)}
                </p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0 bg-blue-100 rounded-md p-3">
                <svg className="h-6 w-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-500">This Month</p>
                <p className="text-2xl font-semibold text-gray-900">
                  {formatCurrency(stats.thisMonth, currency, rates)}
                </p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0 bg-purple-100 rounded-md p-3">
                <svg className="h-6 w-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
                </svg>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-500">Total Sales</p>
                <p className="text-2xl font-semibold text-gray-900">{stats.totalSales}</p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0 bg-yellow-100 rounded-md p-3">
                <svg className="h-6 w-6 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-500">Pending</p>
                <p className="text-2xl font-semibold text-gray-900">
                  {formatCurrency(stats.pending, currency, rates)}
                </p>
              </div>
            </div>
          </div>
        </div>

        {/* Commission Info */}
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-6 mb-8">
          <h3 className="text-lg font-semibold text-blue-900 mb-2">Commission Structure</h3>
          <p className="text-blue-800">
            You receive <strong>70%</strong> of each design sale. PlanMorph retains 30% for platform maintenance, 
            marketing, payment processing, and customer support.
          </p>
        </div>

        {/* Sales Table */}
        <div className="bg-white shadow rounded-lg overflow-hidden">
          <div className="px-6 py-4 border-b border-gray-200">
            <h2 className="text-lg font-semibold text-gray-900">Sales History</h2>
          </div>

          {sales.length === 0 ? (
            <div className="text-center py-12">
              <svg className="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <h3 className="mt-2 text-sm font-medium text-gray-900">No sales yet</h3>
              <p className="mt-1 text-sm text-gray-500">Your earnings will appear here once clients purchase your designs.</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Order</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Design</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Sale Price</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Your Commission (70%)</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Date</th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {sales.map((sale) => (
                    <tr key={sale.id}>
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                        {sale.orderNumber}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {sale.designTitle}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        {formatCurrency(sale.amount, currency, rates)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-semibold text-green-600">
                        {formatCurrency(sale.commission, currency, rates)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {new Date(sale.date).toLocaleDateString()}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>

        {/* Withdrawal Section */}
        <div className="mt-8 bg-white shadow rounded-lg p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Withdraw Earnings</h3>
          <p className="text-sm text-gray-600 mb-4">
            Request a withdrawal to receive your earnings. Minimum withdrawal amount is{' '}
            {formatCurrency(10000, currency, rates)} (paid in KES).
          </p>
          <button
            className="px-6 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:bg-gray-400"
            disabled={stats.totalEarnings < 10000}
          >
            Request Withdrawal
          </button>
          {stats.totalEarnings < 10000 && (
            <p className="text-sm text-gray-500 mt-2">
              Minimum withdrawal amount not met. Current balance: {formatCurrency(stats.totalEarnings, currency, rates)}
            </p>
          )}
        </div>
      </div>
    </ArchitectLayout>
  );
}
