'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import api from '@/src/lib/api';
import toast, { Toaster } from 'react-hot-toast';
import { motion } from 'framer-motion';

export default function EngineerLoginPage() {
  const router = useRouter();
  const [formData, setFormData] = useState({ email: '', password: '' });
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    try {
      const response = await api.post('/auth/login', formData);
      const { token, role, email, firstName, lastName } = response.data;
      if (role !== 'Engineer') {
        toast.error('This login is for engineers only.');
        setIsLoading(false);
        return;
      }
      localStorage.setItem('token', token);
      localStorage.setItem('user', JSON.stringify({ email, firstName, lastName, role }));
      toast.success('Login successful!');
      router.push('/engineer/dashboard');
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Login failed. Please check your credentials.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  return (
    <div className="min-h-screen bg-brand flex relative overflow-hidden">
      <Toaster position="top-right" toastOptions={{ style: { background: '#1F2937', color: '#E5E7EB', border: '1px solid rgba(255,255,255,0.08)' } }} />
      <div className="absolute inset-0 blueprint-grid opacity-20" />
      <div className="absolute top-1/3 right-1/4 w-[500px] h-[500px] bg-slate-teal/[0.04] rounded-full blur-3xl" />

      {/* Left branding */}
      <div className="hidden lg:flex flex-1 flex-col justify-center items-center px-12 relative">
        <motion.div
          initial={{ opacity: 0, x: -30 }}
          animate={{ opacity: 1, x: 0 }}
          transition={{ duration: 0.8, ease: [0.16, 1, 0.3, 1] }}
          className="max-w-md"
        >
          <Link href="/" className="inline-flex items-center gap-2.5 mb-10">
            <img src="/planmorph.svg" alt="PlanMorph" className="h-9 w-auto brightness-0 invert rounded-full" />
            <span className="text-2xl font-display font-bold text-white">PlanMorph</span>
            <span className="px-2 py-0.5 text-[10px] font-semibold uppercase tracking-widest text-slate-teal bg-slate-teal/10 rounded-full border border-slate-teal/20">
              Engineer
            </span>
          </Link>
          <h2 className="text-3xl font-display font-bold text-white leading-snug mb-4">
            Verify designs. <br /><span className="text-gradient-blue">Protect builders.</span>
          </h2>
          <p className="text-white/35 leading-relaxed">
            Review architectural designs for structural integrity and completeness. Your verification ensures every published plan is build-ready.
          </p>
        </motion.div>
      </div>

      {/* Right form */}
      <div className="flex-1 flex items-center justify-center px-6 py-12 relative">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.2, duration: 0.6, ease: [0.16, 1, 0.3, 1] }}
          className="w-full max-w-md"
        >
          <div className="glass-card rounded-2xl p-8 shadow-glass-lg">
            <div className="lg:hidden text-center mb-8">
              <Link href="/" className="inline-flex items-center gap-2">
                <img src="/planmorph.svg" alt="PlanMorph" className="h-8 w-auto brightness-0 invert rounded-full" />
                <span className="text-xl font-display font-bold text-white">PlanMorph</span>
              </Link>
              <span className="block mt-2 px-2 py-0.5 text-[10px] font-semibold uppercase tracking-widest text-slate-teal bg-slate-teal/10 rounded-full border border-slate-teal/20 w-fit mx-auto">
                Engineer Portal
              </span>
            </div>

            <h1 className="text-2xl font-display font-bold text-white mb-1">Engineer Sign In</h1>
            <p className="text-sm text-white/35 mb-8">
              Don&apos;t have an account?{' '}
              <Link href="/engineer/register" className="text-slate-teal hover:text-emerald-400 transition-colors">
                Apply as engineer
              </Link>
            </p>

            <form onSubmit={handleSubmit} className="space-y-5">
              <div>
                <label htmlFor="email" className="block text-xs font-medium text-white/40 mb-1.5">Email</label>
                <input id="email" name="email" type="email" required value={formData.email} onChange={handleChange}
                  placeholder="you@example.com"
                  className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20" />
              </div>
              <div>
                <label htmlFor="password" className="block text-xs font-medium text-white/40 mb-1.5">Password</label>
                <input id="password" name="password" type="password" required value={formData.password} onChange={handleChange}
                  placeholder="••••••••"
                  className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20" />
              </div>
              <button type="submit" disabled={isLoading}
                className="w-full py-3 bg-slate-teal text-white font-semibold rounded-lg hover:bg-teal-500 transition-all duration-300 btn-glow disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isLoading ? (
                  <span className="inline-flex items-center gap-2">
                    <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" /><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" /></svg>
                    Signing in...
                  </span>
                ) : 'Sign in'}
              </button>
            </form>

            <div className="mt-6 pt-6 border-t border-white/6 text-center space-y-2">
              <p className="text-xs text-white/25">Are you a client?</p>
              <Link href="/login" className="text-xs text-brand-accent/70 hover:text-brand-accent transition-colors hover-underline">
                Client Login
              </Link>
            </div>
          </div>
        </motion.div>
      </div>
    </div>
  );
}
