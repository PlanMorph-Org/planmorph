'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import Layout from '@/src/components/Layout';
import api from '@/src/lib/api';
import { Order, OrderFile } from '@/src/types';
import toast, { Toaster } from 'react-hot-toast';
import { useRoleGuard } from '@/src/hooks/useRoleGuard';
import { useCurrencyStore } from '@/src/store/currencyStore';
import { formatCurrency } from '@/src/lib/currency';
import { motion, AnimatePresence } from 'framer-motion';

const fadeUp = {
  hidden: { opacity: 0, y: 20 },
  visible: (i: number = 0) => ({ opacity: 1, y: 0, transition: { delay: i * 0.06, duration: 0.5, ease: [0.16, 1, 0.3, 1] } }) as any,
};

export default function MyOrdersPage() {
  const router = useRouter();
  const { isAuthorized, isChecking } = useRoleGuard({ requiredRole: 'Client', redirectTo: '/login', message: 'Please use the appropriate professional login portal.' });
  const [orders, setOrders] = useState<Order[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);
  const [orderFiles, setOrderFiles] = useState<OrderFile[]>([]);
  const [isLoadingFiles, setIsLoadingFiles] = useState(false);
  const [showConstructionRequest, setShowConstructionRequest] = useState(false);
  const [constructionLocation, setConstructionLocation] = useState('');
  const [isRequestingConstruction, setIsRequestingConstruction] = useState(false);
  const [isInitializingPaystack, setIsInitializingPaystack] = useState(false);
  const [showFileViewer, setShowFileViewer] = useState(false);
  const [viewerUrl, setViewerUrl] = useState('');
  const [viewerFileName, setViewerFileName] = useState('');
  const [viewerFileType, setViewerFileType] = useState('');
  const [isLoadingViewer, setIsLoadingViewer] = useState(false);
  const [pdfPage, setPdfPage] = useState(1);
  const [pdfZoom, setPdfZoom] = useState(120);
  const [pdfRotation, setPdfRotation] = useState(0);
  const { currency, rates } = useCurrencyStore();

  useEffect(() => { if (isAuthorized) loadOrders(); }, [isAuthorized]);

  useEffect(() => {
    if (selectedOrder) {
      setOrderFiles([]); setIsLoadingFiles(false); setShowFileViewer(false);
      setViewerUrl(''); setViewerFileName(''); setViewerFileType('');
      setPdfPage(1); setPdfZoom(120); setPdfRotation(0);
    }
  }, [selectedOrder?.id]);

  const loadOrders = async () => { setIsLoading(true); try { const r = await api.get<Order[]>('/orders/my-orders'); setOrders(r.data); } catch { toast.error('Failed to load orders'); } finally { setIsLoading(false); } };
  const openConstructionRequest = () => { setConstructionLocation(''); setShowConstructionRequest(true); };

  const initializePaystack = async () => {
    if (!selectedOrder) return; setIsInitializingPaystack(true);
    try { const r = await api.post('/payments/paystack/initialize', { orderId: selectedOrder.id }); const url = r.data?.authorizationUrl as string | undefined; if (!url) { toast.error('Unable to start Paystack payment.'); return; } window.location.href = url; }
    catch (e: any) { toast.error(e.response?.data?.message || 'Failed to start Paystack payment.'); }
    finally { setIsInitializingPaystack(false); }
  };

  const loadOrderFiles = async () => { if (!selectedOrder) return; setIsLoadingFiles(true); try { const r = await api.get<OrderFile[]>(`/orders/${selectedOrder.id}/files`); setOrderFiles(r.data); } catch (e: any) { toast.error(e.response?.data?.message || 'Failed to load files.'); } finally { setIsLoadingFiles(false); } };
  const fetchFileUrl = async (fileId: string) => { const r = await api.get<{ url: string }>(`/designs/files/${fileId}/download`); return r.data.url; };
  const downloadFile = async (fileId: string) => { try { const url = await fetchFileUrl(fileId); window.open(url, '_blank'); } catch (e: any) { toast.error(e.response?.data?.message || 'Failed to download file.'); } };

  const openFileViewer = async (file: OrderFile) => {
    setIsLoadingViewer(true);
    try { const url = await fetchFileUrl(file.id); setViewerUrl(url); setViewerFileName(file.fileName); setViewerFileType(file.fileType); setPdfPage(1); setPdfZoom(120); setPdfRotation(0); setShowFileViewer(true); }
    catch (e: any) { toast.error(e.response?.data?.message || 'Failed to open file.'); }
    finally { setIsLoadingViewer(false); }
  };

  const formatFileSize = (bytes: number) => { if (bytes < 1024) return `${bytes} B`; const kb = bytes / 1024; if (kb < 1024) return `${kb.toFixed(1)} KB`; return `${(kb / 1024).toFixed(1)} MB`; };
  const canInlineView = (ft: string) => ['Image', 'Video', 'PDF'].includes(ft);

  const groupFilesByCategory = (files: OrderFile[]) => {
    const groups: Record<string, OrderFile[]> = {};
    const getLabel = (c: string) => { switch (c) { case 'ArchitecturalDrawing': return 'Architectural Drawings'; case 'StructuralDrawing': return 'Structural Drawings'; case 'BOQ': return 'Bill of Quantities'; case 'FullRenderImage': return 'Render Images'; case 'FullRenderVideo': return 'Render Videos'; default: return 'Other Files'; } };
    files.forEach((f) => { const l = getLabel(f.category); if (!groups[l]) groups[l] = []; groups[l].push(f); });
    return groups;
  };

  const handleRequestConstruction = async () => {
    if (!selectedOrder) return; if (!constructionLocation.trim()) { toast.error('Please enter a construction location in Kenya.'); return; }
    setIsRequestingConstruction(true);
    try { await api.post(`/orders/${selectedOrder.id}/request-construction`, { location: constructionLocation, country: 'Kenya' }); toast.success('Construction request submitted.'); setShowConstructionRequest(false); setSelectedOrder(null); await loadOrders(); }
    catch (e: any) { toast.error(e.response?.data?.message || 'Failed to submit construction request.'); }
    finally { setIsRequestingConstruction(false); }
  };

  const getStatusStyle = (status: string) => {
    switch (status) {
      case 'Paid': return 'bg-verified/15 text-verified border-verified/20';
      case 'Pending': return 'bg-golden/15 text-golden border-golden/20';
      case 'Completed': return 'bg-brand-accent/15 text-brand-accent border-brand-accent/20';
      case 'Cancelled': return 'bg-red-500/15 text-red-400 border-red-500/20';
      default: return 'bg-white/10 text-white/50 border-white/10';
    }
  };

  if (isChecking || isLoading) {
    return (
      <Layout>
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
          <div className="h-8 w-40 shimmer-bg rounded mb-6" />
          <div className="space-y-4">
            {[...Array(3)].map((_, i) => (
              <div key={i} className="glass-card rounded-xl p-6"><div className="h-4 w-1/3 shimmer-bg rounded mb-3" /><div className="h-3 w-1/2 shimmer-bg rounded mb-2" /><div className="h-5 w-1/4 shimmer-bg rounded" /></div>
            ))}
          </div>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <Toaster position="top-right" toastOptions={{ style: { background: '#1F2937', color: '#E5E7EB', border: '1px solid rgba(255,255,255,0.08)' } }} />

      {/* Header */}
      <section className="relative py-14 border-b border-white/6">
        <div className="absolute inset-0 bg-hero-gradient" />
        <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <motion.h1 initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }} className="text-2xl md:text-3xl font-display font-bold text-white tracking-tight mb-1">
            My Orders
          </motion.h1>
          <motion.p initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.1 }} className="text-xs text-white/30">
            Amounts shown in {currency}. Billing currency is KES.
          </motion.p>
        </div>
      </section>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {orders.length === 0 ? (
          <div className="text-center py-20">
            <div className="w-16 h-16 mx-auto mb-4 glass-card rounded-full flex items-center justify-center">
              <svg className="w-7 h-7 text-white/20" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" /></svg>
            </div>
            <p className="text-white/50 mb-1">No orders yet</p>
            <p className="text-xs text-white/25 mb-6">Browse verified designs and make your first purchase.</p>
            <button onClick={() => router.push('/designs')} className="px-6 py-2.5 bg-brand-accent text-white font-medium rounded-lg hover:bg-blue-500 transition-all duration-300 shadow-blue btn-glow text-sm">
              Browse Designs
            </button>
          </div>
        ) : (
          <div className="space-y-4">
            {orders.map((order, i) => (
              <motion.div key={order.id} variants={fadeUp} initial="hidden" animate="visible" custom={i}
                className="glass-card rounded-xl p-5 card-hover cursor-pointer"
                onClick={() => setSelectedOrder(order)}
              >
                <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
                  <div className="flex-1">
                    <div className="flex items-center gap-3 mb-2">
                      <span className="text-sm font-mono font-semibold text-brand-accent">{order.orderNumber}</span>
                      <span className={`px-2.5 py-0.5 text-[10px] font-semibold rounded-full border ${getStatusStyle(order.status)}`}>
                        {order.status}
                      </span>
                    </div>
                    <p className="text-sm text-white/60 mb-1">{order.designTitle}</p>
                    <p className="text-xs text-white/25">Ordered {new Date(order.createdAt).toLocaleDateString()}</p>
                    {order.includesConstruction && (
                      <span className="inline-flex items-center gap-1 mt-2 px-2 py-0.5 text-[10px] font-medium text-purple-300 bg-purple-500/10 rounded-full border border-purple-500/20">
                        <svg className="w-2 h-2" fill="currentColor" viewBox="0 0 8 8"><circle cx={4} cy={4} r={3} /></svg>
                        Construction Included
                      </span>
                    )}
                  </div>
                  <div className="text-right">
                    <p className="text-lg font-bold text-gradient-golden">{formatCurrency(order.amount, currency, rates)}</p>
                    <span className="text-xs text-brand-accent hover:text-blue-400 transition-colors">View Details →</span>
                  </div>
                </div>
              </motion.div>
            ))}
          </div>
        )}
      </div>

      {/* Order Details Modal */}
      <AnimatePresence>
        {selectedOrder && (
          <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
            className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4 z-50"
          >
            <motion.div
              initial={{ opacity: 0, scale: 0.95, y: 20 }} animate={{ opacity: 1, scale: 1, y: 0 }} exit={{ opacity: 0, scale: 0.95, y: 20 }}
              transition={{ duration: 0.3, ease: [0.16, 1, 0.3, 1] }}
              className="glass-modal rounded-2xl max-w-2xl w-full p-6 max-h-[90vh] overflow-y-auto shadow-glass-lg"
            >
              <div className="flex justify-between items-center mb-5">
                <h3 className="text-lg font-display font-bold text-white">Order Details</h3>
                <button onClick={() => { setShowConstructionRequest(false); setSelectedOrder(null); }} className="text-white/30 hover:text-white transition-colors">
                  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>
                </button>
              </div>

              <div className="space-y-5">
                <div className="glass-card-light rounded-lg p-4 flex justify-between items-center">
                  <div>
                    <p className="text-xs text-white/30 mb-0.5">Order Number</p>
                    <p className="text-sm font-mono font-semibold text-white">{selectedOrder.orderNumber}</p>
                  </div>
                  <span className={`px-3 py-1 text-xs font-semibold rounded-full border ${getStatusStyle(selectedOrder.status)}`}>
                    {selectedOrder.status}
                  </span>
                </div>

                <div>
                  <p className="text-xs text-white/30 mb-0.5">Design</p>
                  <p className="text-sm text-white/70">{selectedOrder.designTitle}</p>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <p className="text-xs text-white/30 mb-0.5">Amount</p>
                    <p className="text-lg font-bold text-gradient-golden">{formatCurrency(selectedOrder.amount, currency, rates)}</p>
                  </div>
                  <div>
                    <p className="text-xs text-white/30 mb-0.5">Payment Method</p>
                    <p className="text-sm text-white/60">{selectedOrder.paymentMethod}</p>
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <p className="text-xs text-white/30 mb-0.5">Date Ordered</p>
                    <p className="text-sm text-white/60">{new Date(selectedOrder.createdAt).toLocaleDateString()}</p>
                  </div>
                  {selectedOrder.paidAt && (
                    <div>
                      <p className="text-xs text-white/30 mb-0.5">Paid At</p>
                      <p className="text-sm text-white/60">{new Date(selectedOrder.paidAt).toLocaleString()}</p>
                    </div>
                  )}
                </div>

                {/* Construction Contract */}
                {selectedOrder.includesConstruction && selectedOrder.constructionContract && (
                  <div className="border-t border-white/6 pt-4">
                    <h4 className="text-xs font-semibold uppercase tracking-widest text-purple-300/70 mb-3">Construction Contract</h4>
                    <div className="space-y-3">
                      <div><p className="text-xs text-white/30">Location</p><p className="text-sm text-white/60">{selectedOrder.constructionContract.location}</p></div>
                      <div className="grid grid-cols-2 gap-4">
                        <div><p className="text-xs text-white/30">Estimated Cost</p><p className="text-sm font-semibold text-white/70">{formatCurrency(selectedOrder.constructionContract.estimatedCost, currency, rates)}</p></div>
                        <div><p className="text-xs text-white/30">Commission</p><p className="text-sm font-semibold text-white/70">{formatCurrency(selectedOrder.constructionContract.commissionAmount, currency, rates)}</p></div>
                      </div>
                      <div><p className="text-xs text-white/30">Status</p><span className={`px-2 py-0.5 text-[10px] font-semibold rounded-full border ${getStatusStyle(selectedOrder.constructionContract.status)}`}>{selectedOrder.constructionContract.status}</span></div>
                      {selectedOrder.constructionContract.contractorName && (
                        <div><p className="text-xs text-white/30">Contractor</p><p className="text-sm text-white/60">{selectedOrder.constructionContract.contractorName}</p></div>
                      )}
                    </div>
                  </div>
                )}

                {/* Paid/Completed - Load Files */}
                {(selectedOrder.status === 'Paid' || selectedOrder.status === 'Completed') && (
                  <div className="glass-card-light rounded-lg p-4 border border-verified/20">
                    <p className="text-sm text-verified mb-2">✓ Order confirmed. You can now download your design files.</p>
                    <button onClick={loadOrderFiles} disabled={isLoadingFiles} className="text-sm font-medium text-verified hover:text-emerald-300 transition-colors disabled:opacity-50">
                      {isLoadingFiles ? 'Loading files...' : 'Load Files →'}
                    </button>
                  </div>
                )}

                {/* File Categories */}
                {(selectedOrder.status === 'Paid' || selectedOrder.status === 'Completed') && orderFiles.length > 0 && (
                  <div className="space-y-3">
                    {Object.entries(groupFilesByCategory(orderFiles)).map(([category, files]) => (
                      <div key={category} className="glass-card-light rounded-lg overflow-hidden">
                        <div className="flex items-center justify-between px-4 py-3 border-b border-white/6">
                          <div><p className="text-sm font-semibold text-white">{category}</p><p className="text-[10px] text-white/25">{files.length} file(s)</p></div>
                          <button className="text-[10px] text-brand-accent hover:text-blue-400 transition-colors" onClick={() => files.forEach((f) => downloadFile(f.id))}>Download All</button>
                        </div>
                        <div className="divide-y divide-white/6">
                          {files.map((file) => (
                            <div key={file.id} className="flex items-center justify-between px-4 py-3">
                              <div><p className="text-sm text-white/70">{file.fileName}</p><p className="text-[10px] text-white/25">{file.fileType} • {formatFileSize(file.fileSizeBytes)}</p></div>
                              <div className="flex items-center gap-3">
                                <button onClick={() => openFileViewer(file)} disabled={isLoadingViewer || !canInlineView(file.fileType)} className="text-xs text-white/40 hover:text-white transition-colors disabled:opacity-30">
                                  {canInlineView(file.fileType) ? 'View' : 'N/A'}
                                </button>
                                <button onClick={() => downloadFile(file.id)} className="text-xs text-brand-accent hover:text-blue-400 transition-colors">Download</button>
                              </div>
                            </div>
                          ))}
                        </div>
                      </div>
                    ))}
                  </div>
                )}

                {/* Construction Request CTA */}
                {(selectedOrder.status === 'Paid' || selectedOrder.status === 'Completed') && !selectedOrder.includesConstruction && (
                  <div className="glass-card-light rounded-lg p-4 border border-brand-accent/20">
                    <p className="text-sm text-brand-accent">Need construction services? Available in Kenya only.</p>
                    <button onClick={openConstructionRequest} className="mt-2 text-sm font-medium text-brand-accent hover:text-blue-400 transition-colors">Request Construction →</button>
                  </div>
                )}

                {/* Pending payment */}
                {selectedOrder.status === 'Pending' && (
                  <div className="glass-card-light rounded-lg p-4 border border-golden/20">
                    <p className="text-sm text-golden">⏳ Awaiting payment. Please complete to access design files.</p>
                    <button onClick={initializePaystack} disabled={isInitializingPaystack} className="mt-2 text-sm font-medium text-golden hover:text-golden-light transition-colors disabled:opacity-50">
                      {isInitializingPaystack ? 'Redirecting to Paystack...' : 'Pay with Paystack →'}
                    </button>
                  </div>
                )}
              </div>

              <button onClick={() => { setShowConstructionRequest(false); setSelectedOrder(null); }}
                className="w-full mt-6 py-2.5 border border-white/10 rounded-lg text-sm text-white/40 hover:text-white hover:bg-white/5 transition-all">
                Close
              </button>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Construction Request Modal */}
      <AnimatePresence>
        {showConstructionRequest && selectedOrder && (
          <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
            className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4 z-[60]"
          >
            <motion.div
              initial={{ opacity: 0, scale: 0.95, y: 20 }} animate={{ opacity: 1, scale: 1, y: 0 }} exit={{ opacity: 0, scale: 0.95, y: 20 }}
              className="glass-modal rounded-2xl max-w-md w-full p-6 shadow-glass-lg"
            >
              <div className="flex justify-between items-center mb-4">
                <h3 className="text-lg font-display font-bold text-white">Request Construction</h3>
                <button onClick={() => setShowConstructionRequest(false)} className="text-white/30 hover:text-white transition-colors">
                  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>
                </button>
              </div>

              <div className="mb-4 text-xs text-white/30 space-y-0.5">
                <p>Order: {selectedOrder.orderNumber}</p>
                <p>Design: {selectedOrder.designTitle}</p>
              </div>

              <div className="mb-5">
                <label className="block text-xs font-medium text-white/40 mb-1.5">Construction Location (Kenya)</label>
                <input type="text" value={constructionLocation} onChange={(e) => setConstructionLocation(e.target.value)}
                  placeholder="e.g., Nairobi, Kenya" className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20" />
                <p className="text-[10px] text-white/20 mt-1.5">We'll connect you with a verified contractor. A 2% commission applies.</p>
              </div>

              <div className="flex gap-3">
                <button onClick={() => setShowConstructionRequest(false)} disabled={isRequestingConstruction}
                  className="flex-1 py-2.5 border border-white/10 rounded-lg text-sm text-white/50 hover:text-white hover:bg-white/5 transition-all disabled:opacity-50">
                  Cancel
                </button>
                <button onClick={handleRequestConstruction} disabled={isRequestingConstruction}
                  className="flex-1 py-2.5 bg-brand-accent text-white font-medium rounded-lg hover:bg-blue-500 transition-all btn-glow disabled:opacity-50">
                  {isRequestingConstruction ? 'Submitting...' : 'Submit Request'}
                </button>
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* File Viewer Modal */}
      <AnimatePresence>
        {showFileViewer && (
          <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
            className="fixed inset-0 bg-black/90 backdrop-blur-sm flex items-center justify-center p-4 z-[60]"
          >
            <motion.div
              initial={{ opacity: 0, scale: 0.97 }} animate={{ opacity: 1, scale: 1 }} exit={{ opacity: 0, scale: 0.97 }}
              className="glass-modal rounded-2xl w-full h-full max-w-6xl max-h-[95vh] overflow-hidden flex flex-col shadow-glass-lg"
            >
              {/* File viewer header */}
              <div className="flex flex-wrap items-center justify-between gap-3 px-5 py-3 border-b border-white/6">
                <div>
                  <h3 className="text-sm font-semibold text-white">File Viewer</h3>
                  <p className="text-xs text-white/30">{viewerFileName}</p>
                </div>

                {viewerFileType === 'PDF' && (
                  <div className="flex flex-wrap items-center gap-1.5">
                    {[
                      { label: '←', action: () => setPdfPage((p) => Math.max(1, p - 1)) },
                      { label: '→', action: () => setPdfPage((p) => p + 1) },
                      { label: '+', action: () => setPdfZoom((z) => Math.min(300, z + 10)) },
                      { label: '−', action: () => setPdfZoom((z) => Math.max(50, z - 10)) },
                      { label: '↻', action: () => setPdfRotation((r) => (r + 90) % 360) },
                    ].map((btn) => (
                      <button key={btn.label} onClick={btn.action} className="px-2.5 py-1 text-xs glass-card-light rounded-md text-white/50 hover:text-white transition-colors">
                        {btn.label}
                      </button>
                    ))}
                    <span className="text-[10px] text-white/25 ml-1">Page {pdfPage} • {pdfZoom}%</span>
                  </div>
                )}

                <button onClick={() => setShowFileViewer(false)} className="text-white/30 hover:text-white transition-colors">
                  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>
                </button>
              </div>

              <div className="flex-1 bg-brand overflow-auto">
                {viewerFileType === 'PDF' && viewerUrl && (
                  <div className="w-full h-full flex items-center justify-center" style={{ transform: `rotate(${pdfRotation}deg)` }}>
                    <iframe key={`${viewerUrl}-${pdfPage}-${pdfZoom}`} src={`${viewerUrl.split('#')[0]}#page=${pdfPage}&zoom=${pdfZoom}`} className="w-full h-full" title={viewerFileName} />
                  </div>
                )}
                {viewerFileType === 'Image' && <div className="w-full h-full flex items-center justify-center p-4"><img src={viewerUrl} alt={viewerFileName} className="max-h-[85vh] object-contain rounded-lg" /></div>}
                {viewerFileType === 'Video' && <div className="w-full h-full flex items-center justify-center p-4"><video controls className="w-full max-h-[85vh] rounded-lg"><source src={viewerUrl} />Your browser does not support the video tag.</video></div>}
                {viewerFileType === 'CAD' && <div className="glass-card-light rounded-lg p-5 m-6 text-sm text-white/40">This file type cannot be previewed in the browser. Please download to view.</div>}
              </div>

              <div className="px-5 py-3 border-t border-white/6 flex justify-between items-center">
                <span className="text-[10px] text-white/20">Viewer: {viewerFileType}</span>
                <button onClick={() => viewerUrl && window.open(viewerUrl, '_blank')} className="px-4 py-2 bg-brand-accent text-white text-sm font-medium rounded-lg hover:bg-blue-500 transition-all btn-glow">Download</button>
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>
    </Layout>
  );
}
