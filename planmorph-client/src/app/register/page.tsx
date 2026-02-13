'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import Layout from '@/src/components/Layout';
import { useAuthStore } from '@/src/store/authStore';
import api from '@/src/lib/api';
import { AuthResponse } from '@/src/types';
import toast, { Toaster } from 'react-hot-toast';
import { motion, AnimatePresence } from 'framer-motion';

const roleCards = [
  {
    role: 'Client' as const,
    title: 'Client',
    desc: 'Browse and purchase verified, build-ready designs',
    gradient: 'from-brand-accent/20 to-blue-500/10',
    border: 'border-brand-accent/40',
    iconBg: 'bg-brand-accent/20',
    icon: (
      <svg className="w-6 h-6 text-brand-accent" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M2.25 12l8.954-8.955c.44-.439 1.152-.439 1.591 0L21.75 12M4.5 9.75v10.125c0 .621.504 1.125 1.125 1.125H9.75v-4.875c0-.621.504-1.125 1.125-1.125h2.25c.621 0 1.125.504 1.125 1.125V21h4.125c.621 0 1.125-.504 1.125-1.125V9.75M8.25 21h8.25" /></svg>
    ),
  },
  {
    role: 'Architect' as const,
    title: 'Architect',
    desc: 'List your designs on a verified professional platform',
    gradient: 'from-golden/20 to-amber-500/10',
    border: 'border-golden/40',
    iconBg: 'bg-golden/20',
    icon: (
      <svg className="w-6 h-6 text-golden" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9.53 16.122a3 3 0 00-5.78 1.128 2.25 2.25 0 01-2.4 2.245 4.5 4.5 0 008.4-2.245c0-.399-.078-.78-.22-1.128zm0 0a15.998 15.998 0 003.388-1.62m-5.043-.025a15.994 15.994 0 011.622-3.395m3.42 3.42a15.995 15.995 0 004.764-4.648l3.876-5.814a1.151 1.151 0 00-1.597-1.597L14.146 6.32a15.996 15.996 0 00-4.649 4.763m3.42 3.42a6.776 6.776 0 00-3.42-3.42" /></svg>
    ),
  },
  {
    role: 'Engineer' as const,
    title: 'Engineer',
    desc: 'Review and verify structural integrity and BOQs',
    gradient: 'from-slate-teal/20 to-emerald-500/10',
    border: 'border-slate-teal/40',
    iconBg: 'bg-slate-teal/20',
    icon: (
      <svg className="w-6 h-6 text-slate-teal" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M11.42 15.17l-5.657-5.657A2.625 2.625 0 119.513 5.763l5.657 5.657m-1.414 1.414L15.17 14.24M21 12a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
    ),
  },
];

export default function RegisterPage() {
  const router = useRouter();
  const { register } = useAuthStore();
  const [accountType, setAccountType] = useState<'Client' | 'Architect' | 'Engineer'>('Client');
  const [formData, setFormData] = useState({
    email: '', password: '', confirmPassword: '', firstName: '', lastName: '', phoneNumber: '',
    professionalLicense: '', yearsOfExperience: '', portfolio: '', specialization: '',
  });
  const [cvFile, setCvFile] = useState<File | null>(null);
  const [coverLetterFile, setCoverLetterFile] = useState<File | null>(null);
  const [workExperienceFile, setWorkExperienceFile] = useState<File | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [acceptedTerms, setAcceptedTerms] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!acceptedTerms) { toast.error('You must agree to the Terms of Service and Privacy Policy'); return; }
    if (formData.password !== formData.confirmPassword) { toast.error('Passwords do not match'); return; }

    if (accountType !== 'Client') {
      if (!formData.professionalLicense || !formData.yearsOfExperience) { toast.error('Please fill in all professional fields'); return; }
      const hasPortfolio = formData.portfolio.trim().length > 0;
      const isPdf = (file: File | null) => !!file && (file.type === 'application/pdf' || file.name.toLowerCase().endsWith('.pdf'));
      if (!hasPortfolio) {
        if (!cvFile || !coverLetterFile || !workExperienceFile) { toast.error('Upload your CV, cover letter, and work experience PDF if you do not have a portfolio.'); return; }
        if (!isPdf(cvFile) || !isPdf(coverLetterFile) || !isPdf(workExperienceFile)) { toast.error('CV, cover letter, and work experience must be PDF files.'); return; }
      }
    }

    setIsLoading(true);
    try {
      const years = formData.yearsOfExperience ? parseInt(formData.yearsOfExperience, 10) : undefined;
      if (accountType !== 'Client') {
        const payload = new FormData();
        const hasPortfolio = formData.portfolio.trim().length > 0;
        payload.append('email', formData.email);
        payload.append('password', formData.password);
        payload.append('firstName', formData.firstName);
        payload.append('lastName', formData.lastName);
        payload.append('phoneNumber', formData.phoneNumber);
        payload.append('role', accountType);
        payload.append('professionalLicense', formData.professionalLicense);
        if (!Number.isNaN(years) && years !== undefined) payload.append('yearsOfExperience', String(years));
        if (hasPortfolio) payload.append('portfolioUrl', formData.portfolio.trim());
        if (formData.specialization.trim().length > 0) payload.append('specialization', formData.specialization.trim());
        if (!hasPortfolio) {
          if (cvFile) payload.append('cvFile', cvFile);
          if (coverLetterFile) payload.append('coverLetterFile', coverLetterFile);
          if (workExperienceFile) payload.append('workExperienceFile', workExperienceFile);
        }
        await api.post<AuthResponse>('/auth/register-professional', payload);
        const targetLogin = accountType === 'Engineer' ? '/engineer/login' : '/architect/login';
        toast.success(`${accountType} application submitted! Admin will review your account.`, { duration: 5000 });
        router.push(targetLogin);
        return;
      }
      await register({ email: formData.email, password: formData.password, firstName: formData.firstName, lastName: formData.lastName, phoneNumber: formData.phoneNumber, role: undefined });
      toast.success('Registration successful!');
      router.push('/designs');
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Registration failed. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleFileChange = (setter: (file: File | null) => void) =>
    (e: React.ChangeEvent<HTMLInputElement>) => { setter(e.target.files?.[0] ?? null); };

  const inputClass = 'w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20';
  const labelClass = 'block text-xs font-medium text-white/40 mb-1.5';

  return (
    <Layout>
      <Toaster position="top-right" toastOptions={{ style: { background: '#1F2937', color: '#E5E7EB', border: '1px solid rgba(255,255,255,0.08)' } }} />

      <section className="relative py-16 border-b border-white/6">
        <div className="absolute inset-0 bg-hero-gradient" />
        <div className="absolute inset-0 blueprint-grid opacity-20" />
        <div className="relative max-w-2xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
          <motion.h1
            initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }}
            className="text-3xl md:text-4xl font-display font-bold text-white tracking-tight mb-3"
          >
            Create your account
          </motion.h1>
          <motion.p
            initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.1 }}
            className="text-white/40 text-sm"
          >
            Already have an account?{' '}
            <Link href="/login" className="text-brand-accent hover:text-blue-400 transition-colors">Sign in</Link>
          </motion.p>
        </div>
      </section>

      <div className="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
        {/* Role Selection Cards */}
        <motion.div
          initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.15 }}
          className="mb-8"
        >
          <p className="text-xs font-medium text-white/30 mb-3 uppercase tracking-widest">I want to register as</p>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
            {roleCards.map((card) => (
              <button
                key={card.role}
                type="button"
                onClick={() => setAccountType(card.role)}
                className={`relative glass-card-light rounded-xl p-4 text-left transition-all duration-300 ${accountType === card.role
                    ? `${card.border} border-2 shadow-glass`
                    : 'border border-white/6 hover:border-white/12'
                  }`}
              >
                <div className={`w-9 h-9 rounded-lg ${card.iconBg} flex items-center justify-center mb-3`}>
                  {card.icon}
                </div>
                <h3 className="text-sm font-semibold text-white mb-0.5">{card.title}</h3>
                <p className="text-[11px] text-white/35 leading-relaxed">{card.desc}</p>
                {accountType === card.role && (
                  <motion.div layoutId="role-check" className="absolute top-3 right-3 w-5 h-5 rounded-full bg-brand-accent flex items-center justify-center">
                    <svg className="w-3 h-3 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M5 13l4 4L19 7" /></svg>
                  </motion.div>
                )}
              </button>
            ))}
          </div>
        </motion.div>

        {/* Form */}
        <motion.form
          initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.25 }}
          onSubmit={handleSubmit}
          className="glass-card rounded-2xl p-8 space-y-6"
        >
          {/* Name fields */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label htmlFor="firstName" className={labelClass}>First Name *</label>
              <input id="firstName" name="firstName" type="text" required value={formData.firstName} onChange={handleChange} placeholder="First Name" className={inputClass} />
            </div>
            <div>
              <label htmlFor="lastName" className={labelClass}>Last Name *</label>
              <input id="lastName" name="lastName" type="text" required value={formData.lastName} onChange={handleChange} placeholder="Last Name" className={inputClass} />
            </div>
          </div>

          <div>
            <label htmlFor="email" className={labelClass}>Email *</label>
            <input id="email" name="email" type="email" required value={formData.email} onChange={handleChange} placeholder="you@example.com" className={inputClass} />
          </div>

          <div>
            <label htmlFor="phoneNumber" className={labelClass}>Phone Number *</label>
            <input id="phoneNumber" name="phoneNumber" type="tel" required value={formData.phoneNumber} onChange={handleChange} placeholder="+254 7XX XXX XXX" className={inputClass} />
          </div>

          {/* Professional fields */}
          <AnimatePresence>
            {accountType !== 'Client' && (
              <motion.div
                initial={{ height: 0, opacity: 0 }}
                animate={{ height: 'auto', opacity: 1 }}
                exit={{ height: 0, opacity: 0 }}
                transition={{ duration: 0.3, ease: [0.16, 1, 0.3, 1] }}
                className="overflow-hidden"
              >
                <div className="pt-4 border-t border-white/6 space-y-5">
                  <h3 className="text-xs font-semibold uppercase tracking-widest text-golden/70">Professional Information</h3>

                  <div>
                    <label htmlFor="professionalLicense" className={labelClass}>License Number *</label>
                    <input id="professionalLicense" name="professionalLicense" type="text" required value={formData.professionalLicense} onChange={handleChange} placeholder="Professional License Number" className={inputClass} />
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label htmlFor="yearsOfExperience" className={labelClass}>Years of Experience *</label>
                      <input id="yearsOfExperience" name="yearsOfExperience" type="number" required value={formData.yearsOfExperience} onChange={handleChange} placeholder="e.g., 5" className={inputClass} />
                    </div>
                    <div>
                      <label htmlFor="specialization" className={labelClass}>Specialization</label>
                      <input id="specialization" name="specialization" type="text" value={formData.specialization} onChange={handleChange} placeholder="e.g., Residential" className={inputClass} />
                    </div>
                  </div>

                  <div>
                    <label htmlFor="portfolio" className={labelClass}>Portfolio URL</label>
                    <input id="portfolio" name="portfolio" type="url" value={formData.portfolio} onChange={handleChange} placeholder="https://yourportfolio.com" className={inputClass} />
                  </div>

                  {!formData.portfolio.trim() && (
                    <div className="glass-card-light rounded-xl p-5 space-y-4">
                      <p className="text-xs text-brand-accent">No portfolio? Upload these documents instead:</p>
                      {[
                        { id: 'cvFile', label: 'CV (PDF) *', setter: setCvFile },
                        { id: 'coverLetterFile', label: 'Cover Letter (PDF) *', setter: setCoverLetterFile },
                        { id: 'workExperienceFile', label: 'Work Experience (PDF) *', setter: setWorkExperienceFile },
                      ].map((f) => (
                        <div key={f.id}>
                          <label htmlFor={f.id} className={labelClass}>{f.label}</label>
                          <input id={f.id} name={f.id} type="file" accept=".pdf,application/pdf" required={!formData.portfolio.trim()} onChange={handleFileChange(f.setter)}
                            className="block w-full text-sm text-white/50 file:mr-3 file:py-2 file:px-4 file:rounded-lg file:border-0 file:text-xs file:font-medium file:bg-white/10 file:text-white/70 hover:file:bg-white/15 file:cursor-pointer file:transition-colors" />
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </motion.div>
            )}
          </AnimatePresence>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label htmlFor="password" className={labelClass}>Password *</label>
              <input id="password" name="password" type="password" required value={formData.password} onChange={handleChange} placeholder="Min. 8 characters" className={inputClass} />
            </div>
            <div>
              <label htmlFor="confirmPassword" className={labelClass}>Confirm Password *</label>
              <input id="confirmPassword" name="confirmPassword" type="password" required value={formData.confirmPassword} onChange={handleChange} placeholder="Confirm" className={inputClass} />
            </div>
          </div>

          {accountType !== 'Client' && (
            <div className="glass-card-light rounded-lg p-4">
              <p className="text-xs text-brand-accent">
                <strong>Note:</strong> Professional accounts are reviewed by our team. You&apos;ll receive an email once approved.
              </p>
            </div>
          )}

          {/* Terms */}
          <div className="flex items-start gap-3">
            <input id="acceptedTerms" name="acceptedTerms" type="checkbox" checked={acceptedTerms} onChange={(e) => setAcceptedTerms(e.target.checked)} required
              className="mt-0.5 h-4 w-4 rounded bg-white/5 border-white/20 text-brand-accent focus:ring-brand-accent/50" />
            <label htmlFor="acceptedTerms" className="text-xs text-white/40">
              I agree to the{' '}
              <Link href="/terms-of-service" target="_blank" className="text-brand-accent hover:text-blue-400 transition-colors">Terms of Service</Link>
              {' '}and{' '}
              <Link href="/privacy-policy" target="_blank" className="text-brand-accent hover:text-blue-400 transition-colors">Privacy Policy</Link> *
            </label>
          </div>

          <button type="submit" disabled={isLoading}
            className="w-full py-3 bg-brand-accent text-white font-semibold rounded-lg hover:bg-blue-500 transition-all duration-300 shadow-blue btn-glow disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isLoading ? (
              <span className="inline-flex items-center gap-2">
                <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" /><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" /></svg>
                {accountType !== 'Client' ? 'Submitting application...' : 'Creating account...'}
              </span>
            ) : accountType !== 'Client' ? 'Submit Application' : 'Create Account'}
          </button>
        </motion.form>
      </div>
    </Layout>
  );
}
