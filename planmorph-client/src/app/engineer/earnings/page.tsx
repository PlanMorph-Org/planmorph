'use client';

import { useEffect, useState } from 'react';
import toast, { Toaster } from 'react-hot-toast';
import EngineerLayout from '@/src/components/EngineerLayout';
import { useRoleGuard } from '@/src/hooks/useRoleGuard';
import { useCurrencyStore } from '@/src/store/currencyStore';
import { formatCurrency } from '@/src/lib/currency';
import api from '@/src/lib/api';

interface SaleItem { orderId: string; orderNumber: string; designTitle: string; saleAmount: number; commissionAmount: number; paidAtUtc: string; }
interface CashoutItem { id: string; amount: number; channel: 'Bank' | 'MobileMoney'; destinationMasked: string; status: 'Processing' | 'Completed' | 'Failed'; reference: string; createdAtUtc: string; }
interface EarningsSummary {
  role: string;
  totalEarnings: number;
  totalSuccessfulCashouts: number;
  availableBalance: number;
  reserveAmount: number;
  withdrawableBalance: number;
  canCashoutToday: boolean;
  thisMonthEarnings: number;
  totalSales: number;
  sales: SaleItem[];
  cashouts: CashoutItem[];
}

interface PayoutOption { name: string; code: string; type?: string; }

interface FoundingRoleStatus { slotLimit: number; filled: number; remaining: number; }
interface FoundingCommissionStatus {
  architect: FoundingRoleStatus;
  engineer: FoundingRoleStatus;
  currentUserRole: string;
  currentUserIsFoundingMember: boolean;
  currentUserFoundingSlot?: number;
  updatedAtUtc: string;
}

type Channel = 'Bank' | 'MobileMoney';

export default function EngineerEarningsPage() {
  const { isAuthorized } = useRoleGuard({ requiredRole: 'Engineer', redirectTo: '/engineer/login', message: 'Access denied. Engineer account required.' });
  const { currency, rates } = useCurrencyStore();
  const [loading, setLoading] = useState(true);
  const [summary, setSummary] = useState<EarningsSummary | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [payoutOptions, setPayoutOptions] = useState<PayoutOption[]>([]);
  const [optionsLoading, setOptionsLoading] = useState(false);
  const [foundingStatus, setFoundingStatus] = useState<FoundingCommissionStatus | null>(null);
  const [cashout, setCashout] = useState({ amount: '', channel: 'Bank' as Channel, recipientName: '', accountNumber: '', mobileNumber: '', bankCode: '' });

  useEffect(() => { if (isAuthorized) void loadSummary(); }, [isAuthorized]);
  useEffect(() => { if (isAuthorized) void loadPayoutOptions(cashout.channel); }, [isAuthorized, cashout.channel]);
  useEffect(() => {
    if (!isAuthorized) return;

    void loadFoundingStatus();
    const intervalId = window.setInterval(() => {
      void loadFoundingStatus(true);
    }, 15000);

    return () => window.clearInterval(intervalId);
  }, [isAuthorized]);

  const loadSummary = async () => {
    setLoading(true);
    try {
      const response = await api.get<EarningsSummary>('/earnings/summary');
      setSummary(response.data);
    } catch {
      toast.error('Failed to load earnings.');
    } finally {
      setLoading(false);
    }
  };

  const loadPayoutOptions = async (channel: Channel) => {
    setOptionsLoading(true);
    try {
      const response = await api.get<PayoutOption[]>('/earnings/payout-options', { params: { channel: channel === 'Bank' ? 0 : 1 } });
      setPayoutOptions(response.data);
    } catch {
      setPayoutOptions([]);
      toast.error('Failed to load payout providers.');
    } finally {
      setOptionsLoading(false);
    }
  };

  const loadFoundingStatus = async (silent = false) => {
    try {
      const response = await api.get<FoundingCommissionStatus>('/earnings/commission/founding-status');
      setFoundingStatus(response.data);
    } catch {
      if (!silent) {
        toast.error('Failed to load 0% commission slot status.');
      }
    }
  };

  const submitCashout = async () => {
    if (!summary) return;
    const amount = Number(cashout.amount);
    if (!Number.isFinite(amount) || amount <= 0) { toast.error('Enter a valid amount.'); return; }

    setSubmitting(true);
    try {
      await api.post('/earnings/cashout', {
        amount,
        channel: cashout.channel,
        recipientName: cashout.recipientName,
        accountNumber: cashout.channel === 'Bank' ? cashout.accountNumber : undefined,
        mobileNumber: cashout.channel === 'MobileMoney' ? cashout.mobileNumber : undefined,
        bankCode: cashout.bankCode,
      });
      toast.success('Cashout processed.');
      setCashout({ amount: '', channel: 'Bank', recipientName: '', accountNumber: '', mobileNumber: '', bankCode: '' });
      await loadSummary();
    } catch {
      toast.error('Cashout failed.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <EngineerLayout>
      <Toaster position="top-right" toastOptions={{ style: { background: '#1F2937', color: '#E5E7EB', border: '1px solid rgba(255,255,255,0.08)' } }} />
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <h1 className="text-2xl font-display font-bold text-white mb-2">Engineer Earnings</h1>
        <p className="text-sm text-white/40 mb-6">Engineering design sales and Paystack-secured cashouts.</p>

        {loading || !summary ? <div className="glass-card rounded-xl p-8 text-white/40">Loading...</div> : (
          <>
            {foundingStatus ? (
              <div className="glass-card rounded-xl p-4 mb-6 border border-white/10">
                <p className="text-xs text-white/30 mb-1">0% Commission Founding Slots (Live)</p>
                <p className="text-sm text-white/75">
                  Architects: <span className="text-golden font-semibold">{foundingStatus.architect.filled}/{foundingStatus.architect.slotLimit}</span> filled ({foundingStatus.architect.remaining} left) • Engineers: <span className="text-slate-teal font-semibold">{foundingStatus.engineer.filled}/{foundingStatus.engineer.slotLimit}</span> filled ({foundingStatus.engineer.remaining} left)
                </p>
                <p className="text-xs text-white/50 mt-1">
                  {foundingStatus.currentUserIsFoundingMember
                    ? `You are on 0% commission${foundingStatus.currentUserFoundingSlot ? ` (slot #${foundingStatus.currentUserFoundingSlot})` : ''}.`
                    : `Your role currently has ${foundingStatus.currentUserRole === 'Architect' ? foundingStatus.architect.remaining : foundingStatus.engineer.remaining} slot(s) remaining.`}
                </p>
              </div>
            ) : null}

            <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
              <div className="glass-card rounded-xl p-5"><p className="text-xs text-white/30">Total Earnings</p><p className="text-xl font-bold text-verified">{formatCurrency(summary.totalEarnings, currency, rates)}</p></div>
              <div className="glass-card rounded-xl p-5"><p className="text-xs text-white/30">This Month</p><p className="text-xl font-bold text-brand-accent">{formatCurrency(summary.thisMonthEarnings, currency, rates)}</p></div>
              <div className="glass-card rounded-xl p-5"><p className="text-xs text-white/30">Withdrawable</p><p className="text-xl font-bold text-slate-teal">{formatCurrency(summary.withdrawableBalance, currency, rates)}</p></div>
              <div className="glass-card rounded-xl p-5"><p className="text-xs text-white/30">Sales</p><p className="text-xl font-bold text-white">{summary.totalSales}</p></div>
            </div>

            <div className="glass-card rounded-xl p-6 mb-8">
              <h2 className="text-sm font-semibold text-white mb-2">Cashout (Paystack)</h2>
              <p className="text-xs text-white/30 mb-4">One request/day. Minimum reserve: KES {summary.reserveAmount.toLocaleString()}.</p>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <input className="glass-input rounded-lg px-4 py-3 text-sm text-white" placeholder="Amount (KES)" value={cashout.amount} onChange={(e) => setCashout((s) => ({ ...s, amount: e.target.value }))} />
                <select className="glass-input rounded-lg px-4 py-3 text-sm text-white" value={cashout.channel} onChange={(e) => setCashout((s) => ({ ...s, channel: e.target.value as Channel }))}><option className="bg-brand" value="Bank">Bank</option><option className="bg-brand" value="MobileMoney">Mobile Money</option></select>
                <input className="glass-input rounded-lg px-4 py-3 text-sm text-white" placeholder="Recipient Name" value={cashout.recipientName} onChange={(e) => setCashout((s) => ({ ...s, recipientName: e.target.value }))} />
                <input className="glass-input rounded-lg px-4 py-3 text-sm text-white" placeholder={cashout.channel === 'Bank' ? 'Account Number' : 'Mobile Number'} value={cashout.channel === 'Bank' ? cashout.accountNumber : cashout.mobileNumber} onChange={(e) => setCashout((s) => cashout.channel === 'Bank' ? { ...s, accountNumber: e.target.value } : { ...s, mobileNumber: e.target.value })} />
                <select className="glass-input rounded-lg px-4 py-3 text-sm text-white md:col-span-2" value={cashout.bankCode} onChange={(e) => setCashout((s) => ({ ...s, bankCode: e.target.value }))}><option className="bg-brand" value="" disabled>{optionsLoading ? 'Loading providers...' : `Select ${cashout.channel === 'Bank' ? 'bank' : 'mobile provider'}`}</option>{payoutOptions.map((option) => (<option className="bg-brand" key={option.code} value={option.code}>{option.name}</option>))}</select>
              </div>
              <button onClick={submitCashout} disabled={submitting || !summary.canCashoutToday || summary.withdrawableBalance <= 0} className="mt-4 px-5 py-2.5 bg-slate-teal text-white font-semibold rounded-lg hover:bg-teal-500 transition-all disabled:opacity-40">{submitting ? 'Processing...' : 'Request Cashout'}</button>
            </div>

            <div className="glass-card rounded-xl overflow-hidden mb-8">
              <div className="px-5 py-4 border-b border-white/6"><h2 className="text-sm font-semibold text-white">Sales History</h2></div>
              <div className="overflow-x-auto"><table className="min-w-full text-sm"><thead><tr className="text-white/30 text-[10px] uppercase tracking-widest"><th className="px-5 py-3 text-left">Order</th><th className="px-5 py-3 text-left">Design</th><th className="px-5 py-3 text-left">Sale</th><th className="px-5 py-3 text-left">Commission</th><th className="px-5 py-3 text-left">Date</th></tr></thead><tbody className="divide-y divide-white/6">{summary.sales.map((sale) => (<tr key={sale.orderId}><td className="px-5 py-4 text-white/60 font-mono">{sale.orderNumber}</td><td className="px-5 py-4 text-white/50">{sale.designTitle}</td><td className="px-5 py-4 text-white/40">{formatCurrency(sale.saleAmount, currency, rates)}</td><td className="px-5 py-4 text-verified font-semibold">{formatCurrency(sale.commissionAmount, currency, rates)}</td><td className="px-5 py-4 text-white/30 text-xs">{new Date(sale.paidAtUtc).toLocaleDateString()}</td></tr>))}</tbody></table></div>
            </div>

            <div className="glass-card rounded-xl overflow-hidden">
              <div className="px-5 py-4 border-b border-white/6"><h2 className="text-sm font-semibold text-white">Cashout History</h2></div>
              <div className="overflow-x-auto"><table className="min-w-full text-sm"><thead><tr className="text-white/30 text-[10px] uppercase tracking-widest"><th className="px-5 py-3 text-left">Reference</th><th className="px-5 py-3 text-left">Amount</th><th className="px-5 py-3 text-left">Channel</th><th className="px-5 py-3 text-left">Destination</th><th className="px-5 py-3 text-left">Status</th></tr></thead><tbody className="divide-y divide-white/6">{summary.cashouts.map((item) => (<tr key={item.id}><td className="px-5 py-4 text-white/60 font-mono">{item.reference}</td><td className="px-5 py-4 text-white/50">{formatCurrency(item.amount, currency, rates)}</td><td className="px-5 py-4 text-white/40">{item.channel}</td><td className="px-5 py-4 text-white/40">{item.destinationMasked}</td><td className="px-5 py-4 text-white/50">{item.status}</td></tr>))}</tbody></table></div>
            </div>
          </>
        )}
      </div>
    </EngineerLayout>
  );
}
