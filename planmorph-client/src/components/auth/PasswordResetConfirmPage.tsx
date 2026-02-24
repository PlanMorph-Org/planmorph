'use client';

import { useMemo, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import Link from 'next/link';
import api from '@/src/lib/api';
import toast, { Toaster } from 'react-hot-toast';

type Portal = 'client' | 'architect' | 'engineer';

interface PasswordResetConfirmPageProps {
  portal: Portal;
  title: string;
  subtitle: string;
  loginHref: string;
  accentClass: string;
  requireVerificationCode?: boolean;
  passwordHint?: string;
}

export default function PasswordResetConfirmPage({
  portal,
  title,
  subtitle,
  loginHref,
  accentClass,
  requireVerificationCode = false,
  passwordHint,
}: PasswordResetConfirmPageProps) {
  const router = useRouter();
  const searchParams = useSearchParams();

  const initialToken = useMemo(() => searchParams.get('token') ?? '', [searchParams]);
  const initialEmail = useMemo(() => searchParams.get('email') ?? '', [searchParams]);

  const [formData, setFormData] = useState({
    email: initialEmail,
    token: initialToken,
    verificationCode: '',
    newPassword: '',
    confirmPassword: '',
  });
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData((current) => ({ ...current, [e.target.name]: e.target.value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (formData.newPassword !== formData.confirmPassword) {
      toast.error('Passwords do not match.');
      return;
    }

    setIsSubmitting(true);

    try {
      await api.post('/users/reset-password', {
        email: formData.email,
        token: formData.token,
        newPassword: formData.newPassword,
        portal,
        verificationCode: requireVerificationCode ? formData.verificationCode : undefined,
      });

      toast.success('Password reset successfully.');
      setTimeout(() => router.push(loginHref), 800);
    } catch (error: unknown) {
      const message =
        typeof error === 'object' &&
        error !== null &&
        'response' in error &&
        typeof (error as { response?: { data?: { message?: string } } }).response?.data?.message === 'string'
          ? (error as { response?: { data?: { message?: string } } }).response?.data?.message
          : 'Password reset failed. Please try again.';

      toast.error(message ?? 'Password reset failed. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen bg-brand flex items-center justify-center px-6 py-12">
      <Toaster position="top-right" toastOptions={{ style: { background: '#1F2937', color: '#E5E7EB', border: '1px solid rgba(255,255,255,0.08)' } }} />
      <div className="w-full max-w-md glass-card rounded-2xl p-8 shadow-glass-lg">
        <h1 className="text-2xl font-display font-bold text-white mb-2">{title}</h1>
        <p className="text-sm text-white/40 mb-6">{subtitle}</p>

        {passwordHint ? (
          <div className="glass-card-light rounded-lg p-3 border border-white/10 mb-5">
            <p className="text-xs text-white/55">{passwordHint}</p>
          </div>
        ) : null}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label htmlFor="email" className="block text-xs font-medium text-white/40 mb-1.5">Email</label>
            <input
              id="email"
              name="email"
              type="email"
              required
              autoComplete="email"
              value={formData.email}
              onChange={handleChange}
              className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20"
            />
          </div>

          <div>
            <label htmlFor="token" className="block text-xs font-medium text-white/40 mb-1.5">Reset Token</label>
            <input
              id="token"
              name="token"
              type="text"
              required
              value={formData.token}
              onChange={handleChange}
              className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20"
            />
          </div>

          {requireVerificationCode ? (
            <div>
              <label htmlFor="verificationCode" className="block text-xs font-medium text-white/40 mb-1.5">Verification Code</label>
              <input
                id="verificationCode"
                name="verificationCode"
                type="text"
                required
                maxLength={6}
                value={formData.verificationCode}
                onChange={handleChange}
                placeholder="6-digit code"
                className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20"
              />
            </div>
          ) : null}

          <div>
            <label htmlFor="newPassword" className="block text-xs font-medium text-white/40 mb-1.5">New Password</label>
            <input
              id="newPassword"
              name="newPassword"
              type="password"
              required
              value={formData.newPassword}
              onChange={handleChange}
              className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20"
            />
          </div>

          <div>
            <label htmlFor="confirmPassword" className="block text-xs font-medium text-white/40 mb-1.5">Confirm New Password</label>
            <input
              id="confirmPassword"
              name="confirmPassword"
              type="password"
              required
              value={formData.confirmPassword}
              onChange={handleChange}
              className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20"
            />
          </div>

          <button
            type="submit"
            disabled={isSubmitting}
            className="w-full py-3 bg-brand-accent text-white font-semibold rounded-lg hover:bg-blue-500 transition-all duration-300 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isSubmitting ? 'Resetting...' : 'Reset password'}
          </button>

          <div className="text-center pt-1">
            <Link href={loginHref} className={`text-xs transition-colors hover-underline ${accentClass}`}>
              Back to sign in
            </Link>
          </div>
        </form>
      </div>
    </div>
  );
}
