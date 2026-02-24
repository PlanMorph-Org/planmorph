'use client';

import { useState } from 'react';
import Link from 'next/link';
import api from '@/src/lib/api';
import toast, { Toaster } from 'react-hot-toast';

type Portal = 'client' | 'architect' | 'engineer';

interface PasswordResetRequestPageProps {
  portal: Portal;
  title: string;
  subtitle: string;
  loginHref: string;
  accentClass: string;
  helperText?: string;
}

export default function PasswordResetRequestPage({
  portal,
  title,
  subtitle,
  loginHref,
  accentClass,
  helperText,
}: PasswordResetRequestPageProps) {
  const [email, setEmail] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitted, setSubmitted] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);

    try {
      await api.post('/users/forgot-password', {
        email,
        portal,
      });

      setSubmitted(true);
      toast.success('If the email exists, a reset link has been sent.');
    } catch {
      toast.error('Unable to submit request. Please try again.');
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

        {helperText ? (
          <div className="glass-card-light rounded-lg p-3 border border-white/10 mb-6">
            <p className="text-xs text-white/55">{helperText}</p>
          </div>
        ) : null}

        {submitted ? (
          <div className="space-y-4">
            <p className="text-sm text-white/70">Check your email for password reset instructions.</p>
            <Link href={loginHref} className={`text-sm font-medium hover-underline ${accentClass}`}>
              Back to sign in
            </Link>
          </div>
        ) : (
          <form onSubmit={handleSubmit} className="space-y-5">
            <div>
              <label htmlFor="email" className="block text-xs font-medium text-white/40 mb-1.5">Email</label>
              <input
                id="email"
                name="email"
                type="email"
                required
                autoComplete="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="you@example.com"
                className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20"
              />
            </div>

            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full py-3 bg-brand-accent text-white font-semibold rounded-lg hover:bg-blue-500 transition-all duration-300 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isSubmitting ? 'Sending...' : 'Send reset link'}
            </button>

            <div className="text-center pt-2">
              <Link href={loginHref} className={`text-xs transition-colors hover-underline ${accentClass}`}>
                Back to sign in
              </Link>
            </div>
          </form>
        )}
      </div>
    </div>
  );
}
