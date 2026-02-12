'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import Layout from '@/src/components/Layout';
import { useAuthStore } from '@/src/store/authStore';
import api from '@/src/lib/api';
import { AuthResponse } from '@/src/types';
import toast, { Toaster } from 'react-hot-toast';

export default function RegisterPage() {
  const router = useRouter();
  const { register } = useAuthStore();
  const [accountType, setAccountType] = useState<'Client' | 'Architect' | 'Engineer'>('Client');
  const [formData, setFormData] = useState({
    email: '',
    password: '',
    confirmPassword: '',
    firstName: '',
    lastName: '',
    phoneNumber: '',
    // Architect-specific fields
    professionalLicense: '',
    yearsOfExperience: '',
    portfolio: '',
    specialization: '',
  });
  const [cvFile, setCvFile] = useState<File | null>(null);
  const [coverLetterFile, setCoverLetterFile] = useState<File | null>(null);
  const [workExperienceFile, setWorkExperienceFile] = useState<File | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [acceptedTerms, setAcceptedTerms] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!acceptedTerms) {
      toast.error('You must agree to the Terms of Service and Privacy Policy to continue');
      return;
    }

    if (formData.password !== formData.confirmPassword) {
      toast.error('Passwords do not match');
      return;
    }

    if (accountType !== 'Client') {
      if (!formData.professionalLicense || !formData.yearsOfExperience) {
        toast.error('Please fill in all professional fields');
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
    }

    setIsLoading(true);

    try {
      const years = formData.yearsOfExperience ? parseInt(formData.yearsOfExperience, 10) : undefined;

      if (accountType !== 'Client') {
        const payload = new FormData();
        const hasPortfolio = formData.portfolio.trim().length > 0;
        payload.append('email', formData.email);
        payload.append('password', formData.password);
        payload.append('firstName', formData.firstName);
        payload.append('lastName', formData.lastName);
        payload.append('phoneNumber', formData.phoneNumber);
        payload.append('role', accountType);
        payload.append('professionalLicense', formData.professionalLicense);
        if (!Number.isNaN(years) && years !== undefined) {
          payload.append('yearsOfExperience', String(years));
        }
        if (hasPortfolio) {
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

        await api.post<AuthResponse>('/auth/register-professional', payload);

        const targetLogin = accountType === 'Engineer' ? '/engineer/login' : '/architect/login';
        toast.success(`${accountType} application submitted! Admin will review your account.`, {
          duration: 5000,
        });
        router.push(targetLogin);
        return;
      }

      const response = await register({
        email: formData.email,
        password: formData.password,
        firstName: formData.firstName,
        lastName: formData.lastName,
        phoneNumber: formData.phoneNumber,
        role: undefined,
      });

      toast.success('Registration successful!');
      router.push('/designs');
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
    <Layout>
      <Toaster position="top-right" />
      <div className="min-h-[80vh] flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
        <div className="max-w-2xl w-full space-y-8">
          <div>
            <h2 className="mt-6 text-center text-3xl font-extrabold text-gray-900">
              Create your account
            </h2>
            <p className="mt-2 text-center text-sm text-gray-600">
              Already have an account?{' '}
              <Link href="/login" className="font-medium text-blue-600 hover:text-blue-500">
                Sign in
              </Link>
            </p>
          </div>

          {/* Account Type Selection */}
          <div className="bg-gray-50 rounded-lg p-4">
            <label className="block text-sm font-medium text-gray-700 mb-3">
              I want to register as:
            </label>
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
              <button
                type="button"
                onClick={() => setAccountType('Client')}
                className={`p-4 border-2 rounded-lg text-left transition ${
                  accountType === 'Client'
                    ? 'border-blue-600 bg-blue-50'
                    : 'border-gray-300 hover:border-gray-400'
                }`}
              >
                <h3 className="font-semibold text-gray-900">Client</h3>
                <p className="text-sm text-gray-600 mt-1">
                  Browse and purchase architectural designs
                </p>
              </button>
              <button
                type="button"
                onClick={() => setAccountType('Architect')}
                className={`p-4 border-2 rounded-lg text-left transition ${
                  accountType === 'Architect'
                    ? 'border-blue-600 bg-blue-50'
                    : 'border-gray-300 hover:border-gray-400'
                }`}
              >
                <h3 className="font-semibold text-gray-900">Architect</h3>
                <p className="text-sm text-gray-600 mt-1">
                  Upload and sell your designs on our platform
                </p>
              </button>
              <button
                type="button"
                onClick={() => setAccountType('Engineer')}
                className={`p-4 border-2 rounded-lg text-left transition ${
                  accountType === 'Engineer'
                    ? 'border-emerald-600 bg-emerald-50'
                    : 'border-gray-300 hover:border-gray-400'
                }`}
              >
                <h3 className="font-semibold text-gray-900">Engineer</h3>
                <p className="text-sm text-gray-600 mt-1">
                  Verify structural drawings and BOQs
                </p>
              </button>
            </div>
          </div>

          <form className="mt-8 space-y-6" onSubmit={handleSubmit}>
            <div className="rounded-md shadow-sm space-y-4">
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
                    placeholder="First Name"
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
                    placeholder="Last Name"
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
                  autoComplete="email"
                  required
                  value={formData.email}
                  onChange={handleChange}
                  className="appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                  placeholder="Email address"
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
                  placeholder="Phone Number (e.g., +[country code] [number])"
                />
              </div>

              {/* Architect-specific fields */}
              {accountType !== 'Client' && (
                <>
                  <div className="border-t pt-4 mt-4">
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
                          required={accountType !== 'Client'}
                          value={formData.professionalLicense}
                          onChange={handleChange}
                          className="appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                          placeholder="Professional License Number"
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
                          required={accountType !== 'Client'}
                          value={formData.yearsOfExperience}
                          onChange={handleChange}
                          className="appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                          placeholder="Years of professional experience"
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
                                required={accountType !== 'Client' && !formData.portfolio.trim()}
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
                                required={accountType !== 'Client' && !formData.portfolio.trim()}
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
                                required={accountType !== 'Client' && !formData.portfolio.trim()}
                                onChange={handleFileChange(setWorkExperienceFile)}
                                className="block w-full text-sm text-gray-700"
                              />
                            </div>
                          </div>
                        </div>
                      )}
                    </div>
                  </div>
                </>
              )}

              <div>
                <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-1">
                  Password *
                </label>
                <input
                  id="password"
                  name="password"
                  type="password"
                  autoComplete="new-password"
                  required
                  value={formData.password}
                  onChange={handleChange}
                  className="appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                  placeholder="Password (min. 8 characters)"
                />
              </div>

              <div>
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
                  placeholder="Confirm Password"
                />
              </div>
            </div>

            {accountType !== 'Client' && (
              <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                <p className="text-sm text-blue-800">
                  <strong>Note:</strong> Your professional account will be reviewed by our management team.
                  You'll receive an email once your account is approved.
                </p>
              </div>
            )}

            {/* Terms and Conditions Checkbox */}
            <div className="flex items-start">
              <div className="flex items-center h-5">
                <input
                  id="acceptedTerms"
                  name="acceptedTerms"
                  type="checkbox"
                  checked={acceptedTerms}
                  onChange={(e) => setAcceptedTerms(e.target.checked)}
                  className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                  required
                />
              </div>
              <div className="ml-3 text-sm">
                <label htmlFor="acceptedTerms" className="text-gray-700">
                  I agree to the{' '}
                  <Link
                    href="/terms-of-service"
                    target="_blank"
                    className="text-blue-600 hover:text-blue-700 underline"
                  >
                    Terms of Service
                  </Link>
                  {' '}and{' '}
                  <Link
                    href="/privacy-policy"
                    target="_blank"
                    className="text-blue-600 hover:text-blue-700 underline"
                  >
                    Privacy Policy
                  </Link>
                  {' *'}
                </label>
              </div>
            </div>

            <div>
              <button
                type="submit"
                disabled={isLoading}
                className="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:bg-gray-400"
              >
                {isLoading ? 'Creating account...' : accountType !== 'Client' ? 'Submit Application' : 'Create account'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </Layout>
  );
}
