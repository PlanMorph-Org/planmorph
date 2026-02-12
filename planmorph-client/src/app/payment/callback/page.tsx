'use client';

import { Suspense, useEffect, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import api from '@/src/lib/api';

function PaystackCallbackContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [status, setStatus] = useState('Verifying your payment...');

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
        setTimeout(() => router.push('/my-orders'), 1500);
      } catch {
        setStatus('Payment verification failed. Please contact support.');
      }
    };

    verify();
  }, [router, searchParams]);

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 px-4">
      <div className="bg-white rounded-lg shadow p-8 max-w-md text-center">
        <h1 className="text-xl font-semibold text-gray-900 mb-2">Payment Status</h1>
        <p className="text-sm text-gray-600">{status}</p>
      </div>
    </div>
  );
}

export default function PaystackCallbackPage() {
  return (
    <Suspense fallback={<div className="min-h-screen flex items-center justify-center bg-gray-50 px-4">Loading payment status...</div>}>
      <PaystackCallbackContent />
    </Suspense>
  );
}
