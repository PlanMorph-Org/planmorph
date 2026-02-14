'use client';

import { useState } from 'react';
import Link from 'next/link';
import api from '@/src/lib/api';
import toast, { Toaster } from 'react-hot-toast';
import { motion } from 'framer-motion';

interface ApplicationForm {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  password: string;
  confirmPassword: string;
  studentType: string;
  universityName: string;
  studentIdNumber: string;
  portfolioUrl: string;
}

export default function StudentRegisterPage() {
  const [formData, setFormData] = useState<ApplicationForm>({
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    password: '',
    confirmPassword: '',
    studentType: '',
    universityName: '',
    studentIdNumber: '',
    portfolioUrl: '',
  });
  const [isLoading, setIsLoading] = useState(false);
  const [submitted, setSubmitted] = useState(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (formData.password !== formData.confirmPassword) {
      toast.error('Passwords do not match.');
      return;
    }

    if (!formData.studentType) {
      toast.error('Please select your student type.');
      return;
    }

    setIsLoading(true);
    try {
      const payload: Record<string, string> = {
        firstName: formData.firstName,
        lastName: formData.lastName,
        email: formData.email,
        password: formData.password,
        studentType: formData.studentType,
        universityName: formData.universityName,
      };

      if (formData.phoneNumber) payload.phoneNumber = formData.phoneNumber;
      if (formData.studentIdNumber) payload.studentIdNumber = formData.studentIdNumber;
      if (formData.portfolioUrl) payload.portfolioUrl = formData.portfolioUrl;

      await api.post('/student/apply', payload);
      setSubmitted(true);
      toast.success('Application submitted successfully!', { duration: 6000 });
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to submit application. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  const fadeUp = {
    hidden: { opacity: 0, y: 20 },
    visible: (i: number) => ({
      opacity: 1,
      y: 0,
      transition: { delay: i * 0.08, duration: 0.5, ease: [0.16, 1, 0.3, 1] },
    }),
  };

  if (submitted) {
    return (
      <div className="min-h-screen bg-brand flex items-center justify-center px-4 relative overflow-hidden">
        <Toaster position="top-right" toastOptions={{ style: { background: '#1F2937', color: '#E5E7EB', border: '1px solid rgba(255,255,255,0.08)' } }} />
        <div className="absolute inset-0 blueprint-grid opacity-20" />
        <motion.div
          initial={{ opacity: 0, scale: 0.95 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ duration: 0.6, ease: [0.16, 1, 0.3, 1] }}
          className="glass-card rounded-2xl p-10 max-w-lg w-full text-center relative"
        >
          <div className="w-16 h-16 mx-auto mb-6 rounded-full bg-indigo/15 flex items-center justify-center">
            <svg className="w-8 h-8 text-indigo" fill="none" stroke="currentColor" viewBox="0 0 24 24" strokeWidth={1.5}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M9 12.75L11.25 15 15 9.75M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <h2 className="text-2xl font-display font-bold text-white mb-3">Application Submitted</h2>
          <p className="text-white/40 leading-relaxed mb-6">
            Thank you for applying to the PlanMorph Student Program. Our team will review your application within 24-48 hours. You&apos;ll receive an email once your application has been reviewed.
          </p>
          <Link
            href="/student/login"
            className="inline-block px-6 py-3 bg-indigo text-white font-semibold rounded-lg hover:bg-indigo-light transition-all duration-300"
          >
            Go to Login
          </Link>
        </motion.div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-brand py-12 px-4 relative overflow-hidden">
      <Toaster position="top-right" toastOptions={{ style: { background: '#1F2937', color: '#E5E7EB', border: '1px solid rgba(255,255,255,0.08)' } }} />
      <div className="absolute inset-0 blueprint-grid opacity-20" />
      <div className="absolute top-1/4 right-1/4 w-[600px] h-[600px] bg-indigo/[0.02] rounded-full blur-3xl" />

      <div className="max-w-3xl mx-auto relative">
        {/* Header */}
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6, ease: [0.16, 1, 0.3, 1] }}
          className="text-center mb-10"
        >
          <Link href="/" className="inline-flex items-center gap-2.5 mb-6">
            <img src="/planmorph.svg" alt="PlanMorph" className="h-9 w-auto brightness-0 invert rounded-full" />
            <span className="text-2xl font-display font-bold text-white">PlanMorph</span>
            <span className="px-2 py-0.5 text-[10px] font-semibold uppercase tracking-widest text-indigo bg-indigo/10 rounded-full border border-indigo/20">
              Student Program
            </span>
          </Link>
        </motion.div>

        {/* Program Information Section */}
        <motion.div
          custom={0}
          initial="hidden"
          animate="visible"
          variants={fadeUp}
          className="glass-card rounded-2xl p-8 mb-6"
        >
          <h2 className="text-2xl font-display font-bold text-white mb-2">Learn While You Earn</h2>
          <p className="text-white/35 text-sm mb-6">
            Join PlanMorph&apos;s supervised mentorship program and kickstart your professional career in architecture and engineering design.
          </p>

          {/* What is the Program */}
          <div className="mb-6">
            <h3 className="text-sm font-semibold text-indigo uppercase tracking-wider mb-3">What You Get</h3>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
              {[
                { icon: 'M20.25 14.15v4.25c0 1.094-.787 2.036-1.872 2.18-2.087.277-4.216.42-6.378.42s-4.291-.143-6.378-.42c-1.085-.144-1.872-1.086-1.872-2.18v-4.25m16.5 0a2.18 2.18 0 00.75-1.661V8.706c0-1.081-.768-2.015-1.837-2.175a48.114 48.114 0 00-3.413-.387m4.5 8.006c-.194.165-.42.295-.673.38A23.978 23.978 0 0112 15.75c-2.648 0-5.195-.429-7.577-1.22a2.016 2.016 0 01-.673-.38m0 0A2.18 2.18 0 013 12.489V8.706c0-1.081.768-2.015 1.837-2.175a48.111 48.111 0 013.413-.387m7.5 0V5.25A2.25 2.25 0 0013.5 3h-3a2.25 2.25 0 00-2.25 2.25v.894m7.5 0a48.667 48.667 0 00-7.5 0', text: 'Work on real client projects under licensed professional supervision' },
                { icon: 'M4.26 10.147a60.438 60.438 0 00-.491 6.347A48.62 48.62 0 0112 20.904a48.62 48.62 0 018.232-4.41 60.46 60.46 0 00-.491-6.347m-15.482 0a50.636 50.636 0 00-2.658-.813A59.906 59.906 0 0112 3.493a59.903 59.903 0 0110.399 5.84c-.896.248-1.783.52-2.658.814m-15.482 0A50.717 50.717 0 0112 13.489a50.702 50.702 0 017.74-3.342', text: 'Gain hands-on experience in architecture and engineering design' },
                { icon: 'M2.25 18.75a60.07 60.07 0 0115.797 2.101c.727.198 1.453-.342 1.453-1.096V18.75M3.75 4.5v.75A.75.75 0 013 6h-.75m0 0v-.375c0-.621.504-1.125 1.125-1.125H20.25M2.25 6v9m18-10.5v.75c0 .414.336.75.75.75h.75m-1.5-1.5h.375c.621 0 1.125.504 1.125 1.125v9.75c0 .621-.504 1.125-1.125 1.125h-.375m1.5-1.5H21a.75.75 0 00-.75.75v.75m0 0H3.75m0 0h-.375a1.125 1.125 0 01-1.125-1.125V15m1.5 1.5v-.75A.75.75 0 003 15h-.75M15 10.5a3 3 0 11-6 0 3 3 0 016 0zm3 0h.008v.008H18V10.5zm-12 0h.008v.008H6V10.5z', text: 'Earn income on every completed project' },
                { icon: 'M11.48 3.499a.562.562 0 011.04 0l2.125 5.111a.563.563 0 00.475.345l5.518.442c.499.04.701.663.321.988l-4.204 3.602a.563.563 0 00-.182.557l1.285 5.385a.562.562 0 01-.84.61l-4.725-2.885a.562.562 0 00-.586 0L6.982 20.54a.562.562 0 01-.84-.61l1.285-5.386a.562.562 0 00-.182-.557l-4.204-3.602a.562.562 0 01.321-.988l5.518-.442a.563.563 0 00.475-.345L11.48 3.5z', text: 'Build a professional portfolio with mentored work' },
              ].map((item, idx) => (
                <div key={idx} className="flex items-start gap-3 p-3 rounded-lg bg-white/[0.02]">
                  <div className="w-8 h-8 rounded-lg bg-indigo/10 flex items-center justify-center flex-shrink-0 mt-0.5">
                    <svg className="w-4 h-4 text-indigo" fill="none" stroke="currentColor" viewBox="0 0 24 24" strokeWidth={1.5}>
                      <path strokeLinecap="round" strokeLinejoin="round" d={item.icon} />
                    </svg>
                  </div>
                  <span className="text-sm text-white/50 leading-relaxed">{item.text}</span>
                </div>
              ))}
            </div>
          </div>

          {/* Eligibility */}
          <div className="mb-6">
            <h3 className="text-sm font-semibold text-indigo uppercase tracking-wider mb-3">Eligibility Requirements</h3>
            <ul className="space-y-2">
              {[
                'Must be enrolled in or graduated from an accredited Architecture or Engineering program',
                'Architecture students work under Licensed Architects, Engineering students under Licensed Engineers',
                'Valid university or institution ID, or proof of enrollment/graduation',
                'Portfolio demonstrating design or technical skills (recommended)',
              ].map((req, idx) => (
                <li key={idx} className="flex items-start gap-2.5 text-sm text-white/40">
                  <svg className="w-4 h-4 text-indigo flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24" strokeWidth={2}>
                    <path strokeLinecap="round" strokeLinejoin="round" d="M4.5 12.75l6 6 9-13.5" />
                  </svg>
                  {req}
                </li>
              ))}
            </ul>
          </div>

          {/* How It Works */}
          <div>
            <h3 className="text-sm font-semibold text-indigo uppercase tracking-wider mb-3">How It Works</h3>
            <div className="flex flex-col sm:flex-row gap-3">
              {[
                { step: '1', title: 'Apply', desc: 'Submit your application with required details' },
                { step: '2', title: 'Review', desc: 'Admin reviews your application (24-48 hours)' },
                { step: '3', title: 'Match', desc: 'Get matched with a licensed professional mentor' },
                { step: '4', title: 'Earn', desc: 'Start working on real client projects' },
              ].map((item) => (
                <div key={item.step} className="flex-1 p-3 rounded-lg bg-white/[0.02] text-center">
                  <div className="w-8 h-8 mx-auto mb-2 rounded-full bg-indigo/15 flex items-center justify-center text-sm font-bold text-indigo">
                    {item.step}
                  </div>
                  <div className="text-sm font-medium text-white/70 mb-0.5">{item.title}</div>
                  <div className="text-xs text-white/30">{item.desc}</div>
                </div>
              ))}
            </div>
          </div>
        </motion.div>

        {/* Application Form */}
        <motion.div
          custom={1}
          initial="hidden"
          animate="visible"
          variants={fadeUp}
          className="glass-card rounded-2xl p-8"
        >
          <h2 className="text-xl font-display font-bold text-white mb-1">Submit Your Application</h2>
          <p className="text-sm text-white/35 mb-6">
            Already have an account?{' '}
            <Link href="/student/login" className="text-indigo hover:text-indigo-light transition-colors">
              Sign in
            </Link>
          </p>

          <form onSubmit={handleSubmit} className="space-y-6">
            {/* Personal Information */}
            <div>
              <h3 className="text-xs font-semibold text-white/50 uppercase tracking-wider mb-4">Personal Information</h3>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label htmlFor="firstName" className="block text-xs font-medium text-white/40 mb-1.5">First Name *</label>
                  <input id="firstName" name="firstName" type="text" required value={formData.firstName} onChange={handleChange}
                    placeholder="John"
                    className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20" />
                </div>
                <div>
                  <label htmlFor="lastName" className="block text-xs font-medium text-white/40 mb-1.5">Last Name *</label>
                  <input id="lastName" name="lastName" type="text" required value={formData.lastName} onChange={handleChange}
                    placeholder="Doe"
                    className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20" />
                </div>
                <div>
                  <label htmlFor="email" className="block text-xs font-medium text-white/40 mb-1.5">Email *</label>
                  <input id="email" name="email" type="email" required value={formData.email} onChange={handleChange}
                    placeholder="you@example.com"
                    className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20" />
                </div>
                <div>
                  <label htmlFor="phoneNumber" className="block text-xs font-medium text-white/40 mb-1.5">Phone Number</label>
                  <input id="phoneNumber" name="phoneNumber" type="tel" value={formData.phoneNumber} onChange={handleChange}
                    placeholder="+254 7XX XXX XXX"
                    className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20" />
                </div>
                <div>
                  <label htmlFor="password" className="block text-xs font-medium text-white/40 mb-1.5">Password *</label>
                  <input id="password" name="password" type="password" required minLength={8} value={formData.password} onChange={handleChange}
                    placeholder="Min. 8 characters"
                    className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20" />
                </div>
                <div>
                  <label htmlFor="confirmPassword" className="block text-xs font-medium text-white/40 mb-1.5">Confirm Password *</label>
                  <input id="confirmPassword" name="confirmPassword" type="password" required minLength={8} value={formData.confirmPassword} onChange={handleChange}
                    placeholder="••••••••"
                    className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20" />
                </div>
              </div>
            </div>

            {/* Academic Information */}
            <div className="border-t border-white/6 pt-6">
              <h3 className="text-xs font-semibold text-white/50 uppercase tracking-wider mb-4">Academic Information</h3>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label htmlFor="studentType" className="block text-xs font-medium text-white/40 mb-1.5">Student Type *</label>
                  <select id="studentType" name="studentType" required value={formData.studentType} onChange={handleChange}
                    className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white bg-transparent appearance-none cursor-pointer"
                  >
                    <option value="" className="bg-brand-light text-white/50">Select type...</option>
                    <option value="Architecture" className="bg-brand-light text-white">Architecture</option>
                    <option value="Engineering" className="bg-brand-light text-white">Engineering</option>
                  </select>
                </div>
                <div>
                  <label htmlFor="universityName" className="block text-xs font-medium text-white/40 mb-1.5">University / Institution *</label>
                  <input id="universityName" name="universityName" type="text" required value={formData.universityName} onChange={handleChange}
                    placeholder="e.g. University of Nairobi"
                    className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20" />
                </div>
                <div>
                  <label htmlFor="studentIdNumber" className="block text-xs font-medium text-white/40 mb-1.5">Student ID Number</label>
                  <input id="studentIdNumber" name="studentIdNumber" type="text" value={formData.studentIdNumber} onChange={handleChange}
                    placeholder="Optional"
                    className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20" />
                </div>
                <div>
                  <label htmlFor="portfolioUrl" className="block text-xs font-medium text-white/40 mb-1.5">Portfolio URL</label>
                  <input id="portfolioUrl" name="portfolioUrl" type="url" value={formData.portfolioUrl} onChange={handleChange}
                    placeholder="https://yourportfolio.com"
                    className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20" />
                </div>
              </div>
            </div>

            {/* Submit */}
            <div className="border-t border-white/6 pt-6">
              <button type="submit" disabled={isLoading}
                className="w-full py-3 bg-indigo text-white font-semibold rounded-lg hover:bg-indigo-light transition-all duration-300 btn-glow disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isLoading ? (
                  <span className="inline-flex items-center gap-2">
                    <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" /><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" /></svg>
                    Submitting application...
                  </span>
                ) : 'Submit Application'}
              </button>
              <p className="text-xs text-white/25 text-center mt-3">
                By applying, you agree to PlanMorph&apos;s{' '}
                <Link href="/terms-of-service" className="text-indigo/60 hover:text-indigo transition-colors">Terms of Service</Link>
                {' '}and{' '}
                <Link href="/privacy-policy" className="text-indigo/60 hover:text-indigo transition-colors">Privacy Policy</Link>.
              </p>
            </div>
          </form>
        </motion.div>
      </div>
    </div>
  );
}
