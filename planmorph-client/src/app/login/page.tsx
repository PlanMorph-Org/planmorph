'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { useAuthStore } from '../../store/authStore';
import toast, { Toaster } from 'react-hot-toast';
import { motion } from 'framer-motion';

export default function LoginPage() {
  const router = useRouter();
  const { login } = useAuthStore();
  const [formData, setFormData] = useState({ email: '', password: '' });
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    try {
      await login(formData.email, formData.password);
      toast.success('Signed in.');
      router.push('/designs');
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

      {/* Background decorations */}
      <div className="absolute inset-0 blueprint-grid opacity-20" />
      <div className="absolute top-1/3 right-1/4 w-[500px] h-[500px] bg-brand-accent/[0.04] rounded-full blur-3xl" />

      {/* Left: Branding panel (desktop only) */}
      <div className="hidden lg:flex flex-1 flex-col justify-center items-center px-12 relative">
        <div className="max-w-md">
          <motion.div
            initial={{ opacity: 0, x: -30 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ duration: 0.8, ease: [0.16, 1, 0.3, 1] }}
          >
            <Link href="/" className="inline-flex items-center gap-2.5 mb-10">
              <img src="/planmorph.svg" alt="PlanMorph" className="h-9 w-auto brightness-0 invert rounded-full" />
              <span className="text-2xl font-display font-bold text-white">PlanMorph</span>
            </Link>
            <h2 className="text-3xl font-display font-bold text-white leading-snug mb-4">
              Build-ready packages. <span className="text-gradient-golden">Delivered</span> instantly.
            </h2>
            <p className="text-white/35 leading-relaxed">
              License and download engineer-reviewed architectural and civil design packages—packaged for execution.
            </p>
          </motion.div>

          {/* Decorative structural illustration */}
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            transition={{ delay: 0.5, duration: 1 }}
            className="mt-16"
          >
            <svg viewBox="0 0 400 200" fill="none" className="w-full opacity-[0.06]">
              <rect x="50" y="30" width="120" height="150" stroke="currentColor" strokeWidth="1" className="text-brand-accent" />
              <rect x="230" y="60" width="120" height="120" stroke="currentColor" strokeWidth="1" className="text-golden" />
              <rect x="70" y="50" width="35" height="30" stroke="currentColor" strokeWidth="0.5" className="text-brand-accent" />
              <rect x="120" y="50" width="35" height="30" stroke="currentColor" strokeWidth="0.5" className="text-brand-accent" />
              <rect x="250" y="80" width="35" height="30" stroke="currentColor" strokeWidth="0.5" className="text-golden" />
              <rect x="300" y="80" width="35" height="30" stroke="currentColor" strokeWidth="0.5" className="text-golden" />
              <line x1="30" y1="180" x2="370" y2="180" stroke="currentColor" strokeWidth="1" className="text-steel" />
              <line x1="170" y1="100" x2="230" y2="100" stroke="currentColor" strokeWidth="0.5" strokeDasharray="4 4" className="text-steel" />
            </svg>
          </motion.div>
        </div>
      </div>

      {/* Right: Login form */}
      <div className="flex-1 flex items-center justify-center px-6 py-12 relative">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.2, duration: 0.6, ease: [0.16, 1, 0.3, 1] }}
          className="w-full max-w-md"
        >
          <div className="glass-card rounded-2xl p-8 shadow-glass-lg">
            {/* Logo (mobile only) */}
            <div className="lg:hidden text-center mb-8">
              <Link href="/" className="inline-flex items-center gap-2">
                <img src="/planmorph.svg" alt="PlanMorph" className="h-8 w-auto brightness-0 invert rounded-full" />
                <span className="text-xl font-display font-bold text-white">PlanMorph</span>
              </Link>
            </div>

            <h1 className="text-2xl font-display font-bold text-white mb-1">Sign in</h1>
            <p className="text-sm text-white/35 mb-8">
              Don&apos;t have an account?{' '}
              <Link href="/register" className="text-brand-accent hover:text-blue-400 transition-colors">
                Create one
              </Link>
            </p>

            <form onSubmit={handleSubmit} className="space-y-5">
              <div>
                <label htmlFor="email" className="block text-xs font-medium text-white/40 mb-1.5">Email</label>
                <input
                  id="email"
                  name="email"
                  type="email"
                  required
                  value={formData.email}
                  onChange={handleChange}
                  placeholder="you@example.com"
                  className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20"
                />
              </div>
              <div>
                <label htmlFor="password" className="block text-xs font-medium text-white/40 mb-1.5">Password</label>
                <input
                  id="password"
                  name="password"
                  type="password"
                  required
                  value={formData.password}
                  onChange={handleChange}
                  placeholder="••••••••"
                  className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20"
                />
                <div className="mt-2 text-right">
                  <Link href="/forgot-password" className="text-xs text-brand-accent/70 hover:text-brand-accent transition-colors hover-underline">
                    Forgot password?
                  </Link>
                </div>
              </div>
              <button
                type="submit"
                disabled={isLoading}
                className="w-full py-3 bg-brand-accent text-white font-semibold rounded-lg hover:bg-blue-500 transition-all duration-300 shadow-blue btn-glow disabled:opacity-50 disabled:cursor-not-allowed"
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
              <p className="text-xs text-white/25">
                Are you a professional?
              </p>
              <div className="flex justify-center gap-4">
                <Link href="/architect/login" className="text-xs text-golden/70 hover:text-golden transition-colors hover-underline">
                  Architect Portal
                </Link>
                <Link href="/engineer/login" className="text-xs text-slate-teal/70 hover:text-slate-teal transition-colors hover-underline">
                  Engineer Portal
                </Link>
              </div>
            </div>
          </div>
        </motion.div>
      </div>
    </div>
  );
}
