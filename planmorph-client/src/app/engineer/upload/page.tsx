'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import EngineerLayout from '@/src/components/EngineerLayout';
import { useRoleGuard } from '@/src/hooks/useRoleGuard';
import api from '@/src/lib/api';
import toast, { Toaster } from 'react-hot-toast';

export default function EngineerUploadDesignPage() {
  const router = useRouter();
  const { isAuthorized, isChecking } = useRoleGuard({
    requiredRole: 'Engineer',
    redirectTo: '/engineer/login',
    message: 'Access denied. Engineer account required.',
  });

  const [isUploading, setIsUploading] = useState(false);
  const [designData, setDesignData] = useState({
    title: '',
    description: '',
    price: '',
    bedrooms: '',
    bathrooms: '',
    squareFootage: '',
    stories: '',
    category: 'Bungalow',
    estimatedConstructionCost: '',
  });

  const [files, setFiles] = useState({
    previewImages: [] as File[],
    previewVideos: [] as File[],
    architecturalDrawings: [] as File[],
    structuralDrawings: [] as File[],
    boq: null as File | null,
  });

  const handleInputChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    setDesignData({ ...designData, [e.target.name]: e.target.value });
  };

  const handleFileChange = (
    e: React.ChangeEvent<HTMLInputElement>,
    fileType: keyof typeof files
  ) => {
    const selectedFiles = e.target.files;
    if (!selectedFiles) return;

    if (fileType === 'boq') {
      setFiles({ ...files, boq: selectedFiles[0] });
      return;
    }

    setFiles({ ...files, [fileType]: Array.from(selectedFiles) });
  };

  const handleSubmit = async () => {
    if (!designData.title || !designData.description || !designData.price) {
      toast.error('Please fill in all required fields.');
      return;
    }

    if (files.previewImages.length === 0) {
      toast.error('Please upload at least one preview image.');
      return;
    }

    if (
      files.architecturalDrawings.length === 0 ||
      files.structuralDrawings.length === 0 ||
      !files.boq
    ) {
      toast.error('Please upload architectural, structural, and BOQ files.');
      return;
    }

    setIsUploading(true);
    try {
      const createResponse = await api.post('/designs', {
        title: designData.title,
        description: designData.description,
        price: parseFloat(designData.price),
        bedrooms: parseInt(designData.bedrooms || '0', 10),
        bathrooms: parseInt(designData.bathrooms || '0', 10),
        squareFootage: parseFloat(designData.squareFootage || '0'),
        stories: parseInt(designData.stories || '0', 10),
        category: designData.category,
        estimatedConstructionCost: parseFloat(designData.estimatedConstructionCost || '0'),
      });

      const designId = createResponse.data.id as string;
      const uploadFiles = (fileList: File[], category: string) => {
        const fd = new FormData();
        fileList.forEach((file) => fd.append('files', file));
        return api.post(`/designs/${designId}/files?category=${category}`, fd, {
          headers: { 'Content-Type': 'multipart/form-data' },
        });
      };

      const uploadRequests: Promise<unknown>[] = [];
      uploadRequests.push(uploadFiles(files.previewImages, 'PreviewImage'));

      if (files.previewVideos.length > 0) uploadRequests.push(uploadFiles(files.previewVideos, 'PreviewVideo'));
      uploadRequests.push(uploadFiles(files.architecturalDrawings, 'ArchitecturalDrawing'));
      uploadRequests.push(uploadFiles(files.structuralDrawings, 'StructuralDrawing'));

      const boqForm = new FormData();
      boqForm.append('files', files.boq);
      uploadRequests.push(
        api.post(`/designs/${designId}/files?category=BOQ`, boqForm, {
          headers: { 'Content-Type': 'multipart/form-data' },
        })
      );

      await Promise.all(uploadRequests);
      toast.success('Design submitted. Architectural plans must now be verified by an approved architect.');
      router.push('/engineer/dashboard');
    } catch {
      toast.error('Failed to upload design.');
    } finally {
      setIsUploading(false);
    }
  };

  if (isChecking || !isAuthorized) {
    return (
      <EngineerLayout>
        <div className="max-w-3xl mx-auto px-4 py-16 text-sm text-white/50">
          {isChecking ? 'Loading...' : 'Access denied.'}
        </div>
      </EngineerLayout>
    );
  }

  const inputClass = 'w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20';
  const labelClass = 'block text-xs font-medium text-white/40 mb-1.5';

  return (
    <EngineerLayout>
      <Toaster position="top-right" toastOptions={{ style: { background: '#1F2937', color: '#E5E7EB', border: '1px solid rgba(255,255,255,0.08)' } }} />
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <h1 className="text-2xl font-display font-bold text-white mb-2">Upload Design</h1>
        <p className="text-sm text-white/40 mb-6">Engineer submissions are allowed. Architectural plans require approved architect verification before approval.</p>

        <div className="glass-card rounded-2xl p-6 space-y-5">
          <div>
            <label className={labelClass}>Title *</label>
            <input name="title" value={designData.title} onChange={handleInputChange} className={inputClass} />
          </div>
          <div>
            <label className={labelClass}>Description *</label>
            <textarea name="description" value={designData.description} onChange={handleInputChange} rows={4} className={`${inputClass} resize-none`} />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className={labelClass}>Category *</label>
              <select name="category" value={designData.category} onChange={handleInputChange} className={inputClass}>
                <option value="Bungalow">Bungalow</option>
                <option value="TwoStory">Two Story</option>
                <option value="Mansion">Mansion</option>
                <option value="Apartment">Apartment</option>
                <option value="Commercial">Commercial</option>
              </select>
            </div>
            <div>
              <label className={labelClass}>Price (KES) *</label>
              <input name="price" type="number" value={designData.price} onChange={handleInputChange} className={inputClass} />
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className={labelClass}>Bedrooms *</label>
              <input name="bedrooms" type="number" value={designData.bedrooms} onChange={handleInputChange} className={inputClass} />
            </div>
            <div>
              <label className={labelClass}>Bathrooms *</label>
              <input name="bathrooms" type="number" value={designData.bathrooms} onChange={handleInputChange} className={inputClass} />
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className={labelClass}>Square Footage *</label>
              <input name="squareFootage" type="number" value={designData.squareFootage} onChange={handleInputChange} className={inputClass} />
            </div>
            <div>
              <label className={labelClass}>Stories *</label>
              <input name="stories" type="number" value={designData.stories} onChange={handleInputChange} className={inputClass} />
            </div>
          </div>

          <div>
            <label className={labelClass}>Estimated Construction Cost (KES) *</label>
            <input name="estimatedConstructionCost" type="number" value={designData.estimatedConstructionCost} onChange={handleInputChange} className={inputClass} />
          </div>

          <div className="border-t border-white/10 pt-5 space-y-4">
            <div>
              <label className={labelClass}>Preview Images *</label>
              <input type="file" multiple accept="image/*" onChange={(e) => handleFileChange(e, 'previewImages')} className={inputClass} />
            </div>
            <div>
              <label className={labelClass}>Preview Videos</label>
              <input type="file" multiple accept="video/*" onChange={(e) => handleFileChange(e, 'previewVideos')} className={inputClass} />
            </div>
            <div>
              <label className={labelClass}>Architectural Drawings *</label>
              <input type="file" multiple accept=".pdf,.dwg,.dxf" onChange={(e) => handleFileChange(e, 'architecturalDrawings')} className={inputClass} />
            </div>
            <div>
              <label className={labelClass}>Structural Drawings *</label>
              <input type="file" multiple accept=".pdf,.dwg,.dxf" onChange={(e) => handleFileChange(e, 'structuralDrawings')} className={inputClass} />
            </div>
            <div>
              <label className={labelClass}>BOQ *</label>
              <input type="file" accept=".pdf,.xlsx,.xls" onChange={(e) => handleFileChange(e, 'boq')} className={inputClass} />
            </div>
          </div>

          <button
            onClick={handleSubmit}
            disabled={isUploading}
            className="w-full py-3 bg-slate-teal text-white font-semibold rounded-lg hover:bg-teal-500 transition-all duration-300 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isUploading ? 'Uploading...' : 'Submit Design'}
          </button>
        </div>
      </div>
    </EngineerLayout>
  );
}
