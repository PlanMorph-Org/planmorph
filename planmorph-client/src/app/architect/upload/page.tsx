'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import Layout from '@/src/components/Layout';
import api from '@/src/lib/api';
import toast, { Toaster } from 'react-hot-toast';
import ArchitectLayout from '@/src/components/ArchitectLayout';
import { useRoleGuard } from '@/src/hooks/useRoleGuard';

export default function UploadDesignPage() {
  const router = useRouter();
  const { isAuthorized, isChecking } = useRoleGuard({
    requiredRole: 'Architect',
    redirectTo: '/architect/login',
    message: 'Access denied. Architect account required.',
  });
  const [step, setStep] = useState(1);
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

  const [previewUrls, setPreviewUrls] = useState({
    images: [] as string[],
    videos: [] as string[],
  });

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    setDesignData({
      ...designData,
      [e.target.name]: e.target.value,
    });
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>, fileType: keyof typeof files) => {
    const selectedFiles = e.target.files;
    if (!selectedFiles) return;

    if (fileType === 'boq') {
      setFiles({ ...files, boq: selectedFiles[0] });
    } else {
      const filesArray = Array.from(selectedFiles);
      setFiles({ ...files, [fileType]: filesArray });

      // Create preview URLs for images and videos
      if (fileType === 'previewImages') {
        const urls = filesArray.map(file => URL.createObjectURL(file));
        setPreviewUrls(prev => ({ ...prev, images: urls }));
      } else if (fileType === 'previewVideos') {
        const urls = filesArray.map(file => URL.createObjectURL(file));
        setPreviewUrls(prev => ({ ...prev, videos: urls }));
      }
    }
  };

  const removeFile = (fileType: keyof typeof files, index: number) => {
    if (fileType === 'boq') {
      setFiles({ ...files, boq: null });
    } else {
      const currentFiles = files[fileType] as File[];
      const newFiles = currentFiles.filter((_, i) => i !== index);
      setFiles({ ...files, [fileType]: newFiles });

      if (fileType === 'previewImages') {
        const newUrls = previewUrls.images.filter((_, i) => i !== index);
        setPreviewUrls(prev => ({ ...prev, images: newUrls }));
      } else if (fileType === 'previewVideos') {
        const newUrls = previewUrls.videos.filter((_, i) => i !== index);
        setPreviewUrls(prev => ({ ...prev, videos: newUrls }));
      }
    }
  };

  const handleSubmit = async () => {
    // Validation
    if (!designData.title || !designData.description || !designData.price) {
      toast.error('Please fill in all required fields');
      return;
    }

    if (files.previewImages.length === 0) {
      toast.error('Please upload at least one preview image');
      return;
    }

    if (files.architecturalDrawings.length === 0 || files.structuralDrawings.length === 0 || !files.boq) {
      toast.error('Please upload all required documents (Architectural Drawings, Structural Drawings, and BOQ)');
      return;
    }

    setIsUploading(true);

    try {
      // Step 1: Create the design
      const createResponse = await api.post('/designs', {
        title: designData.title,
        description: designData.description,
        price: parseFloat(designData.price),
        bedrooms: parseInt(designData.bedrooms),
        bathrooms: parseInt(designData.bathrooms),
        squareFootage: parseFloat(designData.squareFootage),
        stories: parseInt(designData.stories),
        category: designData.category,
        estimatedConstructionCost: parseFloat(designData.estimatedConstructionCost),
      });

      const designId = createResponse.data.id;

      // Step 2: Upload files
      const uploadPromises = [];

      // Upload preview images
      if (files.previewImages.length > 0) {
        const formData = new FormData();
        files.previewImages.forEach(file => formData.append('files', file));
        uploadPromises.push(
          api.post(`/designs/${designId}/files?category=PreviewImage`, formData, {
            headers: { 'Content-Type': 'multipart/form-data' },
          })
        );
      }

      // Upload preview videos
      if (files.previewVideos.length > 0) {
        const formData = new FormData();
        files.previewVideos.forEach(file => formData.append('files', file));
        uploadPromises.push(
          api.post(`/designs/${designId}/files?category=PreviewVideo`, formData, {
            headers: { 'Content-Type': 'multipart/form-data' },
          })
        );
      }

      // Upload architectural drawings
      if (files.architecturalDrawings.length > 0) {
        const formData = new FormData();
        files.architecturalDrawings.forEach(file => formData.append('files', file));
        uploadPromises.push(
          api.post(`/designs/${designId}/files?category=ArchitecturalDrawing`, formData, {
            headers: { 'Content-Type': 'multipart/form-data' },
          })
        );
      }

      // Upload structural drawings
      if (files.structuralDrawings.length > 0) {
        const formData = new FormData();
        files.structuralDrawings.forEach(file => formData.append('files', file));
        uploadPromises.push(
          api.post(`/designs/${designId}/files?category=StructuralDrawing`, formData, {
            headers: { 'Content-Type': 'multipart/form-data' },
          })
        );
      }

      // Upload BOQ
      if (files.boq) {
        const formData = new FormData();
        formData.append('files', files.boq);
        uploadPromises.push(
          api.post(`/designs/${designId}/files?category=BOQ`, formData, {
            headers: { 'Content-Type': 'multipart/form-data' },
          })
        );
      }

      await Promise.all(uploadPromises);

      toast.success('Design uploaded successfully! Awaiting professional verification and admin approval.');
      router.push('/architect');
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to upload design. Please try again.');
    } finally {
      setIsUploading(false);
    }
  };

  const nextStep = () => {
    if (step === 1) {
      if (!designData.title || !designData.description || !designData.price) {
        toast.error('Please fill in all required fields');
        return;
      }
    }
    setStep(step + 1);
  };

  const prevStep = () => setStep(step - 1);

  if (isChecking) {
    return (
      <Layout>
        <div className="flex justify-center items-center h-screen">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        </div>
      </Layout>
    );
  }

  if (!isAuthorized) {
    return (
      <Layout>
        <div className="text-center py-12">
          <p className="text-red-600">Access denied. Architect account required.</p>
        </div>
      </Layout>
    );
  }

  return (
    <ArchitectLayout>
      
      <Toaster position="top-right" />
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Upload New Design</h1>
          <p className="text-gray-600 mt-2">Share your architectural design with potential clients</p>
        </div>

        {/* Progress Indicator */}
        <div className="mb-8">
          <div className="flex items-center justify-between">
            <div className="flex items-center">
              <div className={`flex items-center justify-center w-10 h-10 rounded-full ${step >= 1 ? 'bg-blue-600 text-white' : 'bg-gray-300 text-gray-600'}`}>
                1
              </div>
              <span className="ml-2 text-sm font-medium">Basic Info</span>
            </div>
            <div className="flex-1 h-1 mx-4 bg-gray-300">
              <div className={`h-full ${step >= 2 ? 'bg-blue-600' : 'bg-gray-300'}`} style={{ width: step >= 2 ? '100%' : '0%' }}></div>
            </div>
            <div className="flex items-center">
              <div className={`flex items-center justify-center w-10 h-10 rounded-full ${step >= 2 ? 'bg-blue-600 text-white' : 'bg-gray-300 text-gray-600'}`}>
                2
              </div>
              <span className="ml-2 text-sm font-medium">Preview Files</span>
            </div>
            <div className="flex-1 h-1 mx-4 bg-gray-300">
              <div className={`h-full ${step >= 3 ? 'bg-blue-600' : 'bg-gray-300'}`} style={{ width: step >= 3 ? '100%' : '0%' }}></div>
            </div>
            <div className="flex items-center">
              <div className={`flex items-center justify-center w-10 h-10 rounded-full ${step >= 3 ? 'bg-blue-600 text-white' : 'bg-gray-300 text-gray-600'}`}>
                3
              </div>
              <span className="ml-2 text-sm font-medium">Documents</span>
            </div>
          </div>
        </div>

        <div className="bg-white shadow rounded-lg p-6">
          {/* Step 1: Basic Information */}
          {step === 1 && (
            <div className="space-y-6">
              <h2 className="text-xl font-semibold text-gray-900">Basic Information</h2>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Design Title *
                </label>
                <input
                  type="text"
                  name="title"
                  value={designData.title}
                  onChange={handleInputChange}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                  placeholder="e.g., Modern 3-Bedroom Bungalow"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Description *
                </label>
                <textarea
                  name="description"
                  value={designData.description}
                  onChange={handleInputChange}
                  rows={4}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                  placeholder="Describe your design, key features, materials, etc."
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Category *
                  </label>
                  <select
                    name="category"
                    value={designData.category}
                    onChange={handleInputChange}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                  >
                    <option value="Bungalow">Bungalow</option>
                    <option value="TwoStory">Two Story</option>
                    <option value="Mansion">Mansion</option>
                    <option value="Apartment">Apartment</option>
                    <option value="Commercial">Commercial</option>
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Price (KES base) *
                  </label>
                  <input
                    type="number"
                    name="price"
                    value={designData.price}
                    onChange={handleInputChange}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                    placeholder="50000"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Bedrooms *
                  </label>
                  <input
                    type="number"
                    name="bedrooms"
                    value={designData.bedrooms}
                    onChange={handleInputChange}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                    placeholder="3"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Bathrooms *
                  </label>
                  <input
                    type="number"
                    name="bathrooms"
                    value={designData.bathrooms}
                    onChange={handleInputChange}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                    placeholder="2"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Square Footage *
                  </label>
                  <input
                    type="number"
                    name="squareFootage"
                    value={designData.squareFootage}
                    onChange={handleInputChange}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                    placeholder="1500"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Stories *
                  </label>
                  <input
                    type="number"
                    name="stories"
                    value={designData.stories}
                    onChange={handleInputChange}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                    placeholder="1"
                  />
                </div>
              </div>

              <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Estimated Construction Cost (KES base) *
                  </label>
                <input
                  type="number"
                  name="estimatedConstructionCost"
                  value={designData.estimatedConstructionCost}
                  onChange={handleInputChange}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                  placeholder="3000000"
                />
                <p className="text-xs text-gray-500 mt-1">
                  This helps clients budget for construction
                </p>
              </div>
            </div>
          )}

          {/* Step 2: Preview Files */}
          {step === 2 && (
            <div className="space-y-6">
              <h2 className="text-xl font-semibold text-gray-900">Preview Files</h2>
              <p className="text-sm text-gray-600">Upload images and videos that clients will see before purchasing</p>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Preview Images (D5 Renders) *
                </label>
                <input
                  type="file"
                  accept="image/jpeg,image/png,image/jpg"
                  multiple
                  onChange={(e) => handleFileChange(e, 'previewImages')}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                />
                <p className="text-xs text-gray-500 mt-1">
                  Upload 3-6 high-quality renders (JPG, PNG). Max 10MB each.
                </p>

                {previewUrls.images.length > 0 && (
                  <div className="mt-4 grid grid-cols-3 gap-4">
                    {previewUrls.images.map((url, index) => (
                      <div key={index} className="relative">
                        <img src={url} alt={`Preview ${index + 1}`} className="w-full h-32 object-cover rounded-lg" />
                        <button
                          onClick={() => removeFile('previewImages', index)}
                          className="absolute top-2 right-2 bg-red-500 text-white rounded-full p-1 hover:bg-red-600"
                        >
                          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                          </svg>
                        </button>
                      </div>
                    ))}
                  </div>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Preview Videos (Optional)
                </label>
                <input
                  type="file"
                  accept="video/mp4,video/avi,video/mov"
                  multiple
                  onChange={(e) => handleFileChange(e, 'previewVideos')}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                />
                <p className="text-xs text-gray-500 mt-1">
                  Upload 1-2 walkthrough videos (MP4). Max 200MB each.
                </p>

                {files.previewVideos.length > 0 && (
                  <div className="mt-4 space-y-2">
                    {files.previewVideos.map((file, index) => (
                      <div key={index} className="flex items-center justify-between bg-gray-50 p-3 rounded-lg">
                        <span className="text-sm text-gray-700">{file.name}</span>
                        <button
                          onClick={() => removeFile('previewVideos', index)}
                          className="text-red-500 hover:text-red-700"
                        >
                          Remove
                        </button>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          )}

          {/* Step 3: Documents */}
          {step === 3 && (
            <div className="space-y-6">
              <h2 className="text-xl font-semibold text-gray-900">Project Documents</h2>
              <p className="text-sm text-gray-600">Upload the files that clients will receive after purchase</p>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Architectural Drawings *
                </label>
                <input
                  type="file"
                  accept=".pdf,.dwg,.dxf"
                  multiple
                  onChange={(e) => handleFileChange(e, 'architecturalDrawings')}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                />
                <p className="text-xs text-gray-500 mt-1">
                  Upload complete architectural plans (PDF, DWG, DXF)
                </p>

                {files.architecturalDrawings.length > 0 && (
                  <div className="mt-4 space-y-2">
                    {files.architecturalDrawings.map((file, index) => (
                      <div key={index} className="flex items-center justify-between bg-gray-50 p-3 rounded-lg">
                        <span className="text-sm text-gray-700">{file.name}</span>
                        <button
                          onClick={() => removeFile('architecturalDrawings', index)}
                          className="text-red-500 hover:text-red-700"
                        >
                          Remove
                        </button>
                      </div>
                    ))}
                  </div>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Structural/Engineering Drawings *
                </label>
                <input
                  type="file"
                  accept=".pdf,.dwg,.dxf"
                  multiple
                  onChange={(e) => handleFileChange(e, 'structuralDrawings')}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                />
                <p className="text-xs text-gray-500 mt-1">
                  Upload structural engineering plans (PDF, DWG, DXF)
                </p>

                {files.structuralDrawings.length > 0 && (
                  <div className="mt-4 space-y-2">
                    {files.structuralDrawings.map((file, index) => (
                      <div key={index} className="flex items-center justify-between bg-gray-50 p-3 rounded-lg">
                        <span className="text-sm text-gray-700">{file.name}</span>
                        <button
                          onClick={() => removeFile('structuralDrawings', index)}
                          className="text-red-500 hover:text-red-700"
                        >
                          Remove
                        </button>
                      </div>
                    ))}
                  </div>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Bill of Quantities (BOQ) *
                </label>
                <input
                  type="file"
                  accept=".pdf,.xlsx,.xls"
                  onChange={(e) => handleFileChange(e, 'boq')}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                />
                <p className="text-xs text-gray-500 mt-1">
                  Upload BOQ with material quantities and cost estimates (PDF, Excel)
                </p>

                {files.boq && (
                  <div className="mt-4">
                    <div className="flex items-center justify-between bg-gray-50 p-3 rounded-lg">
                      <span className="text-sm text-gray-700">{files.boq.name}</span>
                      <button
                        onClick={() => removeFile('boq', 0)}
                        className="text-red-500 hover:text-red-700"
                      >
                        Remove
                      </button>
                    </div>
                  </div>
                )}
              </div>

              <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                <p className="text-sm text-blue-800">
                  <strong>Note:</strong> Architectural and structural drawings will be verified by licensed professionals, 
                  and the BOQ must be verified by both. You'll be notified once admin approval is complete.
                </p>
              </div>
            </div>
          )}

          {/* Navigation Buttons */}
          <div className="mt-8 flex justify-between">
            {step > 1 && (
              <button
                onClick={prevStep}
                className="px-6 py-2 border border-gray-300 text-gray-700 rounded-md hover:bg-gray-50"
              >
                Previous
              </button>
            )}

            <div className="ml-auto">
              {step < 3 ? (
                <button
                  onClick={nextStep}
                  className="px-6 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
                >
                  Next
                </button>
              ) : (
                <button
                  onClick={handleSubmit}
                  disabled={isUploading}
                  className="px-6 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:bg-gray-400"
                >
                  {isUploading ? 'Uploading...' : 'Submit Design'}
                </button>
              )}
            </div>
          </div>
        </div>
      </div>
    </ArchitectLayout>
  );
}
