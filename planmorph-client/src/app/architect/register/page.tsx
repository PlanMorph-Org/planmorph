'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import api from '@/src/lib/api';
import toast, { Toaster } from 'react-hot-toast';
import { motion, AnimatePresence } from 'framer-motion';

export default function ArchitectRegisterPage() {
  const router = useRouter();
  const [formData, setFormData] = useState({
    email: '', password: '', confirmPassword: '', firstName: '', lastName: '',
    phoneNumber: '', professionalLicense: '', yearsOfExperience: '', portfolio: '', specialization: '',
  });
  const [cvFile, setCvFile] = useState<File | null>(null);
  const [coverLetterFile, setCoverLetterFile] = useState<File | null>(null);
  const [workExperienceFile, setWorkExperienceFile] = useState<File | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (formData.password !== formData.confirmPassword) { toast.error('Passwords do not match'); return; }
    if (!formData.professionalLicense || !formData.yearsOfExperience) { toast.error('Please fill in all required fields'); return; }
    const hasPortfolio = formData.portfolio.trim().length > 0;
    const isPdf = (file: File | null) => !!file && (file.type === 'application/pdf' || file.name.toLowerCase().endsWith('.pdf'));
    if (!hasPortfolio) {
      if (!cvFile || !coverLetterFile || !workExperienceFile) { toast.error('Upload CV, cover letter, and work experience PDF.'); return; }
      if (!isPdf(cvFile) || !isPdf(coverLetterFile) || !isPdf(workExperienceFile)) { toast.error('All documents must be PDF files.'); return; }
    }
    setIsLoading(true);
    try {
      const years = formData.yearsOfExperience ? parseInt(formData.yearsOfExperience, 10) : undefined;
      const payload = new FormData();
      payload.append('email', formData.email); payload.append('password', formData.password);
      payload.append('firstName', formData.firstName); payload.append('lastName', formData.lastName);
      payload.append('phoneNumber', formData.phoneNumber); payload.append('role', 'Architect');
      payload.append('professionalLicense', formData.professionalLicense);
      if (!Number.isNaN(years) && years !== undefined) payload.append('yearsOfExperience', String(years));
      if (formData.portfolio.trim()) payload.append('portfolioUrl', formData.portfolio.trim());
      if (formData.specialization.trim()) payload.append('specialization', formData.specialization.trim());
      if (!hasPortfolio) { if (cvFile) payload.append('cvFile', cvFile); if (coverLetterFile) payload.append('coverLetterFile', coverLetterFile); if (workExperienceFile) payload.append('workExperienceFile', workExperienceFile); }
      await api.post('/auth/register-professional', payload);
      toast.success('Application submitted. Review in 24–48 hours.', { duration: 6000 });
      setTimeout(() => router.push('/architect/login'), 2000);
    } catch (error: any) { toast.error(error.response?.data?.message || 'Registration failed.'); }
    finally { setIsLoading(false); }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => setFormData({ ...formData, [e.target.name]: e.target.value });
  const handleFileChange = (setter: (file: File | null) => void) => (e: React.ChangeEvent<HTMLInputElement>) => { setter(e.target.files?.[0] ?? null); };

  const inputClass = 'w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20 focus:outline-none';
  const labelClass = 'block text-xs font-medium text-white/40 mb-1.5';

  return (
    <div className="min-h-screen bg-brand flex items-center justify-center py-12 px-4">
      <Toaster position="top-right" toastOptions={{ style: { background: '#1F2937', color: '#E5E7EB', border: '1px solid rgba(255,255,255,0.08)' } }} />
      <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.5 }}
        className="max-w-2xl w-full glass-card rounded-2xl p-8 border border-white/10">
        <div className="text-center mb-8">
          <Link href="/" className="inline-flex items-center gap-2 text-2xl font-display font-bold text-white">
            <img src="/planmorph.svg" alt="PlanMorph" className="h-8 w-auto rounded-full" />
            <span>PlanMorph</span>
          </Link>
          <div className="mt-1"><span className="px-2 py-0.5 text-[10px] font-semibold uppercase tracking-widest text-golden bg-golden/10 rounded-full border border-golden/20">Architect Portal</span></div>
          <h2 className="mt-5 text-xl font-semibold text-white">Apply as an Architect</h2>
          <p className="mt-1 text-sm text-white/30">
            Already have an account?{' '}
            <Link href="/architect/login" className="text-golden hover:text-golden-light transition-colors">Sign in</Link>
          </p>
        </div>

        <form className="space-y-5" onSubmit={handleSubmit}>
          <div className="grid grid-cols-2 gap-4">
            <div><label className={labelClass}>First Name *</label><input name="firstName" type="text" required value={formData.firstName} onChange={handleChange} className={inputClass} /></div>
            <div><label className={labelClass}>Last Name *</label><input name="lastName" type="text" required value={formData.lastName} onChange={handleChange} className={inputClass} /></div>
          </div>
          <div><label className={labelClass}>Email *</label><input name="email" type="email" required value={formData.email} onChange={handleChange} className={inputClass} /></div>
          <div><label className={labelClass}>Phone Number *</label><input name="phoneNumber" type="tel" required value={formData.phoneNumber} onChange={handleChange} className={inputClass} placeholder="+[country code] [number]" /></div>

          <div className="border-t border-white/6 pt-5">
            <h3 className="text-sm font-semibold text-golden mb-4">Professional Information</h3>
            <div className="space-y-4">
              <div><label className={labelClass}>Professional License Number *</label><input name="professionalLicense" type="text" required value={formData.professionalLicense} onChange={handleChange} className={inputClass} placeholder="Professional license number" /></div>
              <div><label className={labelClass}>Years of Experience *</label><input name="yearsOfExperience" type="number" required value={formData.yearsOfExperience} onChange={handleChange} className={inputClass} /></div>
              <div><label className={labelClass}>Specialization (Optional)</label><input name="specialization" type="text" value={formData.specialization} onChange={handleChange} className={inputClass} placeholder="e.g., Residential, Commercial, Green Buildings" /></div>
              <div><label className={labelClass}>Portfolio/Website URL (Optional)</label><input name="portfolio" type="url" value={formData.portfolio} onChange={handleChange} className={inputClass} placeholder="https://yourportfolio.com" /></div>

              <AnimatePresence>
                {!formData.portfolio.trim() && (
                  <motion.div initial={{ height: 0, opacity: 0 }} animate={{ height: 'auto', opacity: 1 }} exit={{ height: 0, opacity: 0 }} className="overflow-hidden">
                    <div className="glass-card-light rounded-lg p-4 border border-golden/10">
                      <p className="text-xs text-golden/60 mb-3">No portfolio? Upload your CV, cover letter, and work experience.</p>
                      <div className="space-y-3">
                        {[
                          { label: 'CV (PDF) *', setter: setCvFile, file: cvFile },
                          { label: 'Cover Letter (PDF) *', setter: setCoverLetterFile, file: coverLetterFile },
                          { label: 'Work Experience (PDF) *', setter: setWorkExperienceFile, file: workExperienceFile },
                        ].map(({ label, setter, file }) => (
                          <div key={label}>
                            <label className="block text-xs text-white/30 mb-1">{label}</label>
                            <label className="flex items-center gap-2 cursor-pointer glass-input rounded-lg px-4 py-2.5 text-xs text-white/30 hover:border-golden/30 transition-colors">
                              <svg className="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" /></svg>
                              {file ? <span className="text-white/60">{file.name}</span> : 'Choose PDF file'}
                              <input type="file" accept=".pdf,application/pdf" required onChange={handleFileChange(setter)} className="hidden" />
                            </label>
                          </div>
                        ))}
                      </div>
                    </div>
                  </motion.div>
                )}
              </AnimatePresence>
            </div>
          </div>

          <div className="border-t border-white/6 pt-5">
            <div className="grid grid-cols-2 gap-4">
              <div><label className={labelClass}>Password *</label><input name="password" type="password" required value={formData.password} onChange={handleChange} className={inputClass} placeholder="Minimum 8 characters" /></div>
              <div><label className={labelClass}>Confirm Password *</label><input name="confirmPassword" type="password" required value={formData.confirmPassword} onChange={handleChange} className={inputClass} /></div>
            </div>
          </div>

          <div className="glass-card-light rounded-lg p-4 border border-golden/10">
            <p className="text-xs text-golden/60"><strong className="text-golden">Review window:</strong> 24–48 hours. You&apos;ll receive an email once your account is approved.</p>
          </div>

          <button type="submit" disabled={isLoading}
            className="w-full py-3 bg-golden text-brand font-semibold rounded-lg hover:bg-golden-light transition-all duration-300 text-sm disabled:opacity-40 disabled:cursor-not-allowed">
            {isLoading ? <span className="flex items-center justify-center gap-2"><div className="w-4 h-4 border-2 border-brand/30 border-t-brand rounded-full animate-spin" />Submitting...</span> : 'Submit Application'}
          </button>
        </form>
      </motion.div>
    </div>
  );
}
