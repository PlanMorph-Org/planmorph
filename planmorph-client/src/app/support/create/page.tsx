'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Layout from '@/src/components/Layout';
import api from '@/src/lib/api';
import { CreateTicketDto, TicketCategory, TicketPriority } from '@/src/types';
import toast, { Toaster } from 'react-hot-toast';
import { useRoleGuard } from '@/src/hooks/useRoleGuard';
import { motion } from 'framer-motion';

const categories: { value: TicketCategory; label: string }[] = [
  { value: 'General', label: 'General' },
  { value: 'Technical', label: 'Technical' },
  { value: 'Billing', label: 'Billing' },
  { value: 'Design', label: 'Design' },
  { value: 'Order', label: 'Order' },
  { value: 'Construction', label: 'Construction' },
];

const priorities: { value: TicketPriority; label: string; desc: string }[] = [
  { value: 'Low', label: 'Low', desc: 'General inquiry or feedback' },
  { value: 'Medium', label: 'Medium', desc: 'Issue that needs attention' },
  { value: 'High', label: 'High', desc: 'Significant impact on usage' },
  { value: 'Urgent', label: 'Urgent', desc: 'Critical issue requiring immediate action' },
];

export default function CreateTicketPage() {
  const router = useRouter();
  const { isAuthorized, isChecking } = useRoleGuard({
    requiredRole: 'Client',
    redirectTo: '/login',
  });

  const [formData, setFormData] = useState<CreateTicketDto>({
    subject: '',
    description: '',
    priority: 'Medium',
    category: 'General',
  });
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (formData.subject.length < 5) {
      toast.error('Subject must be at least 5 characters long');
      return;
    }

    if (formData.description.length < 10) {
      toast.error('Description must be at least 10 characters long');
      return;
    }

    setIsSubmitting(true);
    try {
      await api.post('/ticket', formData);
      toast.success('Support ticket created successfully!');
      router.push('/support');
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to create ticket');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isChecking) {
    return (
      <Layout>
        <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
          <div className="h-8 w-40 shimmer-bg rounded mb-6" />
          <div className="glass-card rounded-xl p-6 space-y-4">
            <div className="h-4 w-1/3 shimmer-bg rounded" />
            <div className="h-20 shimmer-bg rounded" />
          </div>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <Toaster
        position="top-right"
        toastOptions={{
          style: {
            background: '#1F2937',
            color: '#E5E7EB',
            border: '1px solid rgba(255,255,255,0.08)',
          },
        }}
      />

      {/* Header */}
      <section className="relative py-14 border-b border-white/6">
        <div className="absolute inset-0 bg-hero-gradient" />
        <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <button
            onClick={() => router.back()}
            className="text-xs text-white/40 hover:text-white transition-colors mb-3"
          >
            &larr; Back to Tickets
          </button>
          <motion.h1
            initial={{ opacity: 0, y: 16 }}
            animate={{ opacity: 1, y: 0 }}
            className="text-2xl md:text-3xl font-display font-bold text-white tracking-tight mb-1"
          >
            Create Support Ticket
          </motion.h1>
          <motion.p
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.1 }}
            className="text-xs text-white/30"
          >
            Describe your issue and we&apos;ll get back to you within 24 hours
          </motion.p>
        </div>
      </section>

      <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <motion.form
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.15 }}
          onSubmit={handleSubmit}
          className="glass-card rounded-xl p-6 space-y-6"
        >
          {/* Subject */}
          <div>
            <label className="block text-sm font-medium text-white/70 mb-2">
              Subject <span className="text-red-400">*</span>
            </label>
            <input
              type="text"
              value={formData.subject}
              onChange={(e) => setFormData({ ...formData, subject: e.target.value })}
              placeholder="Brief description of your issue"
              className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20"
              required
            />
            <p className="text-[10px] text-white/20 mt-1">Minimum 5 characters</p>
          </div>

          {/* Category & Priority */}
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-white/70 mb-2">
                Category
              </label>
              <select
                value={formData.category}
                onChange={(e) => setFormData({ ...formData, category: e.target.value as TicketCategory })}
                className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white"
              >
                {categories.map((cat) => (
                  <option key={cat.value} value={cat.value}>
                    {cat.label}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-white/70 mb-2">
                Priority
              </label>
              <select
                value={formData.priority}
                onChange={(e) => setFormData({ ...formData, priority: e.target.value as TicketPriority })}
                className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white"
              >
                {priorities.map((p) => (
                  <option key={p.value} value={p.value}>
                    {p.label} - {p.desc}
                  </option>
                ))}
              </select>
            </div>
          </div>

          {/* Description */}
          <div>
            <label className="block text-sm font-medium text-white/70 mb-2">
              Description <span className="text-red-400">*</span>
            </label>
            <textarea
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              placeholder="Please provide as much detail as possible about your issue..."
              rows={8}
              className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20 resize-none"
              required
            />
            <p className="text-[10px] text-white/20 mt-1">
              Minimum 10 characters ({formData.description.length}/2000)
            </p>
          </div>

          {/* Info Banner */}
          <div className="glass-card-light rounded-lg p-4 border border-brand-accent/10">
            <p className="text-xs text-white/40">
              <strong className="text-white/60">What happens next?</strong> Our support team will review
              your ticket and respond within 24 hours. You&apos;ll receive email notifications when we update
              your ticket or respond with a message.
            </p>
          </div>

          {/* Actions */}
          <div className="flex gap-3 pt-2">
            <button
              type="button"
              onClick={() => router.back()}
              disabled={isSubmitting}
              className="flex-1 py-3 border border-white/10 rounded-lg text-sm text-white/50 hover:text-white hover:bg-white/5 transition-all disabled:opacity-50"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="flex-1 py-3 bg-brand-accent text-white font-medium rounded-lg hover:bg-blue-500 transition-all btn-glow disabled:opacity-50"
            >
              {isSubmitting ? 'Creating...' : 'Create Ticket'}
            </button>
          </div>
        </motion.form>
      </div>
    </Layout>
  );
}
