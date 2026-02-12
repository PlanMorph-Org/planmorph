'use client';

import { useEffect, useMemo, useState } from 'react';
import toast, { Toaster } from 'react-hot-toast';
import api from '@/src/lib/api';
import EngineerLayout from '@/src/components/EngineerLayout';
import { useRoleGuard } from '@/src/hooks/useRoleGuard';

interface Verification {
  id: string;
  designId: string;
  designTitle: string;
  architectName: string;
  verificationType: string;
  status: string;
  comments?: string;
  createdAt: string;
}

interface VerificationFile {
  id: string;
  fileName: string;
  category: string;
  storageUrl: string;
  fileSizeBytes: number;
}

type FilterKey = 'All' | 'Structural' | 'BOQ';

export default function EngineerDashboard() {
  const { isAuthorized } = useRoleGuard({
    requiredRole: 'Engineer',
    redirectTo: '/engineer/login',
    message: 'Access denied. Engineer account required.',
  });
  const [isLoading, setIsLoading] = useState(true);
  const [verifications, setVerifications] = useState<Verification[]>([]);
  const [filter, setFilter] = useState<FilterKey>('All');

  const [showModal, setShowModal] = useState(false);
  const [selected, setSelected] = useState<Verification | null>(null);
  const [files, setFiles] = useState<VerificationFile[]>([]);
  const [isFilesLoading, setIsFilesLoading] = useState(false);
  const [comments, setComments] = useState('');
  const [isActionLoading, setIsActionLoading] = useState(false);

  useEffect(() => {
    if (isAuthorized) {
      loadVerifications();
    }
  }, [isAuthorized]);

  const loadVerifications = async () => {
    setIsLoading(true);
    try {
      const response = await api.get<Verification[]>('/design-verifications/pending');
      setVerifications(response.data || []);
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to load verifications.');
    } finally {
      setIsLoading(false);
    }
  };

  const filteredVerifications = useMemo(() => {
    if (filter === 'All') return verifications;
    if (filter === 'Structural') {
      return verifications.filter(v => v.verificationType === 'Structural');
    }
    return verifications.filter(v => v.verificationType === 'BOQ' || v.verificationType === 'BOQEngineer');
  }, [verifications, filter]);

  const totalPending = verifications.length;
  const structuralPending = verifications.filter(v => v.verificationType === 'Structural').length;
  const boqPending = verifications.filter(v => v.verificationType === 'BOQ' || v.verificationType === 'BOQEngineer').length;

  const openReview = async (verification: Verification) => {
    setSelected(verification);
    setComments('');
    setFiles([]);
    setShowModal(true);
    setIsFilesLoading(true);

    try {
      const response = await api.get<VerificationFile[]>(`/design-verifications/${verification.id}/files`);
      setFiles(response.data || []);
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to load files.');
    } finally {
      setIsFilesLoading(false);
    }
  };

  const closeReview = () => {
    setShowModal(false);
    setSelected(null);
    setFiles([]);
    setComments('');
    setIsActionLoading(false);
  };

  const handleVerify = async () => {
    if (!selected) return;
    setIsActionLoading(true);

    try {
      await api.put(`/design-verifications/${selected.id}/verify`, { comments });
      toast.success('Verification approved.');
      await loadVerifications();
      closeReview();
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to approve verification.');
    } finally {
      setIsActionLoading(false);
    }
  };

  const handleReject = async () => {
    if (!selected) return;
    setIsActionLoading(true);

    try {
      await api.put(`/design-verifications/${selected.id}/reject`, { comments });
      toast.success('Verification rejected.');
      await loadVerifications();
      closeReview();
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to reject verification.');
    } finally {
      setIsActionLoading(false);
    }
  };

  const formatFileSize = (bytes: number) => {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  };

  const getTypeLabel = (type: string) => {
    switch (type) {
      case 'Structural':
        return 'Structural';
      case 'BOQEngineer':
        return 'BOQ (Engineer)';
      case 'BOQ':
        return 'BOQ';
      default:
        return type;
    }
  };

  const getTypeBadge = (type: string) => {
    switch (type) {
      case 'Structural':
        return 'bg-sky-100 text-sky-800';
      case 'BOQEngineer':
      case 'BOQ':
        return 'bg-amber-100 text-amber-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'Verified':
        return 'bg-emerald-100 text-emerald-800';
      case 'Pending':
        return 'bg-amber-100 text-amber-800';
      case 'Rejected':
        return 'bg-rose-100 text-rose-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  return (
    <EngineerLayout>
      <Toaster position="top-right" />
      <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Verification Dashboard</h1>
          <p className="text-gray-600 mt-2">Review structural drawings and BOQ submissions.</p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
          <div className="bg-white border border-gray-200 rounded-lg p-4 shadow-sm">
            <p className="text-sm text-gray-500">Pending Verifications</p>
            <p className="text-2xl font-semibold text-gray-900">{totalPending}</p>
          </div>
          <div className="bg-white border border-gray-200 rounded-lg p-4 shadow-sm">
            <p className="text-sm text-gray-500">Structural</p>
            <p className="text-2xl font-semibold text-sky-700">{structuralPending}</p>
          </div>
          <div className="bg-white border border-gray-200 rounded-lg p-4 shadow-sm">
            <p className="text-sm text-gray-500">BOQ</p>
            <p className="text-2xl font-semibold text-amber-700">{boqPending}</p>
          </div>
        </div>

        <div className="flex flex-wrap gap-3 mb-6">
          {(['All', 'Structural', 'BOQ'] as FilterKey[]).map((key) => (
            <button
              key={key}
              onClick={() => setFilter(key)}
              className={`px-4 py-2 rounded-full text-sm font-medium border transition ${
                filter === key
                  ? 'bg-emerald-600 text-white border-emerald-600'
                  : 'bg-white text-gray-700 border-gray-200 hover:border-emerald-400'
              }`}
            >
              {key}
            </button>
          ))}
        </div>

        {isLoading ? (
          <div className="text-center py-12">
            <div className="animate-spin inline-block w-8 h-8 border-4 border-emerald-200 border-t-emerald-600 rounded-full" />
          </div>
        ) : filteredVerifications.length === 0 ? (
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 text-center text-gray-600">
            No pending verifications.
          </div>
        ) : (
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
            <table className="min-w-full text-sm">
              <thead className="bg-gray-50 text-gray-600">
                <tr>
                  <th className="px-6 py-3 text-left font-medium">Design</th>
                  <th className="px-6 py-3 text-left font-medium">Architect</th>
                  <th className="px-6 py-3 text-left font-medium">Type</th>
                  <th className="px-6 py-3 text-left font-medium">Status</th>
                  <th className="px-6 py-3 text-left font-medium">Submitted</th>
                  <th className="px-6 py-3 text-left font-medium">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200">
                {filteredVerifications.map((verification) => (
                  <tr key={verification.id}>
                    <td className="px-6 py-4 font-medium text-gray-900">
                      {verification.designTitle}
                    </td>
                    <td className="px-6 py-4 text-gray-600">{verification.architectName}</td>
                    <td className="px-6 py-4">
                      <span className={`px-3 py-1 rounded-full text-xs font-semibold ${getTypeBadge(verification.verificationType)}`}>
                        {getTypeLabel(verification.verificationType)}
                      </span>
                    </td>
                    <td className="px-6 py-4">
                      <span className={`px-3 py-1 rounded-full text-xs font-semibold ${getStatusBadge(verification.status)}`}>
                        {verification.status}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-gray-600">
                      {new Date(verification.createdAt).toLocaleDateString()}
                    </td>
                    <td className="px-6 py-4">
                      <button
                        onClick={() => openReview(verification)}
                        className="text-emerald-600 hover:text-emerald-800 font-medium"
                      >
                        Review
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {showModal && selected && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 px-4">
          <div className="bg-white rounded-lg shadow-xl max-w-4xl w-full max-h-[90vh] overflow-hidden">
            <div className="flex items-center justify-between px-6 py-4 border-b">
              <div>
                <h2 className="text-lg font-semibold text-gray-900">Review {getTypeLabel(selected.verificationType)}</h2>
                <p className="text-sm text-gray-500">{selected.designTitle}</p>
              </div>
              <button
                onClick={closeReview}
                className="text-gray-400 hover:text-gray-600"
              >
                X
              </button>
            </div>

            <div className="px-6 py-4 overflow-y-auto max-h-[60vh]">
              <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                <div className="lg:col-span-1">
                  <div className="bg-gray-50 rounded-lg p-4 text-sm">
                    <p className="text-gray-500">Architect</p>
                    <p className="font-medium text-gray-900">{selected.architectName}</p>
                    <p className="mt-3 text-gray-500">Submitted</p>
                    <p className="font-medium text-gray-900">
                      {new Date(selected.createdAt).toLocaleDateString()}
                    </p>
                  </div>

                  <div className="mt-6">
                    <label className="block text-sm font-medium text-gray-700 mb-2">Comments</label>
                    <textarea
                      value={comments}
                      onChange={(e) => setComments(e.target.value)}
                      rows={4}
                      className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-emerald-500 focus:border-emerald-500"
                      placeholder="Add any notes or feedback..."
                    />
                  </div>
                </div>

                <div className="lg:col-span-2">
                  <h3 className="text-sm font-semibold text-gray-700 mb-3">Files</h3>
                  {isFilesLoading ? (
                    <div className="text-center py-10">
                      <div className="animate-spin inline-block w-6 h-6 border-4 border-emerald-200 border-t-emerald-600 rounded-full" />
                    </div>
                  ) : files.length === 0 ? (
                    <div className="border border-dashed border-gray-300 rounded-lg p-6 text-center text-gray-500">
                      No files available for this verification.
                    </div>
                  ) : (
                    <div className="space-y-3">
                      {files.map((file) => (
                        <div key={file.id} className="flex items-center justify-between border border-gray-200 rounded-lg px-4 py-3">
                          <div>
                            <p className="text-sm font-medium text-gray-900">{file.fileName}</p>
                            <p className="text-xs text-gray-500">{file.category} • {formatFileSize(file.fileSizeBytes)}</p>
                          </div>
                          <a
                            href={file.storageUrl}
                            target="_blank"
                            rel="noreferrer"
                            className="text-emerald-600 hover:text-emerald-800 text-sm font-medium"
                          >
                            View
                          </a>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </div>
            </div>

            <div className="flex justify-end gap-3 px-6 py-4 border-t bg-gray-50">
              <button
                onClick={closeReview}
                className="px-4 py-2 text-sm font-medium text-gray-700 hover:text-gray-900"
              >
                Close
              </button>
              <button
                onClick={handleReject}
                disabled={isActionLoading}
                className="px-4 py-2 text-sm font-medium text-white bg-rose-600 rounded-md hover:bg-rose-700 disabled:opacity-50"
              >
                Reject
              </button>
              <button
                onClick={handleVerify}
                disabled={isActionLoading}
                className="px-4 py-2 text-sm font-medium text-white bg-emerald-600 rounded-md hover:bg-emerald-700 disabled:opacity-50"
              >
                Approve
              </button>
            </div>
          </div>
        </div>
      )}
    </EngineerLayout>
  );
}
