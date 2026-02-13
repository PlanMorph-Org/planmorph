'use client';

import { useEffect, useMemo, useState } from 'react';
import toast, { Toaster } from 'react-hot-toast';
import api from '@/src/lib/api';
import EngineerLayout from '@/src/components/EngineerLayout';
import { useRoleGuard } from '@/src/hooks/useRoleGuard';
import { motion, AnimatePresence } from 'framer-motion';

interface Verification { id: string; designId: string; designTitle: string; architectName: string; verificationType: string; status: string; comments?: string; createdAt: string; }
interface VerificationFile { id: string; fileName: string; category: string; storageUrl: string; fileSizeBytes: number; }
type FilterKey = 'All' | 'Structural' | 'BOQ';

const fadeUp = {
  hidden: { opacity: 0, y: 20 },
  visible: (i: number = 0) => ({ opacity: 1, y: 0, transition: { delay: i * 0.08, duration: 0.5, ease: [0.16, 1, 0.3, 1] } }) as const,
};

export default function EngineerDashboard() {
  const { isAuthorized } = useRoleGuard({ requiredRole: 'Engineer', redirectTo: '/engineer/login', message: 'Access denied. Engineer account required.' });
  const [isLoading, setIsLoading] = useState(true);
  const [verifications, setVerifications] = useState<Verification[]>([]);
  const [filter, setFilter] = useState<FilterKey>('All');
  const [showModal, setShowModal] = useState(false);
  const [selected, setSelected] = useState<Verification | null>(null);
  const [files, setFiles] = useState<VerificationFile[]>([]);
  const [isFilesLoading, setIsFilesLoading] = useState(false);
  const [comments, setComments] = useState('');
  const [isActionLoading, setIsActionLoading] = useState(false);

  useEffect(() => { if (isAuthorized) loadVerifications(); }, [isAuthorized]);

  const loadVerifications = async () => { setIsLoading(true); try { const r = await api.get<Verification[]>('/design-verifications/pending'); setVerifications(r.data || []); } catch (e: any) { toast.error(e.response?.data?.message || 'Failed to load verifications.'); } finally { setIsLoading(false); } };

  const filteredVerifications = useMemo(() => {
    if (filter === 'All') return verifications;
    if (filter === 'Structural') return verifications.filter(v => v.verificationType === 'Structural');
    return verifications.filter(v => v.verificationType === 'BOQ' || v.verificationType === 'BOQEngineer');
  }, [verifications, filter]);

  const totalPending = verifications.length;
  const structuralPending = verifications.filter(v => v.verificationType === 'Structural').length;
  const boqPending = verifications.filter(v => v.verificationType === 'BOQ' || v.verificationType === 'BOQEngineer').length;

  const openReview = async (verification: Verification) => {
    setSelected(verification); setComments(''); setFiles([]); setShowModal(true); setIsFilesLoading(true);
    try { const r = await api.get<VerificationFile[]>(`/design-verifications/${verification.id}/files`); setFiles(r.data || []); }
    catch (e: any) { toast.error(e.response?.data?.message || 'Failed to load files.'); }
    finally { setIsFilesLoading(false); }
  };

  const closeReview = () => { setShowModal(false); setSelected(null); setFiles([]); setComments(''); setIsActionLoading(false); };

  const handleVerify = async () => {
    if (!selected) return; setIsActionLoading(true);
    try { await api.put(`/design-verifications/${selected.id}/verify`, { comments }); toast.success('Verification approved.'); await loadVerifications(); closeReview(); }
    catch (e: any) { toast.error(e.response?.data?.message || 'Failed to approve.'); }
    finally { setIsActionLoading(false); }
  };

  const handleReject = async () => {
    if (!selected) return; setIsActionLoading(true);
    try { await api.put(`/design-verifications/${selected.id}/reject`, { comments }); toast.success('Verification rejected.'); await loadVerifications(); closeReview(); }
    catch (e: any) { toast.error(e.response?.data?.message || 'Failed to reject.'); }
    finally { setIsActionLoading(false); }
  };

  const formatFileSize = (bytes: number) => { if (bytes < 1024) return `${bytes} B`; if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`; return `${(bytes / (1024 * 1024)).toFixed(1)} MB`; };

  const getTypeLabel = (t: string) => { switch (t) { case 'Structural': return 'Structural'; case 'BOQEngineer': return 'BOQ (Eng)'; case 'BOQ': return 'BOQ'; default: return t; } };
  const getTypeBadge = (t: string) => { switch (t) { case 'Structural': return 'bg-brand-accent/15 text-brand-accent border-brand-accent/20'; case 'BOQEngineer': case 'BOQ': return 'bg-golden/15 text-golden border-golden/20'; default: return 'bg-white/10 text-white/50 border-white/10'; } };
  const getStatusBadge = (s: string) => { switch (s) { case 'Verified': return 'bg-verified/15 text-verified border-verified/20'; case 'Pending': return 'bg-golden/15 text-golden border-golden/20'; case 'Rejected': return 'bg-red-500/15 text-red-400 border-red-500/20'; default: return 'bg-white/10 text-white/50 border-white/10'; } };

  const statCards = [
    { label: 'Total Pending', value: totalPending, gradient: 'from-slate-teal/20 to-teal-500/10', color: 'text-slate-teal' },
    { label: 'Structural', value: structuralPending, gradient: 'from-brand-accent/20 to-blue-500/10', color: 'text-brand-accent' },
    { label: 'BOQ', value: boqPending, gradient: 'from-golden/20 to-amber-500/10', color: 'text-golden' },
  ];

  return (
    <EngineerLayout>
      <Toaster position="top-right" toastOptions={{ style: { background: '#1F2937', color: '#E5E7EB', border: '1px solid rgba(255,255,255,0.08)' } }} />
      <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <motion.div initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }} className="mb-8">
          <h1 className="text-2xl md:text-3xl font-display font-bold text-white">Verification Dashboard</h1>
          <p className="text-sm text-white/40 mt-1">Review structural drawings and BOQ submissions.</p>
        </motion.div>

        {/* Stats */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
          {statCards.map((card, i) => (
            <motion.div key={card.label} variants={fadeUp} initial="hidden" animate="visible" custom={i} className="glass-card rounded-xl p-5 card-hover">
              <p className="text-[10px] text-white/30 uppercase tracking-widest mb-1">{card.label}</p>
              <p className={`text-2xl font-bold font-mono ${card.color}`}>{card.value}</p>
            </motion.div>
          ))}
        </div>

        {/* Filters */}
        <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} transition={{ delay: 0.2 }} className="flex flex-wrap gap-2 mb-6">
          {(['All', 'Structural', 'BOQ'] as FilterKey[]).map((key) => (
            <button key={key} onClick={() => setFilter(key)}
              className={`px-4 py-1.5 rounded-full text-xs font-medium transition-all duration-300 ${filter === key ? 'bg-slate-teal text-white' : 'text-white/40 hover:text-white bg-white/5 hover:bg-white/10'}`}
            >
              {key}
            </button>
          ))}
        </motion.div>

        {/* Table */}
        {isLoading ? (
          <div className="space-y-3">{[...Array(5)].map((_, i) => <div key={i} className="glass-card rounded-xl p-5 h-16 shimmer-bg" />)}</div>
        ) : filteredVerifications.length === 0 ? (
          <div className="glass-card rounded-xl p-10 text-center">
            <p className="text-white/40">No pending verifications.</p>
          </div>
        ) : (
          <motion.div initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.3 }} className="glass-card rounded-xl overflow-hidden">
            <div className="overflow-x-auto">
              <table className="min-w-full text-sm">
                <thead>
                  <tr className="text-white/30 text-[10px] uppercase tracking-widest">
                    <th className="px-5 py-3 text-left font-medium">Design</th>
                    <th className="px-5 py-3 text-left font-medium">Architect</th>
                    <th className="px-5 py-3 text-left font-medium">Type</th>
                    <th className="px-5 py-3 text-left font-medium">Status</th>
                    <th className="px-5 py-3 text-left font-medium">Submitted</th>
                    <th className="px-5 py-3 text-left font-medium">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-white/6">
                  {filteredVerifications.map((v) => (
                    <tr key={v.id} className="hover:bg-white/[0.02] transition-colors">
                      <td className="px-5 py-4 text-sm font-medium text-white">{v.designTitle}</td>
                      <td className="px-5 py-4 text-white/40">{v.architectName}</td>
                      <td className="px-5 py-4">
                        <span className={`px-2.5 py-0.5 text-[10px] font-semibold rounded-full border ${getTypeBadge(v.verificationType)}`}>
                          {getTypeLabel(v.verificationType)}
                        </span>
                      </td>
                      <td className="px-5 py-4">
                        <span className={`px-2.5 py-0.5 text-[10px] font-semibold rounded-full border ${getStatusBadge(v.status)}`}>
                          {v.status}
                        </span>
                      </td>
                      <td className="px-5 py-4 text-white/30 text-xs">{new Date(v.createdAt).toLocaleDateString()}</td>
                      <td className="px-5 py-4">
                        <button onClick={() => openReview(v)} className="text-xs text-slate-teal hover:text-emerald-400 font-medium transition-colors">
                          Review
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </motion.div>
        )}
      </div>

      {/* Review Modal */}
      <AnimatePresence>
        {showModal && selected && (
          <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
            className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm px-4"
          >
            <motion.div
              initial={{ opacity: 0, scale: 0.95, y: 20 }} animate={{ opacity: 1, scale: 1, y: 0 }} exit={{ opacity: 0, scale: 0.95, y: 20 }}
              className="glass-modal rounded-2xl max-w-4xl w-full max-h-[90vh] overflow-hidden shadow-glass-lg"
            >
              <div className="flex items-center justify-between px-6 py-4 border-b border-white/6">
                <div>
                  <h2 className="text-lg font-display font-bold text-white">Review {getTypeLabel(selected.verificationType)}</h2>
                  <p className="text-xs text-white/30">{selected.designTitle}</p>
                </div>
                <button onClick={closeReview} className="text-white/30 hover:text-white transition-colors">
                  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>
                </button>
              </div>

              <div className="px-6 py-5 overflow-y-auto max-h-[60vh]">
                <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                  <div className="lg:col-span-1 space-y-5">
                    <div className="glass-card-light rounded-lg p-4">
                      <p className="text-[10px] text-white/30 uppercase tracking-widest">Architect</p>
                      <p className="text-sm font-medium text-white mt-0.5">{selected.architectName}</p>
                      <p className="text-[10px] text-white/30 uppercase tracking-widest mt-3">Submitted</p>
                      <p className="text-sm text-white/60 mt-0.5">{new Date(selected.createdAt).toLocaleDateString()}</p>
                    </div>

                    <div>
                      <label className="block text-xs font-medium text-white/40 mb-1.5">Comments</label>
                      <textarea value={comments} onChange={(e) => setComments(e.target.value)} rows={4}
                        className="w-full glass-input rounded-lg px-4 py-3 text-sm text-white placeholder:text-white/20 resize-none"
                        placeholder="Add notes or feedback..." />
                    </div>
                  </div>

                  <div className="lg:col-span-2">
                    <h3 className="text-xs font-semibold uppercase tracking-widest text-white/30 mb-3">Files</h3>
                    {isFilesLoading ? (
                      <div className="space-y-3">{[...Array(3)].map((_, i) => <div key={i} className="glass-card-light rounded-lg h-14 shimmer-bg" />)}</div>
                    ) : files.length === 0 ? (
                      <div className="glass-card-light rounded-lg p-8 text-center"><p className="text-white/30 text-sm">No files available.</p></div>
                    ) : (
                      <div className="space-y-2">
                        {files.map((file) => (
                          <div key={file.id} className="flex items-center justify-between glass-card-light rounded-lg px-4 py-3">
                            <div>
                              <p className="text-sm font-medium text-white/70">{file.fileName}</p>
                              <p className="text-[10px] text-white/25">{file.category} • {formatFileSize(file.fileSizeBytes)}</p>
                            </div>
                            <a href={file.storageUrl} target="_blank" rel="noreferrer" className="text-xs text-slate-teal hover:text-emerald-400 font-medium transition-colors">
                              View
                            </a>
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                </div>
              </div>

              <div className="flex justify-end gap-3 px-6 py-4 border-t border-white/6">
                <button onClick={closeReview} className="px-4 py-2 text-sm text-white/40 hover:text-white transition-colors">Close</button>
                <button onClick={handleReject} disabled={isActionLoading}
                  className="px-4 py-2 text-sm font-medium text-white bg-red-500/80 rounded-lg hover:bg-red-500 transition-all disabled:opacity-50">
                  Reject
                </button>
                <button onClick={handleVerify} disabled={isActionLoading}
                  className="px-4 py-2 text-sm font-medium text-white bg-slate-teal rounded-lg hover:bg-teal-500 transition-all disabled:opacity-50">
                  Approve
                </button>
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>
    </EngineerLayout>
  );
}
