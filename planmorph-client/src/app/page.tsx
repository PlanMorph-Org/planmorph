'use client';

import Layout from '../components/Layout';
import Link from 'next/link';
import { useEffect, useState } from 'react';
import api from '../lib/api';
import { Design } from '../types';
import { useCurrencyStore } from '../store/currencyStore';
import { formatCurrency } from '../lib/currency';

export default function Home() {
  const [featuredDesigns, setFeaturedDesigns] = useState<Design[]>([]);
  const { currency, rates } = useCurrencyStore();

  useEffect(() => {
    loadFeaturedDesigns();
  }, []);

  const loadFeaturedDesigns = async () => {
    try {
      const response = await api.get<Design[]>('/designs');
      setFeaturedDesigns(response.data.slice(0, 3));
    } catch (error) {
      console.error('Failed to load designs:', error);
    }
  };

  return (
    <Layout>
      {/* Hero Section */}
      <section className="bg-gradient-to-r from-blue-600 to-blue-800 text-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-24">
          <div className="text-center">
            <h1 className="text-4xl md:text-6xl font-bold mb-6">
              Build Your Dream Home Anywhere
            </h1>
            <p className="text-xl md:text-2xl mb-8 text-blue-100">
              Browse professional architectural designs from top talent worldwide.
              Purchase plans instantly. Construction support available in Kenya.
            </p>
            <div className="flex justify-center space-x-4">
              <Link
                href="/designs"
                className="px-8 py-3 bg-white text-blue-600 font-semibold rounded-lg hover:bg-gray-100 transition"
              >
                Browse Designs
              </Link>
              <Link
                href="/about"
                className="px-8 py-3 border-2 border-white text-white font-semibold rounded-lg hover:bg-white hover:text-blue-600 transition"
              >
                Learn More
              </Link>
            </div>
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section className="border-t border-gray-200">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
          <h2 className="text-3xl font-bold text-center mb-12">Why Choose PlanMorph?</h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            <div className="text-center p-6">
              <div className="w-16 h-16 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <svg className="w-8 h-8 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
              </div>
              <h3 className="text-xl font-semibold mb-2">Verified Designs</h3>
              <p className="text-gray-600">
                All designs are approved by licensed architects and engineers
              </p>
            </div>

            <div className="text-center p-6">
              <div className="w-16 h-16 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <svg className="w-8 h-8 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
                </svg>
              </div>
              <h3 className="text-xl font-semibold mb-2">Instant Download</h3>
              <p className="text-gray-600">
                Get your architectural and engineering drawings immediately after purchase
              </p>
            </div>

            <div className="text-center p-6">
              <div className="w-16 h-16 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <svg className="w-8 h-8 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                </svg>
              </div>
              <h3 className="text-xl font-semibold mb-2">Construction Support</h3>
              <p className="text-gray-600">
                Get connected with trusted contractors for project execution (Kenya only for now)
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* Featured Designs */}
      <section className="bg-gray-100 py-16 border-t border-gray-200">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <h2 className="text-3xl font-bold text-center mb-12">Featured Designs</h2>
          
          {featuredDesigns.length === 0 ? (
            <div className="text-center text-gray-600">
              <p>No designs available yet. Check back soon!</p>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
              {featuredDesigns.map((design) => (
                <Link
                  key={design.id}
                  href={`/designs/${design.id}`}
                  className="bg-white rounded-lg shadow-md overflow-hidden hover:shadow-xl transition"
                >
                  <div className="h-48 bg-gray-300 flex items-center justify-center">
                    {design.previewImages.length > 0 ? (
                      <img
                        src={design.previewImages[0]}
                        alt={design.title}
                        className="w-full h-full object-cover"
                      />
                    ) : (
                      <span className="text-gray-500">No Image</span>
                    )}
                  </div>
                  <div className="p-6">
                    <h3 className="text-xl font-semibold mb-2">{design.title}</h3>
                    <p className="text-gray-600 mb-4 line-clamp-2">{design.description}</p>
                    <div className="flex justify-between items-center">
                      <div className="text-sm text-gray-500">
                        {design.bedrooms} BR • {design.bathrooms} BA
                      </div>
                      <div className="text-xl font-bold text-blue-600">
                        {formatCurrency(design.price, currency, rates)}
                      </div>
                    </div>
                  </div>
                </Link>
              ))}
            </div>
          )}

          <div className="text-center mt-12">
            <Link
              href="/designs"
              className="inline-block px-8 py-3 bg-blue-600 text-white font-semibold rounded-lg hover:bg-blue-700 transition"
            >
              View All Designs
            </Link>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="bg-blue-600 text-white py-16 border-t border-blue-700">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
          <h2 className="text-3xl font-bold mb-4">Ready to Start Building?</h2>
          <p className="text-xl mb-8 text-blue-100">
            Join thousands of satisfied customers who built their dream homes with PlanMorph worldwide
          </p>
          <Link
            href="/register"
            className="inline-block px-8 py-3 bg-white text-blue-600 font-semibold rounded-lg hover:bg-gray-100 transition"
          >
            Get Started Today
          </Link>
        </div>
      </section>

      {/* Architect Section */}
      <section className="bg-gray-900 text-white py-16">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <h2 className="text-3xl font-bold mb-4">Are You an Architect?</h2>
            <p className="text-xl text-gray-300 mb-8">
              Join our platform and earn money by selling your architectural designs to thousands of potential clients
            </p>
            <div className="flex justify-center space-x-4">
              <Link
                href="/architect/register"
                className="inline-block px-8 py-3 bg-white text-gray-900 font-semibold rounded-lg hover:bg-gray-100 transition"
              >
                Apply as Architect
              </Link>
              <Link
                href="/architect/login"
                className="inline-block px-8 py-3 border-2 border-white text-white font-semibold rounded-lg hover:bg-white hover:text-gray-900 transition"
              >
                Architect Login
              </Link>
            </div>
          </div>
        </div>
      </section>

      {/* Engineer Section */}
      <section className="bg-emerald-700 text-white py-16 border-t border-emerald-800">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <h2 className="text-3xl font-bold mb-4">Are You an Engineer?</h2>
            <p className="text-xl text-emerald-100 mb-8">
              Help verify structural integrity and BOQs for high-quality architectural projects
            </p>
            <div className="flex justify-center space-x-4">
              <Link
                href="/engineer/register"
                className="inline-block px-8 py-3 bg-white text-emerald-700 font-semibold rounded-lg hover:bg-emerald-50 transition"
              >
                Apply as Engineer
              </Link>
              <Link
                href="/engineer/login"
                className="inline-block px-8 py-3 border-2 border-white text-white font-semibold rounded-lg hover:bg-white hover:text-emerald-700 transition"
              >
                Engineer Login
              </Link>
            </div>
          </div>
        </div>
      </section>
    </Layout>
  );
}
