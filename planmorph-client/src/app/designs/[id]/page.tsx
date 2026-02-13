'use client';

import { useEffect, useMemo, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import Layout from '@/src/components/Layout';
import api from '@/src/lib/api';
import { Design, CreateOrderDto } from '@/src/types';
import { useAuthStore } from '@/src/store/authStore';
import toast, { Toaster } from 'react-hot-toast';
import { useCurrencyStore } from '@/src/store/currencyStore';
import { formatCurrency } from '@/src/lib/currency';
import { ShieldCheckIcon, CheckCircleIcon } from '@heroicons/react/24/solid';
import { motion, AnimatePresence } from 'framer-motion';

export default function DesignDetailPage() {
  const params = useParams();
  const router = useRouter();
  const { isAuthenticated, user, logout } = useAuthStore();
  const { currency, rates } = useCurrencyStore();
  const [design, setDesign] = useState<Design | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [showPurchaseModal, setShowPurchaseModal] = useState(false);
  const [includesConstruction, setIncludesConstruction] = useState(false);
  const [constructionLocation, setConstructionLocation] = useState('');
  const [paymentMethod] = useState<'Paystack'>('Paystack');
  const [constructionCountry] = useState('Kenya');
  const [isPurchasing, setIsPurchasing] = useState(false);
  const [activeMediaIndex, setActiveMediaIndex] = useState(0);
  const [isLightboxOpen, setIsLightboxOpen] = useState(false);
  const [lightboxIndex, setLightboxIndex] = useState(0);

  useEffect(() => { if (params.id) loadDesign(params.id as string); }, [params.id]);
  useEffect(() => { setActiveMediaIndex(0); }, [design?.id]);

  const mediaItems = useMemo(() => {
    if (!design) return [];
    return [
      ...design.previewImages.map((url) => ({ type: 'image' as const, url })),
      ...design.previewVideos.map((url) => ({ type: 'video' as const, url })),
    ];
  }, [design]);

  useEffect(() => {
    if (!isLightboxOpen) return;
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') { setIsLightboxOpen(false); return; }
      if (event.key === 'ArrowRight') setLightboxIndex((prev) => (prev + 1) % mediaItems.length);
      if (event.key === 'ArrowLeft') setLightboxIndex((prev) => (prev - 1 + mediaItems.length) % mediaItems.length);
    };
    document.body.style.overflow = 'hidden';
    window.addEventListener('keydown', handleKeyDown);
    return () => { document.body.style.overflow = ''; window.removeEventListener('keydown', handleKeyDown); };
  }, [isLightboxOpen, mediaItems.length]);

  const openLightbox = (index: number) => { if (mediaItems.length === 0) return; setLightboxIndex(index); setIsLightboxOpen(true); };
  const closeLightbox = () => setIsLightboxOpen(false);
  const showNextMedia = () => setLightboxIndex((prev) => (prev + 1) % mediaItems.length);
  const showPrevMedia = () => setLightboxIndex((prev) => (prev - 1 + mediaItems.length) % mediaItems.length);

  const loadDesign = async (id: string) => {
    setIsLoading(true);
    try { const response = await api.get<Design>(`/designs/${id}`); setDesign(response.data); }
    catch { toast.error('Failed to load design'); router.push('/designs'); }
    finally { setIsLoading(false); }
  };

  const handlePurchase = async () => {
    if (!isAuthenticated) { toast.error('Please login to purchase'); router.push('/login'); return; }
    if (user?.role !== 'Client') { logout(); toast.error('Please use the client login to make purchases.'); router.push('/login'); return; }
    if (includesConstruction && !constructionLocation.trim()) { toast.error('Please enter construction location'); return; }
    if (includesConstruction && constructionCountry !== 'Kenya') { toast.error('Construction services are currently available only in Kenya.'); return; }

    setIsPurchasing(true);
    try {
      const orderData: CreateOrderDto = {
        designId: design!.id, paymentMethod, includesConstruction,
        constructionLocation: includesConstruction ? constructionLocation : undefined,
        constructionCountry: includesConstruction ? constructionCountry : undefined,
      };
      const response = await api.post('/orders', orderData);
      toast.success('Order created successfully!');
      setShowPurchaseModal(false);
      toast.success(`Order Number: ${response.data.orderNumber}. Please proceed with payment.`, { duration: 5000 });
      setTimeout(() => router.push('/my-orders'), 2000);
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Purchase failed. Please try again.');
    } finally { setIsPurchasing(false); }
  };

  if (isLoading) {
    return (
      <Layout>
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-10">
            <div className="aspect-[4/3] glass-card rounded-2xl shimmer-bg" />
            <div className="space-y-4">
              <div className="h-6 w-24 shimmer-bg rounded" />
              <div className="h-8 w-3/4 shimmer-bg rounded" />
              <div className="h-4 w-full shimmer-bg rounded" />
              <div className="h-4 w-2/3 shimmer-bg rounded" />
              <div className="h-32 glass-card rounded-lg shimmer-bg mt-6" />
            </div>
          </div>
        </div>
      </Layout>
    );
  }

  if (!design) {
    return (
      <Layout>
        <div className="max-w-7xl mx-auto px-4 py-20 text-center">
          <p className="text-white/40">Design not found</p>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <Toaster position="top-right" toastOptions={{ style: { background: '#1F2937', color: '#E5E7EB', border: '1px solid rgba(255,255,255,0.08)' } }} />
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Breadcrumb */}
        <nav className="mb-8">
          <ol className="flex items-center space-x-2 text-sm text-white/30">
            <li><a href="/designs" className="hover:text-brand-accent transition-colors hover-underline">Designs</a></li>
            <li className="text-white/15">/</li>
            <li className="text-white/60">{design.title}</li>
          </ol>
        </nav>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-10">
          {/* Left: Gallery */}
          <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.6, ease: [0.16, 1, 0.3, 1] }}>
            {(() => {
              const activeItem = mediaItems[activeMediaIndex];
              return (
                <>
                  <button
                    type="button"
                    onClick={() => openLightbox(activeMediaIndex)}
                    className="relative w-full glass-card rounded-2xl overflow-hidden aspect-[4/3] group"
                  >
                    {activeItem ? (
                      activeItem.type === 'image' ? (
                        <img src={activeItem.url} alt={design.title} className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-105" />
                      ) : (
                        <video controls className="w-full h-full object-cover"><source src={activeItem.url} type="video/mp4" /></video>
                      )
                    ) : (
                      <div className="w-full h-full flex items-center justify-center text-white/20">No Preview</div>
                    )}
                    <div className="absolute top-3 left-3 glass-card text-white/70 text-xs font-medium px-3 py-1.5 rounded-full">
                      Gallery &middot; {mediaItems.length}
                    </div>
                    {activeItem && (
                      <div className="absolute inset-0 bg-black/20 opacity-0 group-hover:opacity-100 transition-opacity duration-300 flex items-center justify-center">
                        <span className="text-white/80 text-sm font-medium glass-card px-4 py-2 rounded-lg">Click to expand</span>
                      </div>
                    )}
                  </button>

                  {mediaItems.length > 1 && (
                    <div className="mt-4 flex gap-3 overflow-x-auto pb-2">
                      {mediaItems.map((item, index) => (
                        <button
                          key={`${item.type}-${index}`}
                          onClick={() => setActiveMediaIndex(index)}
                          className={`relative flex-shrink-0 w-24 h-20 rounded-lg overflow-hidden border-2 transition-all duration-300 ${activeMediaIndex === index ? 'border-brand-accent shadow-blue' : 'border-white/10 hover:border-white/20'
                            }`}
                        >
                          {item.type === 'image' ? (
                            <img src={item.url} alt={`Preview ${index + 1}`} className="w-full h-full object-cover" />
                          ) : (
                            <div className="w-full h-full bg-brand-light flex items-center justify-center relative">
                              <video muted preload="metadata" className="w-full h-full object-cover"><source src={item.url} type="video/mp4" /></video>
                              <div className="absolute inset-0 flex items-center justify-center">
                                <div className="glass-card rounded-full p-1.5"><svg className="w-3 h-3 text-white" fill="currentColor" viewBox="0 0 20 20"><path d="M6.5 5.5a1 1 0 011.5-.87l7 4.5a1 1 0 010 1.74l-7 4.5A1 1 0 016.5 14V5.5z" /></svg></div>
                              </div>
                            </div>
                          )}
                        </button>
                      ))}
                    </div>
                  )}
                </>
              );
            })()}
          </motion.div>

          {/* Right: Details */}
          <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.15, duration: 0.6, ease: [0.16, 1, 0.3, 1] }}>
            <div className="mb-6">
              <span className="inline-block px-3 py-1 text-xs font-semibold text-brand-accent bg-brand-accent/10 rounded-full mb-3">
                {design.category}
              </span>
              <div className="flex items-center gap-1.5 text-verified text-xs font-medium mt-1 mb-3">
                <ShieldCheckIcon className="h-4 w-4" />
                Engineer Verified
              </div>
              <h1 className="text-2xl md:text-3xl font-display font-bold text-white mb-4">{design.title}</h1>
              <p className="text-white/40 text-sm leading-relaxed mb-6">{design.description}</p>
            </div>

            {/* Specs */}
            <div className="glass-card rounded-xl p-6 mb-6">
              <h3 className="text-xs font-semibold uppercase tracking-widest text-white/30 mb-4">Specifications</h3>
              <div className="grid grid-cols-2 gap-4">
                {[
                  { label: 'Bedrooms', value: design.bedrooms },
                  { label: 'Bathrooms', value: design.bathrooms },
                  { label: 'Square Footage', value: `${design.squareFootage.toLocaleString()} sqft` },
                  { label: 'Stories', value: design.stories },
                ].map((spec) => (
                  <div key={spec.label}>
                    <p className="text-xs text-white/30">{spec.label}</p>
                    <p className="text-lg font-semibold text-white font-mono">{spec.value}</p>
                  </div>
                ))}
              </div>
            </div>

            {/* Pricing */}
            <div className="glass-card rounded-xl p-6 mb-6">
              <div className="flex justify-between items-center mb-3">
                <span className="text-sm text-white/40">Design Plans</span>
                <span className="text-2xl font-bold text-gradient-golden">
                  {formatCurrency(design.price, currency, rates)}
                </span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-xs text-white/30">Est. Construction Cost</span>
                <span className="text-sm font-semibold text-white/60">
                  {formatCurrency(design.estimatedConstructionCost, currency, rates)}
                </span>
              </div>
              <p className="text-[10px] text-white/20 mt-3">Prices shown in {currency}. Billing currency is KES.</p>
            </div>

            {/* What's Included */}
            <div className="mb-6">
              <h3 className="text-xs font-semibold uppercase tracking-widest text-white/30 mb-3">What&apos;s Included</h3>
              <ul className="space-y-2">
                {['Complete architectural drawings', 'Structural/Engineering drawings', 'Bill of Quantities (BOQ)', 'High-resolution 3D renders', 'Modification support'].map((item) => (
                  <li key={item} className="flex items-start gap-2">
                    <CheckCircleIcon className="w-4 h-4 text-verified mt-0.5 flex-shrink-0" />
                    <span className="text-sm text-white/50">{item}</span>
                  </li>
                ))}
              </ul>
            </div>

            <button
              onClick={() => setShowPurchaseModal(true)}
              className="w-full py-3.5 bg-brand-accent text-white font-semibold rounded-xl hover:bg-blue-500 transition-all duration-300 shadow-blue btn-glow text-lg"
            >
              Purchase Now
            </button>
          </motion.div>
        </div>
      </div>

      {/* Purchase Modal */}
      <AnimatePresence>
        {showPurchaseModal && (
          <motion.div
            initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
            className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4 z-50"
          >
            <motion.div
              initial={{ opacity: 0, scale: 0.95, y: 20 }}
              animate={{ opacity: 1, scale: 1, y: 0 }}
              exit={{ opacity: 0, scale: 0.95, y: 20 }}
              transition={{ duration: 0.3, ease: [0.16, 1, 0.3, 1] }}
              className="glass-modal rounded-2xl max-w-md w-full p-6 shadow-glass-lg"
            >
              <div className="flex justify-between items-center mb-5">
                <h3 className="text-lg font-display font-bold text-white">Complete Purchase</h3>
                <button onClick={() => setShowPurchaseModal(false)} className="text-white/30 hover:text-white transition-colors">
                  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>
                </button>
              </div>

              <div className="mb-5">
                <p className="text-sm text-white/40 mb-1">{design.title}</p>
                <p className="text-2xl font-bold text-gradient-golden">{formatCurrency(design.price, currency, rates)}</p>
                <p className="text-[10px] text-white/20 mt-1">Billing currency is KES.</p>
              </div>

              <div className="mb-4">
                <label className="block text-xs font-medium text-white/40 mb-1.5">Payment Method</label>
                <div className="glass-input rounded-lg px-4 py-2.5 text-sm text-white/60">Paystack (Card / Mobile Money)</div>
                <p className="text-[10px] text-white/20 mt-1.5">Supports card and mobile money payments.</p>
              </div>

              <div className="mb-4">
                <label className="flex items-center gap-2 cursor-pointer">
                  <input type="checkbox" checked={includesConstruction} onChange={(e) => setIncludesConstruction(e.target.checked)}
                    className="h-4 w-4 rounded bg-white/5 border-white/20 text-brand-accent focus:ring-brand-accent/50" />
                  <span className="text-sm text-white/50">I need construction services (Kenya only)</span>
                </label>
              </div>

              <AnimatePresence>
                {includesConstruction && (
                  <motion.div initial={{ height: 0, opacity: 0 }} animate={{ height: 'auto', opacity: 1 }} exit={{ height: 0, opacity: 0 }} className="overflow-hidden mb-4">
                    <label className="block text-xs font-medium text-white/40 mb-1.5">Construction Location</label>
                    <input type="text" value={constructionLocation} onChange={(e) => setConstructionLocation(e.target.value)}
                      placeholder="e.g., Nairobi, Kenya" className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20" />
                    <p className="text-[10px] text-white/20 mt-1.5">We&apos;ll connect you with a trusted contractor in Kenya (2% commission applies)</p>
                  </motion.div>
                )}
              </AnimatePresence>

              <div className="flex gap-3 mt-6">
                <button onClick={() => setShowPurchaseModal(false)} disabled={isPurchasing}
                  className="flex-1 px-4 py-2.5 border border-white/10 rounded-lg text-sm text-white/50 hover:text-white hover:bg-white/5 transition-all disabled:opacity-50">
                  Cancel
                </button>
                <button onClick={handlePurchase} disabled={isPurchasing}
                  className="flex-1 px-4 py-2.5 bg-brand-accent text-white font-medium rounded-lg hover:bg-blue-500 transition-all duration-300 btn-glow disabled:opacity-50">
                  {isPurchasing ? (
                    <span className="inline-flex items-center gap-2">
                      <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" /><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" /></svg>
                      Processing...
                    </span>
                  ) : 'Confirm Purchase'}
                </button>
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Lightbox */}
      <AnimatePresence>
        {isLightboxOpen && mediaItems.length > 0 && (
          <motion.div
            initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
            className="fixed inset-0 z-50 flex items-center justify-center"
          >
            <div className="absolute inset-0 bg-black/95 backdrop-blur-sm" onClick={closeLightbox} role="presentation" />
            <button type="button" className="absolute top-6 right-6 z-10 text-white/60 hover:text-white transition-colors" onClick={closeLightbox}>
              <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M6 18L18 6M6 6l12 12" /></svg>
            </button>
            <button type="button" onClick={showPrevMedia} className="absolute left-6 z-10 text-white/40 hover:text-white text-5xl transition-colors" aria-label="Previous">&#8249;</button>

            <div className="relative z-10 max-w-5xl w-full px-8">
              <div className="rounded-xl overflow-hidden">
                {mediaItems[lightboxIndex].type === 'image' ? (
                  <img src={mediaItems[lightboxIndex].url} alt={design.title} className="w-full max-h-[75vh] object-contain bg-black" />
                ) : (
                  <video controls className="w-full max-h-[75vh] object-contain bg-black"><source src={mediaItems[lightboxIndex].url} type="video/mp4" /></video>
                )}
              </div>
              <div className="mt-4 flex items-center justify-center gap-3 overflow-x-auto pb-2">
                {mediaItems.map((item, index) => (
                  <button key={`lightbox-${item.type}-${index}`} onClick={() => setLightboxIndex(index)}
                    className={`relative w-20 h-16 rounded-lg overflow-hidden border-2 transition-all ${lightboxIndex === index ? 'border-brand-accent shadow-blue' : 'border-white/10'}`}>
                    {item.type === 'image' ? (
                      <img src={item.url} alt={`Preview ${index + 1}`} className="w-full h-full object-cover" />
                    ) : (
                      <div className="w-full h-full bg-brand-light flex items-center justify-center relative">
                        <video muted preload="metadata" className="w-full h-full object-cover"><source src={item.url} type="video/mp4" /></video>
                        <div className="absolute inset-0 flex items-center justify-center"><div className="glass-card rounded-full p-1"><svg className="w-3 h-3 text-white" fill="currentColor" viewBox="0 0 20 20"><path d="M6.5 5.5a1 1 0 011.5-.87l7 4.5a1 1 0 010 1.74l-7 4.5A1 1 0 016.5 14V5.5z" /></svg></div></div>
                      </div>
                    )}
                  </button>
                ))}
              </div>
            </div>

            <button type="button" onClick={showNextMedia} className="absolute right-6 z-10 text-white/40 hover:text-white text-5xl transition-colors" aria-label="Next">&#8250;</button>
          </motion.div>
        )}
      </AnimatePresence>
    </Layout>
  );
}
