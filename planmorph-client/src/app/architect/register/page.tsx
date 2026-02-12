'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import api from '@/src/lib/api';
import toast, { Toaster } from 'react-hot-toast';

export default function ArchitectRegisterPage() {
  const router = useRouter();
  const [formData, setFormData] = useState({
    email: '',
    password: '',
    confirmPassword: '',
    firstName: '',
    lastName: '',
    phoneNumber: '',
    professionalLicense: '',
    yearsOfExperience: '',
    portfolio: '',
    specialization: '',
  });
  const [cvFile, setCvFile] = useState<File | null>(null);
  const [coverLetterFile, setCoverLetterFile] = useState<File | null>(null);
  const [workExperienceFile, setWorkExperienceFile] = useState<File | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (formData.password !== formData.confirmPassword) {
      toast.error('Passwords do not match');
      return;
    }

    if (!formData.professionalLicense || !formData.yearsOfExperience) {
      toast.error('Please fill in all required fields');
      return;
    }

    const hasPortfolio = formData.portfolio.trim().length > 0;
    const isPdf = (file: File | null) =>
      !!file && (file.type === 'application/pdf' || file.name.toLowerCase().endsWith('.pdf'));

    if (!hasPortfolio) {
      if (!cvFile || !coverLetterFile || !workExperienceFile) {
        toast.error('Upload your CV, cover letter, and work experience PDF if you do not have a portfolio.');
        return;
      }

      if (!isPdf(cvFile) || !isPdf(coverLetterFile) || !isPdf(workExperienceFile)) {
        toast.error('CV, cover letter, and work experience must be PDF files.');
        return;
      }
    }

    setIsLoading(true);

    try {
      // Register as architect
      const years = formData.yearsOfExperience ? parseInt(formData.yearsOfExperience, 10) : undefined;
      const payload = new FormData();
      payload.append('email', formData.email);
      payload.append('password', formData.password);
      payload.append('firstName', formData.firstName);
      payload.append('lastName', formData.lastName);
      payload.append('phoneNumber', formData.phoneNumber);
      payload.append('role', 'Architect');
      payload.append('professionalLicense', formData.professionalLicense);
      if (!Number.isNaN(years) && years !== undefined) {
        payload.append('yearsOfExperience', String(years));
      }
      if (formData.portfolio.trim().length > 0) {
        payload.append('portfolioUrl', formData.portfolio.trim());
      }
      if (formData.specialization.trim().length > 0) {
        payload.append('specialization', formData.specialization.trim());
      }
      if (!hasPortfolio) {
        if (cvFile) {
          payload.append('cvFile', cvFile);
        }
        if (coverLetterFile) {
          payload.append('coverLetterFile', coverLetterFile);
        }
        if (workExperienceFile) {
          payload.append('workExperienceFile', workExperienceFile);
        }
      }

      await api.post('/auth/register-professional', payload);

      toast.success('Application submitted successfully! Our team will review and approve your account within 24-48 hours.', {
        duration: 6000,
      });

      setTimeout(() => {
        router.push('/architect/login');
      }, 2000);
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Registration failed. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleFileChange = (setter: (file: File | null) => void) =>
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const file = e.target.files?.[0] ?? null;
      setter(file);
    };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 py-12 px-4 sm:px-6 lg:px-8">
      <Toaster position="top-right" />
      <div className="max-w-2xl mx-auto bg-white rounded-lg shadow-xl p-8">
        <div className="text-center mb-8">
          <Link href="/" className="inline-flex items-center gap-2 text-3xl font-bold text-blue-600">
            <img src="/planmorph.svg" alt="PlanMorph" className="h-9 w-auto" />
            <span>PlanMorph</span>
          </Link>
          <p className="text-sm text-gray-500 mt-1">Architect Portal</p>
          <h2 className="mt-6 text-3xl font-extrabold text-gray-900">
            Apply as an Architect
          </h2>
          <p className="mt-2 text-sm text-gray-600">
            Already have an account?{' '}
            <Link href="/architect/login" className="font-medium text-blue-600 hover:text-blue-500">
              Sign in
            </Link>
          </p>
        </div>

        <form className="space-y-6" onSubmit={handleSubmit}>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label htmlFor="firstName" className="block text-sm font-medium text-gray-700 mb-1">
                First Name *
              </label>
              <input
                id="firstName"
                name="firstName"
                type="text"
                required
                value={formData.firstName}
                onChange={handleChange}
                className="appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
              />
            </div>
            <div>
              <label htmlFor="lastName" className="block text-sm font-medium text-gray-700 mb-1">
                Last Name *
              </label>
              <input
                id="lastName"
                name="lastName"
                type="text"
                required
                value={formData.lastName}
                onChange={handleChange}
                className="appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
              />
            </div>
          </div>

          <div>
            <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-1">
              Email address *
            </label>
            <input
              id="email"
              name="email"
              type="email"
              required
              value={formData.email}
              onChange={handleChange}
              className="appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
            />
          </div>

          <div>
            <label htmlFor="phoneNumber" className="block text-sm font-medium text-gray-700 mb-1">
              Phone Number *
            </label>
            <input
              id="phoneNumber"
              name="phoneNumber"
              type="tel"
              required
              value={formData.phoneNumber}
              onChange={handleChange}
              className="appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
              placeholder="+[country code] [number]"
            />
          </div>

          <div className="border-t pt-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Professional Information</h3>
            
            <div className="space-y-4">
              <div>
                <label htmlFor="professionalLicense" className="block text-sm font-medium text-gray-700 mb-1">
                  Professional License Number *
                </label>
                <input
                  id="professionalLicense"
                  name="professionalLicense"
                  type="text"
                  required
                  value={formData.professionalLicense}
                  onChange={handleChange}
                  className="appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                  placeholder="Professional license number"
                />
              </div>

              <div>
                <label htmlFor="yearsOfExperience" className="block text-sm font-medium text-gray-700 mb-1">
                  Years of Experience *
                </label>
                <input
                  id="yearsOfExperience"
                  name="yearsOfExperience"
                  type="number"
                  required
                  value={formData.yearsOfExperience}
                  onChange={handleChange}
                  className="appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                />
              </div>

              <div>
                <label htmlFor="specialization" className="block text-sm font-medium text-gray-700 mb-1">
                  Specialization (Optional)
                </label>
                <input
                  id="specialization"
                  name="specialization"
                  type="text"
                  value={formData.specialization}
                  onChange={handleChange}
                  className="appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                  placeholder="e.g., Residential, Commercial, Green Buildings"
                />
              </div>

              <div>
                <label htmlFor="portfolio" className="block text-sm font-medium text-gray-700 mb-1">
                  Portfolio/Website URL (Optional)
                </label>
                <input
                  id="portfolio"
                  name="portfolio"
                  type="url"
                  value={formData.portfolio}
                  onChange={handleChange}
                  className="appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                  placeholder="https://yourportfolio.com"
                />
              </div>

              {!formData.portfolio.trim() && (
                <div className="rounded-lg border border-blue-100 bg-blue-50 p-4">
                  <p className="text-sm text-blue-800 mb-3">
                    No portfolio? Upload your CV, cover letter, and work experience PDF.
                  </p>
                  <div className="space-y-3">
                    <div>
                      <label htmlFor="cvFile" className="block text-sm font-medium text-gray-700 mb-1">
                        CV (PDF) *
                      </label>
                      <input
                        id="cvFile"
                        name="cvFile"
                        type="file"
                        accept=".pdf,application/pdf"
                        required
                        onChange={handleFileChange(setCvFile)}
                        className="block w-full text-sm text-gray-700"
                      />
                    </div>
                    <div>
                      <label htmlFor="coverLetterFile" className="block text-sm font-medium text-gray-700 mb-1">
                        Cover Letter (PDF) *
                      </label>
                      <input
                        id="coverLetterFile"
                        name="coverLetterFile"
                        type="file"
                        accept=".pdf,application/pdf"
                        required
                        onChange={handleFileChange(setCoverLetterFile)}
                        className="block w-full text-sm text-gray-700"
                      />
                    </div>
                    <div>
                      <label htmlFor="workExperienceFile" className="block text-sm font-medium text-gray-700 mb-1">
                        Work Experience (PDF) *
                      </label>
                      <input
                        id="workExperienceFile"
                        name="workExperienceFile"
                        type="file"
                        accept=".pdf,application/pdf"
                        required
                        onChange={handleFileChange(setWorkExperienceFile)}
                        className="block w-full text-sm text-gray-700"
                      />
                    </div>
                  </div>
                </div>
              )}
            </div>
          </div>

          <div className="border-t pt-6">
            <div>
              <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-1">
                Password *
              </label>
              <input
                id="password"
                name="password"
                type="password"
                required
                value={formData.password}
                onChange={handleChange}
                className="appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                placeholder="Minimum 8 characters"
              />
            </div>

            <div className="mt-4">
              <label htmlFor="confirmPassword" className="block text-sm font-medium text-gray-700 mb-1">
                Confirm Password *
              </label>
              <input
                id="confirmPassword"
                name="confirmPassword"
                type="password"
                required
                value={formData.confirmPassword}
                onChange={handleChange}
                className="appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
              />
            </div>
          </div>

          <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
            <p className="text-sm text-blue-800">
              <strong>Application Review:</strong> Your architect account will be reviewed by our admin team within 24-48 hours. 
              You'll receive an email once approved and can start uploading designs.
            </p>
          </div>

          <button
            type="submit"
            disabled={isLoading}
            className="w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:bg-gray-400"
          >
            {isLoading ? 'Submitting application...' : 'Submit Application'}
          </button>
        </form>
      </div>
    </div>
  );
}
