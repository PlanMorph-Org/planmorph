'use client';

import { Suspense, useEffect, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import api from '@/src/lib/api';

function PaystackCallbackContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [status, setStatus] = useState('Verifying your payment...');
  const [isSuccess, setIsSuccess] = useState<boolean | null>(null);

  useEffect(() => {
    const reference = searchParams.get('reference') || searchParams.get('trxref');
    if (!reference) {
      setStatus('Missing payment reference. Redirecting to orders...');
      const timer = setTimeout(() => router.push('/my-orders'), 1500);
      return () => clearTimeout(timer);
    }

    const verify = async () => {
      try {
        await api.get(`/payments/paystack/verify?reference=${encodeURIComponent(reference)}`);
        setStatus('Payment confirmed. Redirecting to your orders...');
        setIsSuccess(true);
        setTimeout(() => router.push('/my-orders'), 1500);
      } catch {
        setStatus('Payment verification failed. Please contact support.');
        setIsSuccess(false);
      }
    };

    verify();
  }, [router, searchParams]);

  return (
    <div className="min-h-screen flex items-center justify-center bg-brand px-4">
      <div className="glass-card rounded-2xl p-10 max-w-md w-full text-center border border-white/10">
        <div className="mb-4">
          {isSuccess === null && <div className="w-10 h-10 border-2 border-golden/30 border-t-golden rounded-full animate-spin mx-auto" />}
          {isSuccess === true && <div className="w-10 h-10 rounded-full bg-verified/20 flex items-center justify-center mx-auto"><svg className="w-5 h-5 text-verified" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" /></svg></div>}
          {isSuccess === false && <div className="w-10 h-10 rounded-full bg-rose-500/20 flex items-center justify-center mx-auto"><svg className="w-5 h-5 text-rose-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg></div>}
        </div>
        <h1 className="text-base font-semibold text-white mb-1">Payment Status</h1>
        <p className="text-sm text-white/40">{status}</p>
      </div>
    </div>
  );
}

export default function PaystackCallbackPage() {
  return (
    <Suspense fallback={<div className="min-h-screen flex items-center justify-center bg-brand px-4"><div className="w-8 h-8 border-2 border-golden/30 border-t-golden rounded-full animate-spin" /></div>}>
      <PaystackCallbackContent />
    </Suspense>
  );
}
