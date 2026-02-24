'use client';

import { useEffect, useState } from 'react';
import toast, { Toaster } from 'react-hot-toast';
import StudentLayout from '@/src/components/StudentLayout';
import { useRoleGuard } from '@/src/hooks/useRoleGuard';
import api from '@/src/lib/api';

interface StudentTaskEarning {
  projectId: string;
  projectNumber: string;
  taskTitle: string;
  taskCategory: string;
  organisationSetFee: number;
  earnedAtUtc: string;
  paymentStatus: string;
}

interface CashoutItem {
  id: string;
  amount: number;
  channel: 'Bank' | 'MobileMoney';
  destinationMasked: string;
  status: 'Processing' | 'Completed' | 'Failed';
  reference: string;
  createdAtUtc: string;
}

interface StudentEarningsSummary {
  role: string;
  totalEarnings: number;
  totalSuccessfulCashouts: number;
  availableBalance: number;
  reserveAmount: number;
  withdrawableBalance: number;
  canCashoutToday: boolean;
  taskEarnings: StudentTaskEarning[];
  cashouts: CashoutItem[];
}

interface PayoutOption { name: string; code: string; type?: string; }

type Channel = 'Bank' | 'MobileMoney';

export default function StudentEarningsPage() {
  const { isAuthorized } = useRoleGuard({ requiredRole: 'Student', redirectTo: '/student/login', message: 'Access denied. Student account required.' });
  const [loading, setLoading] = useState(true);
  const [summary, setSummary] = useState<StudentEarningsSummary | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [payoutOptions, setPayoutOptions] = useState<PayoutOption[]>([]);
  const [optionsLoading, setOptionsLoading] = useState(false);
  const [cashout, setCashout] = useState({ amount: '', channel: 'Bank' as Channel, recipientName: '', accountNumber: '', mobileNumber: '', bankCode: '' });

  useEffect(() => { if (isAuthorized) void loadSummary(); }, [isAuthorized]);
  useEffect(() => { if (isAuthorized) void loadPayoutOptions(cashout.channel); }, [isAuthorized, cashout.channel]);

  const loadSummary = async () => {
    setLoading(true);
    try {
      const response = await api.get<StudentEarningsSummary>('/earnings/summary');
      setSummary(response.data);
    } catch {
      toast.error('Failed to load student earnings.');
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
    <StudentLayout>
      <Toaster position="top-right" toastOptions={{ style: { background: '#1F2937', color: '#E5E7EB', border: '1px solid rgba(255,255,255,0.08)' } }} />
      <div className="pt-24 pb-16 px-4 sm:px-6 lg:px-8 max-w-6xl mx-auto">
        <h1 className="text-2xl font-display font-bold text-white mb-2">Student Earnings</h1>
        <p className="text-sm text-white/40 mb-6">Earnings are set by the organisation per task and paid out securely through Paystack.</p>

        {loading || !summary ? <div className="glass-card rounded-xl p-8 text-white/40">Loading...</div> : (
          <>
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
              <div className="glass-card rounded-xl p-5"><p className="text-xs text-white/30">Total Earnings</p><p className="text-xl font-bold text-white">KES {summary.totalEarnings.toLocaleString()}</p></div>
              <div className="glass-card rounded-xl p-5"><p className="text-xs text-white/30">Paid Out</p><p className="text-xl font-bold text-white">KES {summary.totalSuccessfulCashouts.toLocaleString()}</p></div>
              <div className="glass-card rounded-xl p-5"><p className="text-xs text-white/30">Available</p><p className="text-xl font-bold text-white">KES {summary.availableBalance.toLocaleString()}</p></div>
              <div className="glass-card rounded-xl p-5"><p className="text-xs text-white/30">Withdrawable</p><p className="text-xl font-bold text-indigo">KES {summary.withdrawableBalance.toLocaleString()}</p></div>
            </div>

            <div className="glass-card rounded-xl p-6 mb-8">
              <h2 className="text-sm font-semibold text-white mb-2">Cashout (Paystack)</h2>
              <p className="text-xs text-white/30 mb-4">One cashout/day. KES {summary.reserveAmount.toLocaleString()} must remain in account.</p>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <input className="glass-input rounded-lg px-4 py-3 text-sm text-white" placeholder="Amount (KES)" value={cashout.amount} onChange={(e) => setCashout((s) => ({ ...s, amount: e.target.value }))} />
                <select className="glass-input rounded-lg px-4 py-3 text-sm text-white" value={cashout.channel} onChange={(e) => setCashout((s) => ({ ...s, channel: e.target.value as Channel }))}><option className="bg-brand" value="Bank">Bank</option><option className="bg-brand" value="MobileMoney">Mobile Money</option></select>
                <input className="glass-input rounded-lg px-4 py-3 text-sm text-white" placeholder="Recipient Name" value={cashout.recipientName} onChange={(e) => setCashout((s) => ({ ...s, recipientName: e.target.value }))} />
                <input className="glass-input rounded-lg px-4 py-3 text-sm text-white" placeholder={cashout.channel === 'Bank' ? 'Account Number' : 'Mobile Number'} value={cashout.channel === 'Bank' ? cashout.accountNumber : cashout.mobileNumber} onChange={(e) => setCashout((s) => cashout.channel === 'Bank' ? { ...s, accountNumber: e.target.value } : { ...s, mobileNumber: e.target.value })} />
                <select className="glass-input rounded-lg px-4 py-3 text-sm text-white md:col-span-2" value={cashout.bankCode} onChange={(e) => setCashout((s) => ({ ...s, bankCode: e.target.value }))}><option className="bg-brand" value="" disabled>{optionsLoading ? 'Loading providers...' : `Select ${cashout.channel === 'Bank' ? 'bank' : 'mobile provider'}`}</option>{payoutOptions.map((option) => (<option className="bg-brand" key={option.code} value={option.code}>{option.name}</option>))}</select>
              </div>
              <button onClick={submitCashout} disabled={submitting || !summary.canCashoutToday || summary.withdrawableBalance <= 0} className="mt-4 px-5 py-2.5 bg-indigo text-white font-semibold rounded-lg hover:bg-indigo-light transition-all disabled:opacity-40">{submitting ? 'Processing...' : 'Request Cashout'}</button>
            </div>

            <div className="glass-card rounded-xl overflow-hidden mb-8">
              <div className="px-5 py-4 border-b border-white/6"><h2 className="text-sm font-semibold text-white">Task Earnings (Organisation-Set)</h2></div>
              <div className="overflow-x-auto"><table className="min-w-full text-sm"><thead><tr className="text-white/30 text-[10px] uppercase tracking-widest"><th className="px-5 py-3 text-left">Project</th><th className="px-5 py-3 text-left">Task</th><th className="px-5 py-3 text-left">Category</th><th className="px-5 py-3 text-left">Fee</th><th className="px-5 py-3 text-left">Date</th></tr></thead><tbody className="divide-y divide-white/6">{summary.taskEarnings.map((item) => (<tr key={item.projectId}><td className="px-5 py-4 text-white/60 font-mono">{item.projectNumber}</td><td className="px-5 py-4 text-white/50">{item.taskTitle}</td><td className="px-5 py-4 text-white/40">{item.taskCategory}</td><td className="px-5 py-4 text-verified font-semibold">KES {item.organisationSetFee.toLocaleString()}</td><td className="px-5 py-4 text-white/30 text-xs">{new Date(item.earnedAtUtc).toLocaleDateString()}</td></tr>))}</tbody></table></div>
            </div>

            <div className="glass-card rounded-xl overflow-hidden">
              <div className="px-5 py-4 border-b border-white/6"><h2 className="text-sm font-semibold text-white">Cashout History</h2></div>
              <div className="overflow-x-auto"><table className="min-w-full text-sm"><thead><tr className="text-white/30 text-[10px] uppercase tracking-widest"><th className="px-5 py-3 text-left">Reference</th><th className="px-5 py-3 text-left">Amount</th><th className="px-5 py-3 text-left">Channel</th><th className="px-5 py-3 text-left">Destination</th><th className="px-5 py-3 text-left">Status</th></tr></thead><tbody className="divide-y divide-white/6">{summary.cashouts.map((item) => (<tr key={item.id}><td className="px-5 py-4 text-white/60 font-mono">{item.reference}</td><td className="px-5 py-4 text-white/50">KES {item.amount.toLocaleString()}</td><td className="px-5 py-4 text-white/40">{item.channel}</td><td className="px-5 py-4 text-white/40">{item.destinationMasked}</td><td className="px-5 py-4 text-white/50">{item.status}</td></tr>))}</tbody></table></div>
            </div>
          </>
        )}
      </div>
    </StudentLayout>
  );
}
