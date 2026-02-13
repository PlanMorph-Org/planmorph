'use client';

import { useEffect, useMemo, useState } from 'react';
import toast, { Toaster } from 'react-hot-toast';
import api from '@/src/lib/api';
import ArchitectLayout from '@/src/components/ArchitectLayout';
import { useRoleGuard } from '@/src/hooks/useRoleGuard';
import { motion, AnimatePresence } from 'framer-motion';

interface Verification { id: string; designId: string; designTitle: string; architectName: string; verificationType: string; status: string; comments?: string; createdAt: string; }
interface VerificationFile { id: string; fileName: string; category: string; storageUrl: string; fileSizeBytes: number; }
type FilterKey = 'All' | 'Architectural' | 'BOQ';

export default function ArchitectVerificationsPage() {
  const { isAuthorized } = useRoleGuard({ requiredRole: 'Architect', redirectTo: '/architect/login', message: 'Access denied. Architect account required.' });
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

  const loadVerifications = async () => {
    setIsLoading(true);
    try { const r = await api.get<Verification[]>('/design-verifications/pending'); setVerifications(r.data || []); }
    catch (e: any) { toast.error(e.response?.data?.message || 'Failed to load verifications.'); }
    finally { setIsLoading(false); }
  };

  const filteredVerifications = useMemo(() => {
    if (filter === 'All') return verifications;
    if (filter === 'Architectural') return verifications.filter(v => v.verificationType === 'Architectural');
    return verifications.filter(v => v.verificationType === 'BOQ' || v.verificationType === 'BOQArchitect');
  }, [verifications, filter]);

  const totalPending = verifications.length;
  const architecturalPending = verifications.filter(v => v.verificationType === 'Architectural').length;
  const boqPending = verifications.filter(v => v.verificationType === 'BOQ' || v.verificationType === 'BOQArchitect').length;

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

  const getTypeLabel = (t: string) => { switch (t) { case 'Architectural': return 'Architectural'; case 'BOQArchitect': return 'BOQ (Architect)'; case 'BOQ': return 'BOQ'; default: return t; } };
  const getTypeBadge = (t: string) => t === 'Architectural' ? 'bg-brand-accent/15 text-brand-accent border-brand-accent/20' : 'bg-golden/15 text-golden border-golden/20';
  const getStatusBadge = (s: string) => { switch (s) { case 'Verified': return 'bg-verified/15 text-verified border-verified/20'; case 'Pending': return 'bg-golden/15 text-golden border-golden/20'; case 'Rejected': return 'bg-rose-500/15 text-rose-400 border-rose-500/20'; default: return 'bg-white/5 text-white/30 border-white/10'; } };

  const statCards = [
    { label: 'Pending Verifications', value: totalPending, color: 'text-golden' },
    { label: 'Architectural', value: architecturalPending, color: 'text-brand-accent' },
    { label: 'BOQ', value: boqPending, color: 'text-golden' },
  ];

  return (
    <ArchitectLayout>
      <Toaster position="top-right" toastOptions={{ style: { background: '#1F2937', color: '#E5E7EB', border: '1px solid rgba(255,255,255,0.08)' } }} />
      <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <motion.div initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }} className="mb-8">
          <h1 className="text-2xl md:text-3xl font-display font-bold text-white">Verification Dashboard</h1>
          <p className="text-sm text-white/40 mt-1">Review architectural drawings and BOQ submissions.</p>
        </motion.div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
          {statCards.map((card, i) => (
            <motion.div key={card.label} initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: i * 0.08 }} className="glass-card rounded-xl p-5 card-hover">
              <p className="text-[10px] text-white/30 uppercase tracking-widest mb-1">{card.label}</p>
              <p className={`text-2xl font-bold font-mono ${card.color}`}>{card.value}</p>
            </motion.div>
          ))}
        </div>

        <div className="flex flex-wrap gap-2 mb-6">
          {(['All', 'Architectural', 'BOQ'] as FilterKey[]).map(key => (
            <button key={key} onClick={() => setFilter(key)}
              className={`px-4 py-1.5 rounded-full text-xs font-medium border transition-all duration-200 ${filter === key ? 'bg-golden/20 text-golden border-golden/40' : 'bg-white/5 text-white/30 border-white/10 hover:border-golden/30 hover:text-white/50'}`}
            >{key}</button>
          ))}
        </div>

        {isLoading ? (
          <div className="space-y-3">{[...Array(3)].map((_, i) => <div key={i} className="glass-card rounded-xl h-16 shimmer-bg" />)}</div>
        ) : filteredVerifications.length === 0 ? (
          <div className="glass-card rounded-xl p-12 text-center">
            <div className="w-14 h-14 mx-auto mb-3 glass-card-light rounded-full flex items-center justify-center">
              <svg className="w-6 h-6 text-white/20" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
            </div>
            <p className="text-white/40 text-sm">No pending verifications.</p>
          </div>
        ) : (
          <motion.div initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.3 }} className="glass-card rounded-xl overflow-hidden">
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
                {filteredVerifications.map(v => (
                  <tr key={v.id} className="hover:bg-white/[0.02] transition-colors">
                    <td className="px-5 py-4 text-white/70 font-medium">{v.designTitle}</td>
                    <td className="px-5 py-4 text-white/40">{v.architectName}</td>
                    <td className="px-5 py-4"><span className={`px-2.5 py-0.5 rounded-full text-[10px] font-semibold border ${getTypeBadge(v.verificationType)}`}>{getTypeLabel(v.verificationType)}</span></td>
                    <td className="px-5 py-4"><span className={`px-2.5 py-0.5 rounded-full text-[10px] font-semibold border ${getStatusBadge(v.status)}`}>{v.status}</span></td>
                    <td className="px-5 py-4 text-white/30 text-xs">{new Date(v.createdAt).toLocaleDateString()}</td>
                    <td className="px-5 py-4">
                      <button onClick={() => openReview(v)} className="text-golden hover:text-golden-light text-xs font-medium transition-colors">Review</button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </motion.div>
        )}
      </div>

      {/* Review Modal */}
      <AnimatePresence>
        {showModal && selected && (
          <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }} className="fixed inset-0 z-50 flex items-center justify-center bg-black/70 backdrop-blur-sm px-4">
            <motion.div initial={{ scale: 0.95, opacity: 0 }} animate={{ scale: 1, opacity: 1 }} exit={{ scale: 0.95, opacity: 0 }} transition={{ duration: 0.25 }}
              className="glass-card rounded-2xl max-w-4xl w-full max-h-[90vh] overflow-hidden border border-white/10">
              <div className="flex items-center justify-between px-6 py-4 border-b border-white/6">
                <div>
                  <h2 className="text-base font-semibold text-white">Review {getTypeLabel(selected.verificationType)}</h2>
                  <p className="text-xs text-white/30">{selected.designTitle}</p>
                </div>
                <button onClick={closeReview} className="w-8 h-8 rounded-full glass-card-light flex items-center justify-center text-white/40 hover:text-white transition-colors text-sm">✕</button>
              </div>

              <div className="px-6 py-4 overflow-y-auto max-h-[60vh]">
                <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                  <div className="lg:col-span-1 space-y-4">
                    <div className="glass-card-light rounded-lg p-4 text-sm">
                      <p className="text-white/30 text-[10px] uppercase tracking-widest mb-0.5">Architect</p>
                      <p className="font-medium text-white/70">{selected.architectName}</p>
                      <p className="mt-3 text-white/30 text-[10px] uppercase tracking-widest mb-0.5">Submitted</p>
                      <p className="font-medium text-white/70">{new Date(selected.createdAt).toLocaleDateString()}</p>
                    </div>
                    <div>
                      <label className="block text-xs text-white/30 mb-1.5">Comments</label>
                      <textarea value={comments} onChange={e => setComments(e.target.value)} rows={4}
                        className="w-full glass-input rounded-lg px-4 py-3 text-sm text-white placeholder:text-white/20 focus:outline-none resize-none"
                        placeholder="Add notes or feedback..." />
                    </div>
                  </div>

                  <div className="lg:col-span-2">
                    <h3 className="text-xs font-semibold text-white/40 uppercase tracking-widest mb-3">Files</h3>
                    {isFilesLoading ? (
                      <div className="text-center py-10"><div className="w-6 h-6 border-2 border-golden/30 border-t-golden rounded-full animate-spin mx-auto" /></div>
                    ) : files.length === 0 ? (
                      <div className="glass-card-light rounded-lg p-8 text-center border border-dashed border-white/10">
                        <p className="text-white/30 text-sm">No files for this verification.</p>
                      </div>
                    ) : (
                      <div className="space-y-2">
                        {files.map(file => (
                          <div key={file.id} className="flex items-center justify-between glass-card-light rounded-lg px-4 py-3">
                            <div>
                              <p className="text-sm font-medium text-white/70">{file.fileName}</p>
                              <p className="text-[10px] text-white/30">{file.category} • {formatFileSize(file.fileSizeBytes)}</p>
                            </div>
                            <a href={file.storageUrl} target="_blank" rel="noreferrer" className="text-golden hover:text-golden-light text-xs font-medium transition-colors">View</a>
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
                  className="px-4 py-2 text-sm font-medium text-white bg-rose-500/20 border border-rose-500/30 rounded-lg hover:bg-rose-500/30 disabled:opacity-50 transition-all">Reject</button>
                <button onClick={handleVerify} disabled={isActionLoading}
                  className="px-4 py-2 text-sm font-medium text-brand bg-golden rounded-lg hover:bg-golden-light disabled:opacity-50 transition-all">Approve</button>
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>
    </ArchitectLayout>
  );
}
