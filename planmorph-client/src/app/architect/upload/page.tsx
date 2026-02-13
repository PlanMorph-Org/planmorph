'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import Layout from '@/src/components/Layout';
import api from '@/src/lib/api';
import toast, { Toaster } from 'react-hot-toast';
import ArchitectLayout from '@/src/components/ArchitectLayout';
import { useRoleGuard } from '@/src/hooks/useRoleGuard';
import { motion, AnimatePresence } from 'framer-motion';

const stepLabels = ['Basic Info', 'Preview Files', 'Documents'];

export default function UploadDesignPage() {
  const router = useRouter();
  const { isAuthorized, isChecking } = useRoleGuard({ requiredRole: 'Architect', redirectTo: '/architect/login', message: 'Access denied. Architect account required.' });
  const [step, setStep] = useState(1);
  const [isUploading, setIsUploading] = useState(false);

  const [designData, setDesignData] = useState({
    title: '', description: '', price: '', bedrooms: '', bathrooms: '',
    squareFootage: '', stories: '', category: 'Bungalow', estimatedConstructionCost: '',
  });

  const [files, setFiles] = useState({
    previewImages: [] as File[], previewVideos: [] as File[],
    architecturalDrawings: [] as File[], structuralDrawings: [] as File[],
    boq: null as File | null,
  });

  const [previewUrls, setPreviewUrls] = useState({ images: [] as string[], videos: [] as string[] });

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    setDesignData({ ...designData, [e.target.name]: e.target.value });
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>, fileType: keyof typeof files) => {
    const selectedFiles = e.target.files;
    if (!selectedFiles) return;
    if (fileType === 'boq') {
      setFiles({ ...files, boq: selectedFiles[0] });
    } else {
      const filesArray = Array.from(selectedFiles);
      setFiles({ ...files, [fileType]: filesArray });
      if (fileType === 'previewImages') setPreviewUrls(prev => ({ ...prev, images: filesArray.map(f => URL.createObjectURL(f)) }));
      else if (fileType === 'previewVideos') setPreviewUrls(prev => ({ ...prev, videos: filesArray.map(f => URL.createObjectURL(f)) }));
    }
  };

  const removeFile = (fileType: keyof typeof files, index: number) => {
    if (fileType === 'boq') { setFiles({ ...files, boq: null }); return; }
    const current = files[fileType] as File[];
    const updated = current.filter((_, i) => i !== index);
    setFiles({ ...files, [fileType]: updated });
    if (fileType === 'previewImages') setPreviewUrls(prev => ({ ...prev, images: prev.images.filter((_, i) => i !== index) }));
    else if (fileType === 'previewVideos') setPreviewUrls(prev => ({ ...prev, videos: prev.videos.filter((_, i) => i !== index) }));
  };

  const handleSubmit = async () => {
    if (!designData.title || !designData.description || !designData.price) { toast.error('Please fill in all required fields'); return; }
    if (files.previewImages.length === 0) { toast.error('Please upload at least one preview image'); return; }
    if (files.architecturalDrawings.length === 0 || files.structuralDrawings.length === 0 || !files.boq) { toast.error('Please upload all required documents'); return; }

    setIsUploading(true);
    try {
      const createResponse = await api.post('/designs', {
        title: designData.title, description: designData.description,
        price: parseFloat(designData.price), bedrooms: parseInt(designData.bedrooms),
        bathrooms: parseInt(designData.bathrooms), squareFootage: parseFloat(designData.squareFootage),
        stories: parseInt(designData.stories), category: designData.category,
        estimatedConstructionCost: parseFloat(designData.estimatedConstructionCost),
      });
      const designId = createResponse.data.id;
      const uploadPromises = [];

      const uploadFiles = (fileList: File[], category: string) => {
        const fd = new FormData();
        fileList.forEach(f => fd.append('files', f));
        return api.post(`/designs/${designId}/files?category=${category}`, fd, { headers: { 'Content-Type': 'multipart/form-data' } });
      };

      if (files.previewImages.length > 0) uploadPromises.push(uploadFiles(files.previewImages, 'PreviewImage'));
      if (files.previewVideos.length > 0) uploadPromises.push(uploadFiles(files.previewVideos, 'PreviewVideo'));
      if (files.architecturalDrawings.length > 0) uploadPromises.push(uploadFiles(files.architecturalDrawings, 'ArchitecturalDrawing'));
      if (files.structuralDrawings.length > 0) uploadPromises.push(uploadFiles(files.structuralDrawings, 'StructuralDrawing'));
      if (files.boq) { const fd = new FormData(); fd.append('files', files.boq); uploadPromises.push(api.post(`/designs/${designId}/files?category=BOQ`, fd, { headers: { 'Content-Type': 'multipart/form-data' } })); }

      await Promise.all(uploadPromises);
      toast.success('Design uploaded! Awaiting verification and admin approval.');
      router.push('/architect');
    } catch (error: any) { toast.error(error.response?.data?.message || 'Failed to upload design.'); }
    finally { setIsUploading(false); }
  };

  const nextStep = () => {
    if (step === 1 && (!designData.title || !designData.description || !designData.price)) { toast.error('Please fill in all required fields'); return; }
    setStep(step + 1);
  };
  const prevStep = () => setStep(step - 1);

  const inputClass = 'w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20 focus:outline-none';
  const labelClass = 'block text-xs font-medium text-white/40 mb-1.5';

  if (isChecking) {
    return (
      <Layout>
        <div className="flex justify-center items-center h-screen">
          <div className="w-8 h-8 border-2 border-golden/30 border-t-golden rounded-full animate-spin" />
        </div>
      </Layout>
    );
  }

  if (!isAuthorized) {
    return (
      <Layout>
        <div className="text-center py-20"><p className="text-red-400 text-sm">Access denied. Architect account required.</p></div>
      </Layout>
    );
  }

  return (
    <ArchitectLayout>
      <Toaster position="top-right" toastOptions={{ style: { background: '#1F2937', color: '#E5E7EB', border: '1px solid rgba(255,255,255,0.08)' } }} />
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <motion.div initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }} className="mb-8">
          <h1 className="text-2xl md:text-3xl font-display font-bold text-white">Upload New Design</h1>
          <p className="text-sm text-white/40 mt-1">Share your architectural design with potential clients</p>
        </motion.div>

        {/* Progress Indicator */}
        <div className="mb-8">
          <div className="flex items-center justify-between">
            {stepLabels.map((label, i) => (
              <div key={label} className="flex items-center">
                {i > 0 && <div className="w-16 sm:w-24 h-0.5 mx-2"><div className={`h-full transition-all duration-500 rounded-full ${step > i ? 'bg-golden' : 'bg-white/10'}`} /></div>}
                <div className="flex items-center gap-2">
                  <div className={`flex items-center justify-center w-8 h-8 rounded-full text-xs font-bold transition-all duration-300 ${step > i ? 'bg-golden text-brand' : step === i + 1 ? 'bg-golden/20 text-golden border border-golden/40' : 'bg-white/5 text-white/20 border border-white/10'}`}>
                    {step > i + 1 ? '✓' : i + 1}
                  </div>
                  <span className={`text-xs font-medium hidden sm:inline ${step === i + 1 ? 'text-golden' : 'text-white/30'}`}>{label}</span>
                </div>
              </div>
            ))}
          </div>
        </div>

        <motion.div initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.2 }} className="glass-card rounded-2xl p-6">
          <AnimatePresence mode="wait">
            {/* Step 1: Basic Information */}
            {step === 1 && (
              <motion.div key="step1" initial={{ opacity: 0, x: 20 }} animate={{ opacity: 1, x: 0 }} exit={{ opacity: 0, x: -20 }} transition={{ duration: 0.3 }} className="space-y-5">
                <h2 className="text-base font-semibold text-white">Basic Information</h2>

                <div><label className={labelClass}>Design Title *</label>
                  <input type="text" name="title" value={designData.title} onChange={handleInputChange} className={inputClass} placeholder="e.g., Modern 3-Bedroom Bungalow" /></div>

                <div><label className={labelClass}>Description *</label>
                  <textarea name="description" value={designData.description} onChange={handleInputChange} rows={4} className={`${inputClass} resize-none`} placeholder="Describe your design, key features, materials..." /></div>

                <div className="grid grid-cols-2 gap-4">
                  <div><label className={labelClass}>Category *</label>
                    <select name="category" value={designData.category} onChange={handleInputChange} className={inputClass}>
                      <option value="Bungalow">Bungalow</option><option value="TwoStory">Two Story</option>
                      <option value="Mansion">Mansion</option><option value="Apartment">Apartment</option>
                      <option value="Commercial">Commercial</option>
                    </select></div>
                  <div><label className={labelClass}>Price (KES base) *</label>
                    <input type="number" name="price" value={designData.price} onChange={handleInputChange} className={inputClass} placeholder="50000" /></div>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div><label className={labelClass}>Bedrooms *</label>
                    <input type="number" name="bedrooms" value={designData.bedrooms} onChange={handleInputChange} className={inputClass} placeholder="3" /></div>
                  <div><label className={labelClass}>Bathrooms *</label>
                    <input type="number" name="bathrooms" value={designData.bathrooms} onChange={handleInputChange} className={inputClass} placeholder="2" /></div>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div><label className={labelClass}>Square Footage *</label>
                    <input type="number" name="squareFootage" value={designData.squareFootage} onChange={handleInputChange} className={inputClass} placeholder="1500" /></div>
                  <div><label className={labelClass}>Stories *</label>
                    <input type="number" name="stories" value={designData.stories} onChange={handleInputChange} className={inputClass} placeholder="1" /></div>
                </div>

                <div><label className={labelClass}>Estimated Construction Cost (KES base) *</label>
                  <input type="number" name="estimatedConstructionCost" value={designData.estimatedConstructionCost} onChange={handleInputChange} className={inputClass} placeholder="3000000" />
                  <p className="text-[10px] text-white/20 mt-1">Helps clients budget for construction</p></div>
              </motion.div>
            )}

            {/* Step 2: Preview Files */}
            {step === 2 && (
              <motion.div key="step2" initial={{ opacity: 0, x: 20 }} animate={{ opacity: 1, x: 0 }} exit={{ opacity: 0, x: -20 }} transition={{ duration: 0.3 }} className="space-y-6">
                <div>
                  <h2 className="text-base font-semibold text-white">Preview Files</h2>
                  <p className="text-xs text-white/30 mt-0.5">Upload images and videos clients will see before purchasing</p>
                </div>

                <div>
                  <label className={labelClass}>Preview Images (D5 Renders) *</label>
                  <label className="flex items-center justify-center gap-2 w-full py-6 glass-input rounded-lg border-dashed border-2 border-white/10 hover:border-golden/30 transition-colors cursor-pointer">
                    <svg className="w-5 h-5 text-white/30" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>
                    <span className="text-xs text-white/30">Click to upload images (JPG, PNG). Max 10MB each.</span>
                    <input type="file" accept="image/jpeg,image/png,image/jpg" multiple onChange={(e) => handleFileChange(e, 'previewImages')} className="hidden" />
                  </label>
                  {previewUrls.images.length > 0 && (
                    <div className="mt-3 grid grid-cols-3 gap-3">
                      {previewUrls.images.map((url, i) => (
                        <div key={i} className="relative group rounded-lg overflow-hidden">
                          <img src={url} alt={`Preview ${i + 1}`} className="w-full h-28 object-cover" />
                          <button onClick={() => removeFile('previewImages', i)} className="absolute top-1.5 right-1.5 w-6 h-6 bg-red-500/80 text-white rounded-full flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity text-xs">✕</button>
                        </div>
                      ))}
                    </div>
                  )}
                </div>

                <div>
                  <label className={labelClass}>Preview Videos (Optional)</label>
                  <label className="flex items-center justify-center gap-2 w-full py-6 glass-input rounded-lg border-dashed border-2 border-white/10 hover:border-golden/30 transition-colors cursor-pointer">
                    <svg className="w-5 h-5 text-white/30" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M15 10l4.553-2.276A1 1 0 0121 8.618v6.764a1 1 0 01-1.447.894L15 14M5 18h8a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" /></svg>
                    <span className="text-xs text-white/30">Click to upload videos (MP4). Max 200MB each.</span>
                    <input type="file" accept="video/mp4,video/avi,video/mov" multiple onChange={(e) => handleFileChange(e, 'previewVideos')} className="hidden" />
                  </label>
                  {files.previewVideos.length > 0 && (
                    <div className="mt-3 space-y-2">
                      {files.previewVideos.map((file, i) => (
                        <div key={i} className="flex items-center justify-between glass-card-light rounded-lg px-4 py-2.5">
                          <span className="text-sm text-white/60">{file.name}</span>
                          <button onClick={() => removeFile('previewVideos', i)} className="text-xs text-red-400 hover:text-red-300 transition-colors">Remove</button>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </motion.div>
            )}

            {/* Step 3: Documents */}
            {step === 3 && (
              <motion.div key="step3" initial={{ opacity: 0, x: 20 }} animate={{ opacity: 1, x: 0 }} exit={{ opacity: 0, x: -20 }} transition={{ duration: 0.3 }} className="space-y-6">
                <div>
                  <h2 className="text-base font-semibold text-white">Project Documents</h2>
                  <p className="text-xs text-white/30 mt-0.5">Files clients receive after purchase</p>
                </div>

                {[
                  { label: 'Architectural Drawings *', key: 'architecturalDrawings' as const, accept: '.pdf,.dwg,.dxf', hint: 'Complete architectural plans (PDF, DWG, DXF)' },
                  { label: 'Structural/Engineering Drawings *', key: 'structuralDrawings' as const, accept: '.pdf,.dwg,.dxf', hint: 'Structural engineering plans (PDF, DWG, DXF)' },
                ].map(({ label, key, accept, hint }) => (
                  <div key={key}>
                    <label className={labelClass}>{label}</label>
                    <label className="flex items-center justify-center gap-2 w-full py-5 glass-input rounded-lg border-dashed border-2 border-white/10 hover:border-golden/30 transition-colors cursor-pointer">
                      <svg className="w-5 h-5 text-white/30" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" /></svg>
                      <span className="text-xs text-white/30">{hint}</span>
                      <input type="file" accept={accept} multiple onChange={(e) => handleFileChange(e, key)} className="hidden" />
                    </label>
                    {(files[key] as File[]).length > 0 && (
                      <div className="mt-2 space-y-1.5">
                        {(files[key] as File[]).map((file, i) => (
                          <div key={i} className="flex items-center justify-between glass-card-light rounded-lg px-4 py-2.5">
                            <span className="text-sm text-white/60">{file.name}</span>
                            <button onClick={() => removeFile(key, i)} className="text-xs text-red-400 hover:text-red-300 transition-colors">Remove</button>
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                ))}

                <div>
                  <label className={labelClass}>Bill of Quantities (BOQ) *</label>
                  <label className="flex items-center justify-center gap-2 w-full py-5 glass-input rounded-lg border-dashed border-2 border-white/10 hover:border-golden/30 transition-colors cursor-pointer">
                    <svg className="w-5 h-5 text-white/30" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 17v-2m3 2v-4m3 4v-6m2 10H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" /></svg>
                    <span className="text-xs text-white/30">BOQ with material quantities and costs (PDF, Excel)</span>
                    <input type="file" accept=".pdf,.xlsx,.xls" onChange={(e) => handleFileChange(e, 'boq')} className="hidden" />
                  </label>
                  {files.boq && (
                    <div className="mt-2">
                      <div className="flex items-center justify-between glass-card-light rounded-lg px-4 py-2.5">
                        <span className="text-sm text-white/60">{files.boq.name}</span>
                        <button onClick={() => removeFile('boq', 0)} className="text-xs text-red-400 hover:text-red-300 transition-colors">Remove</button>
                      </div>
                    </div>
                  )}
                </div>

                <div className="glass-card-light rounded-lg p-4 border border-golden/10">
                  <p className="text-xs text-golden/60">
                    <strong className="text-golden">Note:</strong> Architectural and structural drawings will be verified by licensed professionals, and the BOQ must be verified by both. You'll be notified once admin approval is complete.
                  </p>
                </div>
              </motion.div>
            )}
          </AnimatePresence>

          {/* Navigation Buttons */}
          <div className="mt-8 flex justify-between">
            {step > 1 && (
              <button onClick={prevStep} className="px-5 py-2.5 border border-white/10 rounded-lg text-sm text-white/40 hover:text-white hover:bg-white/5 transition-all">
                Previous
              </button>
            )}
            <div className="ml-auto">
              {step < 3 ? (
                <button onClick={nextStep} className="px-6 py-2.5 bg-golden text-brand font-semibold rounded-lg hover:bg-golden-light transition-all duration-300 text-sm">
                  Next
                </button>
              ) : (
                <button onClick={handleSubmit} disabled={isUploading}
                  className="px-6 py-2.5 bg-golden text-brand font-semibold rounded-lg hover:bg-golden-light transition-all duration-300 text-sm disabled:opacity-40 disabled:cursor-not-allowed">
                  {isUploading ? (
                    <span className="flex items-center gap-2">
                      <div className="w-4 h-4 border-2 border-brand/30 border-t-brand rounded-full animate-spin" />
                      Uploading...
                    </span>
                  ) : 'Submit Design'}
                </button>
              )}
            </div>
          </div>
        </motion.div>
      </div>
    </ArchitectLayout>
  );
}
