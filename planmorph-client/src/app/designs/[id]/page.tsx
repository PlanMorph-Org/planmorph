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

  useEffect(() => {
    if (params.id) {
      loadDesign(params.id as string);
    }
  }, [params.id]);

  useEffect(() => {
    setActiveMediaIndex(0);
  }, [design?.id]);

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
      if (event.key === 'Escape') {
        setIsLightboxOpen(false);
        return;
      }
      if (event.key === 'ArrowRight') {
        setLightboxIndex((prev) => (prev + 1) % mediaItems.length);
      }
      if (event.key === 'ArrowLeft') {
        setLightboxIndex((prev) => (prev - 1 + mediaItems.length) % mediaItems.length);
      }
    };

    document.body.style.overflow = 'hidden';
    window.addEventListener('keydown', handleKeyDown);
    return () => {
      document.body.style.overflow = '';
      window.removeEventListener('keydown', handleKeyDown);
    };
  }, [isLightboxOpen, mediaItems.length]);

  const openLightbox = (index: number) => {
    if (mediaItems.length === 0) return;
    setLightboxIndex(index);
    setIsLightboxOpen(true);
  };

  const closeLightbox = () => setIsLightboxOpen(false);

  const showNextMedia = () => {
    setLightboxIndex((prev) => (prev + 1) % mediaItems.length);
  };

  const showPrevMedia = () => {
    setLightboxIndex((prev) => (prev - 1 + mediaItems.length) % mediaItems.length);
  };

  const loadDesign = async (id: string) => {
    setIsLoading(true);
    try {
      const response = await api.get<Design>(`/designs/${id}`);
      setDesign(response.data);
    } catch (error) {
      toast.error('Failed to load design');
      router.push('/designs');
    } finally {
      setIsLoading(false);
    }
  };

  const handlePurchase = async () => {
    if (!isAuthenticated) {
      toast.error('Please login to purchase');
      router.push('/login');
      return;
    }
    if (user?.role !== 'Client') {
      logout();
      toast.error('Please use the client login to make purchases.');
      router.push('/login');
      return;
    }

    if (includesConstruction && !constructionLocation.trim()) {
      toast.error('Please enter construction location');
      return;
    }
    if (includesConstruction && constructionCountry !== 'Kenya') {
      toast.error('Construction services are currently available only in Kenya.');
      return;
    }

    setIsPurchasing(true);

    try {
      const orderData: CreateOrderDto = {
        designId: design!.id,
        paymentMethod,
        includesConstruction,
        constructionLocation: includesConstruction ? constructionLocation : undefined,
        constructionCountry: includesConstruction ? constructionCountry : undefined,
      };

      const response = await api.post('/orders', orderData);
      toast.success('Order created successfully!');
      
      // In a real app, you'd redirect to payment gateway here
      // For now, we'll just close the modal and show success
      setShowPurchaseModal(false);
      
      // Show payment instructions
      toast.success(`Order Number: ${response.data.orderNumber}. Please proceed with payment.`, {
        duration: 5000,
      });
      
      // Redirect to orders page after a delay
      setTimeout(() => {
        router.push('/my-orders');
      }, 2000);
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Purchase failed. Please try again.');
    } finally {
      setIsPurchasing(false);
    }
  };

  if (isLoading) {
    return (
      <Layout>
        <div className="flex justify-center items-center h-screen">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        </div>
      </Layout>
    );
  }

  if (!design) {
    return (
      <Layout>
        <div className="max-w-7xl mx-auto px-4 py-12">
          <p className="text-center text-gray-500">Design not found</p>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <Toaster position="top-right" />
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Breadcrumb */}
        <nav className="mb-8">
          <ol className="flex items-center space-x-2 text-sm text-gray-500">
            <li>
              <a href="/designs" className="hover:text-blue-600">Designs</a>
            </li>
            <li>/</li>
            <li className="text-gray-900">{design.title}</li>
          </ol>
        </nav>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-10">
          {/* Left Column - Media Gallery */}
          <div>
            {(() => {
              const activeItem = mediaItems[activeMediaIndex];

              return (
                <>
                  <button
                    type="button"
                    onClick={() => openLightbox(activeMediaIndex)}
                    className="relative w-full bg-gray-900 rounded-2xl overflow-hidden aspect-[4/3] shadow-sm"
                  >
                    {activeItem ? (
                      activeItem.type === 'image' ? (
                        <img
                          src={activeItem.url}
                          alt={design.title}
                          className="w-full h-full object-cover"
                        />
                      ) : (
                        <video controls className="w-full h-full object-cover">
                          <source src={activeItem.url} type="video/mp4" />
                          Your browser does not support the video tag.
                        </video>
                      )
                    ) : (
                      <div className="w-full h-full flex items-center justify-center text-gray-400">
                        No Preview Available
                      </div>
                    )}
                    <div className="absolute top-3 left-3 bg-white/90 text-gray-900 text-xs font-semibold px-3 py-1 rounded-full">
                      Gallery • {mediaItems.length}
                    </div>
                    {activeItem && (
                      <div className="absolute bottom-3 right-3 bg-black/60 text-white text-xs px-2 py-1 rounded">
                        Click to view
                      </div>
                    )}
                  </button>

                  {mediaItems.length > 1 && (
                    <div className="mt-4 flex gap-3 overflow-x-auto pb-2">
                      {mediaItems.map((item, index) => (
                        <button
                          key={`${item.type}-${index}`}
                          onClick={() => setActiveMediaIndex(index)}
                          className={`relative flex-shrink-0 w-24 h-20 rounded-lg overflow-hidden border-2 ${
                            activeMediaIndex === index ? 'border-blue-600' : 'border-gray-200'
                          }`}
                        >
                          {item.type === 'image' ? (
                            <img src={item.url} alt={`Preview ${index + 1}`} className="w-full h-full object-cover" />
                          ) : (
                            <div className="w-full h-full bg-gray-900 flex items-center justify-center">
                              <video muted preload="metadata" className="w-full h-full object-cover">
                                <source src={item.url} type="video/mp4" />
                              </video>
                              <div className="absolute inset-0 flex items-center justify-center">
                                <div className="bg-white/80 rounded-full p-2">
                                  <svg className="w-4 h-4 text-gray-900" fill="currentColor" viewBox="0 0 20 20">
                                    <path d="M6.5 5.5a1 1 0 011.5-.87l7 4.5a1 1 0 010 1.74l-7 4.5A1 1 0 016.5 14V5.5z" />
                                  </svg>
                                </div>
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
          </div>

          {/* Right Column - Details */}
          <div>
            <div className="mb-6">
              <span className="inline-block px-3 py-1 text-sm font-semibold text-blue-600 bg-blue-100 rounded-full mb-2">
                {design.category}
              </span>
              <h1 className="text-3xl font-bold text-gray-900 mb-4">{design.title}</h1>
              <p className="text-gray-600 text-lg mb-6">{design.description}</p>
            </div>

            {/* Specifications */}
            <div className="bg-gray-50 rounded-lg p-6 mb-6">
              <h3 className="text-lg font-semibold mb-4">Specifications</h3>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-sm text-gray-500">Bedrooms</p>
                  <p className="text-lg font-semibold">{design.bedrooms}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-500">Bathrooms</p>
                  <p className="text-lg font-semibold">{design.bathrooms}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-500">Square Footage</p>
                  <p className="text-lg font-semibold">{design.squareFootage.toLocaleString()} sqft</p>
                </div>
                <div>
                  <p className="text-sm text-gray-500">Stories</p>
                  <p className="text-lg font-semibold">{design.stories}</p>
                </div>
              </div>
            </div>

            {/* Pricing */}
            <div className="border-t border-b border-gray-200 py-6 mb-6">
              <div className="flex justify-between items-center mb-4">
                <span className="text-lg text-gray-600">Design Plans</span>
                <span className="text-3xl font-bold text-blue-600">
                  {formatCurrency(design.price, currency, rates)}
                </span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-sm text-gray-600">Estimated Construction Cost</span>
                <span className="text-lg font-semibold text-gray-900">
                  {formatCurrency(design.estimatedConstructionCost, currency, rates)}
                </span>
              </div>
              <p className="text-xs text-gray-500 mt-3">
                Prices shown in {currency}. Billing currency is KES.
              </p>
            </div>

            {/* What's Included */}
            <div className="mb-6">
              <h3 className="text-lg font-semibold mb-3">What's Included:</h3>
              <ul className="space-y-2">
                <li className="flex items-start">
                  <svg className="w-5 h-5 text-green-500 mt-0.5 mr-2" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                  </svg>
                  <span className="text-gray-700">Complete architectural drawings</span>
                </li>
                <li className="flex items-start">
                  <svg className="w-5 h-5 text-green-500 mt-0.5 mr-2" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                  </svg>
                  <span className="text-gray-700">Structural/Engineering drawings</span>
                </li>
                <li className="flex items-start">
                  <svg className="w-5 h-5 text-green-500 mt-0.5 mr-2" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                  </svg>
                  <span className="text-gray-700">Bill of Quantities (BOQ)</span>
                </li>
                <li className="flex items-start">
                  <svg className="w-5 h-5 text-green-500 mt-0.5 mr-2" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                  </svg>
                  <span className="text-gray-700">High-resolution 3D renders</span>
                </li>
                <li className="flex items-start">
                  <svg className="w-5 h-5 text-green-500 mt-0.5 mr-2" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                  </svg>
                  <span className="text-gray-700">Modification support</span>
                </li>
              </ul>
            </div>

            {/* Purchase Button */}
            <button
              onClick={() => setShowPurchaseModal(true)}
              className="w-full bg-blue-600 text-white py-3 px-6 rounded-lg font-semibold text-lg hover:bg-blue-700 transition"
            >
              Purchase Now
            </button>
          </div>
        </div>
      </div>

      {/* Purchase Modal */}
      {showPurchaseModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg max-w-md w-full p-6">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-xl font-bold">Complete Purchase</h3>
              <button
                onClick={() => setShowPurchaseModal(false)}
                className="text-gray-400 hover:text-gray-600"
              >
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>

            <div className="mb-4">
              <p className="text-gray-600 mb-2">Design: {design.title}</p>
              <p className="text-2xl font-bold text-blue-600">
                {formatCurrency(design.price, currency, rates)}
              </p>
              <p className="text-xs text-gray-500 mt-1">Billing currency is KES.</p>
            </div>

            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Payment Method
              </label>
              <div className="w-full px-3 py-2 border border-gray-300 rounded-md bg-gray-50 text-gray-800">
                Paystack (Card / Mobile Money)
              </div>
              <p className="text-xs text-gray-500 mt-2">
                Paystack supports card and mobile money payments.
              </p>
            </div>

            <div className="mb-4">
              <label className="flex items-center">
                <input
                  type="checkbox"
                  checked={includesConstruction}
                  onChange={(e) => setIncludesConstruction(e.target.checked)}
                  className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                />
                <span className="ml-2 text-sm text-gray-700">
                  I need construction services (Kenya only)
                </span>
              </label>
              <p className="text-xs text-gray-500 mt-2">
                Construction services are currently available only in Kenya.
              </p>
            </div>

            {includesConstruction && (
              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Construction Location
                </label>
                <input
                  type="text"
                  value={constructionLocation}
                  onChange={(e) => setConstructionLocation(e.target.value)}
                  placeholder="e.g., Nairobi, Kenya"
                  className="w-full px-3 py-2 border border-gray-300 rounded-md"
                />
                <p className="text-xs text-gray-500 mt-1">
                  We'll connect you with a trusted contractor in Kenya (2% commission applies)
                </p>
              </div>
            )}

            <div className="flex space-x-3">
              <button
                onClick={() => setShowPurchaseModal(false)}
                className="flex-1 px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50"
                disabled={isPurchasing}
              >
                Cancel
              </button>
              <button
                onClick={handlePurchase}
                disabled={isPurchasing}
                className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:bg-gray-400"
              >
                {isPurchasing ? 'Processing...' : 'Confirm Purchase'}
              </button>
            </div>
          </div>
        </div>
      )}

      {isLightboxOpen && mediaItems.length > 0 && (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
          <div
            className="absolute inset-0 bg-black/90"
            onClick={closeLightbox}
            role="presentation"
          />

          <button
            type="button"
            className="absolute top-6 right-6 z-10 text-white/80 hover:text-white"
            onClick={closeLightbox}
          >
            <span className="text-3xl">&times;</span>
          </button>

          <button
            type="button"
            onClick={showPrevMedia}
            className="absolute left-6 z-10 text-white/80 hover:text-white text-4xl"
            aria-label="Previous"
          >
            &#8249;
          </button>

          <div className="relative z-10 max-w-5xl w-full px-8">
            <div className="bg-black rounded-xl overflow-hidden">
              {mediaItems[lightboxIndex].type === 'image' ? (
                <img
                  src={mediaItems[lightboxIndex].url}
                  alt={design.title}
                  className="w-full max-h-[75vh] object-contain bg-black"
                />
              ) : (
                <video controls className="w-full max-h-[75vh] object-contain bg-black">
                  <source src={mediaItems[lightboxIndex].url} type="video/mp4" />
                  Your browser does not support the video tag.
                </video>
              )}
            </div>

            <div className="mt-4 flex items-center justify-center gap-3 overflow-x-auto pb-2">
              {mediaItems.map((item, index) => (
                <button
                  key={`lightbox-${item.type}-${index}`}
                  onClick={() => setLightboxIndex(index)}
                  className={`relative w-20 h-16 rounded-lg overflow-hidden border-2 ${
                    lightboxIndex === index ? 'border-blue-500' : 'border-transparent'
                  }`}
                >
                  {item.type === 'image' ? (
                    <img src={item.url} alt={`Preview ${index + 1}`} className="w-full h-full object-cover" />
                  ) : (
                    <div className="w-full h-full bg-gray-900 flex items-center justify-center">
                      <video muted preload="metadata" className="w-full h-full object-cover">
                        <source src={item.url} type="video/mp4" />
                      </video>
                      <div className="absolute inset-0 flex items-center justify-center">
                        <div className="bg-white/80 rounded-full p-1">
                          <svg className="w-3 h-3 text-gray-900" fill="currentColor" viewBox="0 0 20 20">
                            <path d="M6.5 5.5a1 1 0 011.5-.87l7 4.5a1 1 0 010 1.74l-7 4.5A1 1 0 016.5 14V5.5z" />
                          </svg>
                        </div>
                      </div>
                    </div>
                  )}
                </button>
              ))}
            </div>
          </div>

          <button
            type="button"
            onClick={showNextMedia}
            className="absolute right-6 z-10 text-white/80 hover:text-white text-4xl"
            aria-label="Next"
          >
            &#8250;
          </button>
        </div>
      )}
    </Layout>
  );
}
