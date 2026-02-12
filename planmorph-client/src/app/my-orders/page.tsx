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

export default function MyOrdersPage() {
  const router = useRouter();
  const { isAuthorized, isChecking } = useRoleGuard({
    requiredRole: 'Client',
    redirectTo: '/login',
    message: 'Please use the appropriate professional login portal.',
  });
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

  useEffect(() => {
    if (isAuthorized) {
      loadOrders();
    }
  }, [isAuthorized]);

  useEffect(() => {
    if (selectedOrder) {
      setOrderFiles([]);
      setIsLoadingFiles(false);
      setShowFileViewer(false);
      setViewerUrl('');
      setViewerFileName('');
      setViewerFileType('');
      setPdfPage(1);
      setPdfZoom(120);
      setPdfRotation(0);
    }
  }, [selectedOrder?.id]);

  const loadOrders = async () => {
    setIsLoading(true);
    try {
      const response = await api.get<Order[]>('/orders/my-orders');
      setOrders(response.data);
    } catch (error) {
      toast.error('Failed to load orders');
    } finally {
      setIsLoading(false);
    }
  };

  const openConstructionRequest = () => {
    setConstructionLocation('');
    setShowConstructionRequest(true);
  };

  const initializePaystack = async () => {
    if (!selectedOrder) return;
    setIsInitializingPaystack(true);
    try {
      const response = await api.post('/payments/paystack/initialize', {
        orderId: selectedOrder.id,
      });
      const authorizationUrl = response.data?.authorizationUrl as string | undefined;
      if (!authorizationUrl) {
        toast.error('Unable to start Paystack payment.');
        return;
      }
      window.location.href = authorizationUrl;
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to start Paystack payment.');
    } finally {
      setIsInitializingPaystack(false);
    }
  };

  const loadOrderFiles = async () => {
    if (!selectedOrder) return;
    setIsLoadingFiles(true);
    try {
      const response = await api.get<OrderFile[]>(`/orders/${selectedOrder.id}/files`);
      setOrderFiles(response.data);
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to load files.');
    } finally {
      setIsLoadingFiles(false);
    }
  };

  const fetchFileUrl = async (fileId: string) => {
    const response = await api.get<{ url: string }>(`/designs/files/${fileId}/download`);
    return response.data.url;
  };

  const downloadFile = async (fileId: string) => {
    try {
      const url = await fetchFileUrl(fileId);
      window.open(url, '_blank');
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to download file.');
    }
  };

  const openFileViewer = async (file: OrderFile) => {
    setIsLoadingViewer(true);
    try {
      const url = await fetchFileUrl(file.id);
      setViewerUrl(url);
      setViewerFileName(file.fileName);
      setViewerFileType(file.fileType);
      setPdfPage(1);
      setPdfZoom(120);
      setPdfRotation(0);
      setShowFileViewer(true);
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to open file.');
    } finally {
      setIsLoadingViewer(false);
    }
  };

  const formatFileSize = (bytes: number) => {
    if (bytes < 1024) return `${bytes} B`;
    const kb = bytes / 1024;
    if (kb < 1024) return `${kb.toFixed(1)} KB`;
    const mb = kb / 1024;
    return `${mb.toFixed(1)} MB`;
  };

  const canInlineView = (fileType: string) => ['Image', 'Video', 'PDF'].includes(fileType);

  const groupFilesByCategory = (files: OrderFile[]) => {
    const groups: Record<string, OrderFile[]> = {};

    const getGroupLabel = (category: string) => {
      switch (category) {
        case 'ArchitecturalDrawing':
          return 'Architectural Drawings';
        case 'StructuralDrawing':
          return 'Structural Drawings';
        case 'BOQ':
          return 'Bill of Quantities (BOQ)';
        case 'FullRenderImage':
          return 'Render Images';
        case 'FullRenderVideo':
          return 'Render Videos';
        default:
          return 'Other Files';
      }
    };

    files.forEach((file) => {
      const label = getGroupLabel(file.category);
      if (!groups[label]) {
        groups[label] = [];
      }
      groups[label].push(file);
    });

    return groups;
  };

  const handleRequestConstruction = async () => {
    if (!selectedOrder) return;
    if (!constructionLocation.trim()) {
      toast.error('Please enter a construction location in Kenya.');
      return;
    }

    setIsRequestingConstruction(true);
    try {
      await api.post(`/orders/${selectedOrder.id}/request-construction`, {
        location: constructionLocation,
        country: 'Kenya',
      });

      toast.success('Construction request submitted.');
      setShowConstructionRequest(false);
      setSelectedOrder(null);
      await loadOrders();
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to submit construction request.');
    } finally {
      setIsRequestingConstruction(false);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Paid':
        return 'bg-green-100 text-green-800';
      case 'Pending':
        return 'bg-yellow-100 text-yellow-800';
      case 'Completed':
        return 'bg-blue-100 text-blue-800';
      case 'Cancelled':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  if (isChecking || isLoading) {
    return (
      <Layout>
        <div className="flex justify-center items-center h-screen">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <Toaster position="top-right" />
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">My Orders</h1>
        <p className="text-sm text-gray-500 mb-8">
          Amounts shown in {currency}. Billing currency is KES.
        </p>

        {orders.length === 0 ? (
          <div className="text-center py-12">
            <svg className="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
            <h3 className="mt-2 text-sm font-medium text-gray-900">No orders yet</h3>
            <p className="mt-1 text-sm text-gray-500">Get started by browsing our designs.</p>
            <div className="mt-6">
              <button
                onClick={() => router.push('/designs')}
                className="inline-flex items-center px-4 py-2 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
              >
                Browse Designs
              </button>
            </div>
          </div>
        ) : (
          <div className="bg-white shadow overflow-hidden sm:rounded-md">
            <ul className="divide-y divide-gray-200">
              {orders.map((order) => (
                <li key={order.id}>
                  <div className="px-4 py-4 sm:px-6 hover:bg-gray-50">
                    <div className="flex items-center justify-between">
                      <div className="flex-1">
                        <div className="flex items-center justify-between">
                          <p className="text-sm font-medium text-blue-600 truncate">
                            {order.orderNumber}
                          </p>
                          <div className="ml-2 flex-shrink-0 flex">
                            <p className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${getStatusColor(order.status)}`}>
                              {order.status}
                            </p>
                          </div>
                        </div>
                        <div className="mt-2 sm:flex sm:justify-between">
                          <div className="sm:flex">
                            <p className="flex items-center text-sm text-gray-500">
                              {order.designTitle}
                            </p>
                          </div>
                          <div className="mt-2 flex items-center text-sm text-gray-500 sm:mt-0">
                            <p>
                              Ordered on {new Date(order.createdAt).toLocaleDateString()}
                            </p>
                          </div>
                        </div>
                        <div className="mt-2 flex items-center justify-between">
                          <p className="text-lg font-semibold text-gray-900">
                            {formatCurrency(order.amount, currency, rates)}
                          </p>
                          <button
                            onClick={() => setSelectedOrder(order)}
                            className="text-sm text-blue-600 hover:text-blue-800 font-medium"
                          >
                            View Details
                          </button>
                        </div>
                        {order.includesConstruction && (
                          <div className="mt-2">
                            <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-purple-100 text-purple-800">
                              <svg className="mr-1.5 h-2 w-2" fill="currentColor" viewBox="0 0 8 8">
                                <circle cx={4} cy={4} r={3} />
                              </svg>
                              Construction Services Included
                            </span>
                          </div>
                        )}
                      </div>
                    </div>
                  </div>
                </li>
              ))}
            </ul>
          </div>
        )}
      </div>

      {/* Order Details Modal */}
      {selectedOrder && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg max-w-2xl w-full p-6 max-h-[90vh] overflow-y-auto">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-xl font-bold">Order Details</h3>
              <button
                onClick={() => {
                  setShowConstructionRequest(false);
                  setSelectedOrder(null);
                }}
                className="text-gray-400 hover:text-gray-600"
              >
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>

            <div className="space-y-4">
              <div>
                <label className="text-sm font-medium text-gray-500">Order Number</label>
                <p className="text-lg font-semibold">{selectedOrder.orderNumber}</p>
              </div>

              <div>
                <label className="text-sm font-medium text-gray-500">Design</label>
                <p className="text-lg">{selectedOrder.designTitle}</p>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-medium text-gray-500">Amount</label>
                  <p className="text-lg font-semibold">
                    {formatCurrency(selectedOrder.amount, currency, rates)}
                  </p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-500">Status</label>
                  <p className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${getStatusColor(selectedOrder.status)}`}>
                    {selectedOrder.status}
                  </p>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-medium text-gray-500">Payment Method</label>
                  <p className="text-lg">{selectedOrder.paymentMethod}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-500">Order Date</label>
                  <p className="text-lg">{new Date(selectedOrder.createdAt).toLocaleDateString()}</p>
                </div>
              </div>

              {selectedOrder.paidAt && (
                <div>
                  <label className="text-sm font-medium text-gray-500">Paid At</label>
                  <p className="text-lg">{new Date(selectedOrder.paidAt).toLocaleString()}</p>
                </div>
              )}

              {selectedOrder.includesConstruction && selectedOrder.constructionContract && (
                <div className="border-t pt-4">
                  <h4 className="font-semibold mb-3">Construction Contract</h4>
                  <div className="space-y-2">
                    <div>
                      <label className="text-sm font-medium text-gray-500">Location</label>
                      <p>{selectedOrder.constructionContract.location}</p>
                    </div>
                    <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="text-sm font-medium text-gray-500">Estimated Cost</label>
                      <p className="font-semibold">
                        {formatCurrency(selectedOrder.constructionContract.estimatedCost, currency, rates)}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">Commission</label>
                      <p className="font-semibold">
                        {formatCurrency(selectedOrder.constructionContract.commissionAmount, currency, rates)}
                      </p>
                    </div>
                  </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">Status</label>
                      <p className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${getStatusColor(selectedOrder.constructionContract.status)}`}>
                        {selectedOrder.constructionContract.status}
                      </p>
                    </div>
                    {selectedOrder.constructionContract.contractorName && (
                      <div>
                        <label className="text-sm font-medium text-gray-500">Contractor</label>
                        <p>{selectedOrder.constructionContract.contractorName}</p>
                      </div>
                    )}
                  </div>
                </div>
              )}

              {(selectedOrder.status === 'Paid' || selectedOrder.status === 'Completed') && (
                <div className="bg-green-50 border border-green-200 rounded-lg p-4">
                  <p className="text-sm text-green-800">
                    ✓ Your order has been confirmed. You can now download your design files.
                  </p>
                  <button
                    className="mt-2 text-sm font-medium text-green-700 hover:text-green-900"
                    onClick={loadOrderFiles}
                    disabled={isLoadingFiles}
                  >
                    {isLoadingFiles ? 'Loading files...' : 'Load Files →'}
                  </button>
                </div>
              )}

              {(selectedOrder.status === 'Paid' || selectedOrder.status === 'Completed') && orderFiles.length > 0 && (
                <div className="space-y-4">
                  {Object.entries(groupFilesByCategory(orderFiles)).map(([category, files]) => (
                    <div key={category} className="border rounded-lg">
                      <div className="flex items-center justify-between px-4 py-3 bg-gray-50">
                        <div>
                          <p className="font-semibold text-gray-900">{category}</p>
                          <p className="text-xs text-gray-500">{files.length} file(s)</p>
                        </div>
                        <button
                          className="text-xs text-blue-600 hover:text-blue-800"
                          onClick={() => files.forEach((file) => downloadFile(file.id))}
                        >
                          Download All
                        </button>
                      </div>
                      <div className="divide-y">
                        {files.map((file) => (
                          <div key={file.id} className="flex items-center justify-between px-4 py-3">
                            <div>
                              <p className="text-sm font-medium text-gray-900">{file.fileName}</p>
                              <p className="text-xs text-gray-500">
                                {file.fileType} • {formatFileSize(file.fileSizeBytes)}
                              </p>
                            </div>
                            <div className="flex items-center gap-3">
                              <button
                                onClick={() => openFileViewer(file)}
                                className="text-sm font-medium text-gray-700 hover:text-gray-900"
                                disabled={isLoadingViewer || !canInlineView(file.fileType)}
                              >
                                {canInlineView(file.fileType) ? 'View' : 'Preview N/A'}
                              </button>
                              <button
                                onClick={() => downloadFile(file.id)}
                                className="text-sm font-medium text-blue-600 hover:text-blue-800"
                              >
                                Download
                              </button>
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>
                  ))}
                </div>
              )}

              {(selectedOrder.status === 'Paid' || selectedOrder.status === 'Completed') && !selectedOrder.includesConstruction && (
                <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                  <p className="text-sm text-blue-800">
                    Need construction services? Available in Kenya only.
                  </p>
                  <button
                    onClick={openConstructionRequest}
                    className="mt-2 text-sm font-medium text-blue-700 hover:text-blue-900"
                  >
                    Request Construction →
                  </button>
                </div>
              )}

              {selectedOrder.status === 'Pending' && (
                <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
                  <p className="text-sm text-yellow-800">
                    ⏳ Awaiting payment confirmation. Please complete your payment to access the design files.
                  </p>
                  <button
                    className="mt-2 text-sm font-medium text-yellow-800 hover:text-yellow-900 disabled:text-gray-400"
                    onClick={initializePaystack}
                    disabled={isInitializingPaystack}
                  >
                    {isInitializingPaystack ? 'Redirecting to Paystack...' : 'Pay with Paystack →'}
                  </button>
                </div>
              )}
            </div>

            <div className="mt-6">
              <button
                onClick={() => {
                  setShowConstructionRequest(false);
                  setSelectedOrder(null);
                }}
                className="w-full px-4 py-2 bg-gray-100 text-gray-700 rounded-md hover:bg-gray-200"
              >
                Close
              </button>
            </div>
          </div>
        </div>
      )}

      {showConstructionRequest && selectedOrder && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg max-w-md w-full p-6">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-xl font-bold">Request Construction (Kenya Only)</h3>
              <button
                onClick={() => setShowConstructionRequest(false)}
                className="text-gray-400 hover:text-gray-600"
              >
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>

            <div className="mb-4">
              <p className="text-sm text-gray-600">Order: {selectedOrder.orderNumber}</p>
              <p className="text-sm text-gray-600">Design: {selectedOrder.designTitle}</p>
            </div>

            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Construction Location (Kenya)
              </label>
              <input
                type="text"
                value={constructionLocation}
                onChange={(e) => setConstructionLocation(e.target.value)}
                placeholder="e.g., Nairobi, Kenya"
                className="w-full px-3 py-2 border border-gray-300 rounded-md"
              />
              <p className="text-xs text-gray-500 mt-2">
                We will connect you with a verified contractor. A 2% commission applies.
              </p>
            </div>

            <div className="flex space-x-3">
              <button
                onClick={() => setShowConstructionRequest(false)}
                className="flex-1 px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50"
                disabled={isRequestingConstruction}
              >
                Cancel
              </button>
              <button
                onClick={handleRequestConstruction}
                disabled={isRequestingConstruction}
                className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:bg-gray-400"
              >
                {isRequestingConstruction ? 'Submitting...' : 'Submit Request'}
              </button>
            </div>
          </div>
        </div>
      )}

      {showFileViewer && (
        <div className="fixed inset-0 bg-black bg-opacity-60 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg w-full h-full max-w-6xl max-h-[95vh] overflow-hidden flex flex-col">
            <div className="flex flex-wrap items-center justify-between gap-3 px-4 py-3 border-b">
              <div>
                <h3 className="text-lg font-bold">File Viewer</h3>
                <p className="text-sm text-gray-500">{viewerFileName}</p>
              </div>

              {viewerFileType === 'PDF' && (
                <div className="flex flex-wrap items-center gap-2">
                  <button
                    className="px-2 py-1 text-sm border rounded"
                    onClick={() => setPdfPage((prev) => Math.max(1, prev - 1))}
                  >
                    Prev
                  </button>
                  <div className="flex items-center gap-1">
                    <span className="text-xs text-gray-600">Page</span>
                    <input
                      type="number"
                      min={1}
                      value={pdfPage}
                      onChange={(e) => setPdfPage(Math.max(1, Number(e.target.value)))}
                      className="w-16 border rounded px-2 py-1 text-sm"
                    />
                  </div>
                  <button
                    className="px-2 py-1 text-sm border rounded"
                    onClick={() => setPdfPage((prev) => prev + 1)}
                  >
                    Next
                  </button>
                  <button
                    className="px-2 py-1 text-sm border rounded"
                    onClick={() => setPdfZoom((prev) => Math.min(300, prev + 10))}
                  >
                    Zoom +
                  </button>
                  <button
                    className="px-2 py-1 text-sm border rounded"
                    onClick={() => setPdfZoom((prev) => Math.max(50, prev - 10))}
                  >
                    Zoom -
                  </button>
                  <button
                    className="px-2 py-1 text-sm border rounded"
                    onClick={() => setPdfRotation((prev) => (prev + 90) % 360)}
                  >
                    Rotate
                  </button>
                  <span className="text-xs text-gray-500">{pdfZoom}%</span>
                </div>
              )}

              <button
                onClick={() => setShowFileViewer(false)}
                className="text-gray-400 hover:text-gray-600"
              >
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>

            <div className="flex-1 bg-gray-900 overflow-auto">
              {viewerFileType === 'PDF' && viewerUrl && (
                <div className="w-full h-full flex items-center justify-center">
                  <div
                    className="w-full h-full flex items-center justify-center"
                    style={{ transform: `rotate(${pdfRotation}deg)` }}
                  >
                    <iframe
                      key={`${viewerUrl}-${pdfPage}-${pdfZoom}`}
                      src={`${viewerUrl.split('#')[0]}#page=${pdfPage}&zoom=${pdfZoom}`}
                      className="w-full h-full"
                      title={viewerFileName}
                    />
                  </div>
                </div>
              )}

              {viewerFileType === 'Image' && (
                <div className="w-full h-full flex items-center justify-center">
                  <img src={viewerUrl} alt={viewerFileName} className="max-h-[85vh] object-contain" />
                </div>
              )}

              {viewerFileType === 'Video' && (
                <div className="w-full h-full flex items-center justify-center">
                  <video controls className="w-full max-h-[85vh]">
                    <source src={viewerUrl} />
                    Your browser does not support the video tag.
                  </video>
                </div>
              )}

              {viewerFileType === 'CAD' && (
                <div className="bg-gray-50 border border-gray-200 rounded p-4 text-sm text-gray-600 m-6">
                  This file type cannot be previewed in the browser. Please download to view it.
                </div>
              )}
            </div>

            <div className="px-4 py-3 border-t flex justify-between items-center">
              <div className="text-xs text-gray-500">
                Viewer mode: {viewerFileType}
              </div>
              <button
                onClick={() => viewerUrl && window.open(viewerUrl, '_blank')}
                className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
              >
                Download
              </button>
            </div>
          </div>
        </div>
      )}
    </Layout>
  );
}
