'use client';

import { useEffect, useState } from 'react';
import Layout from '@/src/components/Layout';
import Link from 'next/link';
import api from '@/src/lib/api';
import { Design, DesignFilter } from '@/src/types';
import toast, { Toaster } from 'react-hot-toast';
import { useCurrencyStore } from '@/src/store/currencyStore';
import { formatCurrency } from '@/src/lib/currency';

export default function DesignsPage() {
  const [designs, setDesigns] = useState<Design[]>([]);
  const [filteredDesigns, setFilteredDesigns] = useState<Design[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [showFilters, setShowFilters] = useState(false);
  const [filters, setFilters] = useState<DesignFilter>({});
  const { currency, rates } = useCurrencyStore();

  useEffect(() => {
    loadDesigns();
  }, []);

  const loadDesigns = async () => {
    setIsLoading(true);
    try {
      const response = await api.get<Design[]>('/designs');
      setDesigns(response.data);
      setFilteredDesigns(response.data);
    } catch (error) {
      toast.error('Failed to load designs');
    } finally {
      setIsLoading(false);
    }
  };

  const applyFilters = () => {
    let filtered = [...designs];

    if (filters.minBedrooms) {
      filtered = filtered.filter(d => d.bedrooms >= filters.minBedrooms!);
    }
    if (filters.maxBedrooms) {
      filtered = filtered.filter(d => d.bedrooms <= filters.maxBedrooms!);
    }
    if (filters.minPrice) {
      filtered = filtered.filter(d => d.price >= filters.minPrice!);
    }
    if (filters.maxPrice) {
      filtered = filtered.filter(d => d.price <= filters.maxPrice!);
    }
    if (filters.category) {
      filtered = filtered.filter(d => d.category === filters.category);
    }
    if (filters.stories) {
      filtered = filtered.filter(d => d.stories === filters.stories);
    }

    setFilteredDesigns(filtered);
    setShowFilters(false);
  };

  const clearFilters = () => {
    setFilters({});
    setFilteredDesigns(designs);
  };

  return (
    <Layout>
      <Toaster position="top-right" />
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="flex justify-between items-center mb-2">
          <h1 className="text-3xl font-bold text-gray-900">Browse Designs</h1>
          <button
            onClick={() => setShowFilters(!showFilters)}
            className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-50"
          >
            {showFilters ? 'Hide Filters' : 'Show Filters'}
          </button>
        </div>
        <p className="text-sm text-gray-500 mb-8">
          Prices shown in {currency}. Billing currency is KES.
        </p>

        {/* Filters Panel */}
        {showFilters && (
          <div className="bg-white p-6 rounded-lg shadow-md mb-8">
            <h3 className="text-lg font-semibold mb-4">Filter Designs</h3>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Bedrooms
                </label>
                <div className="grid grid-cols-2 gap-2">
                  <input
                    type="number"
                    placeholder="Min"
                    value={filters.minBedrooms || ''}
                    onChange={(e) => setFilters({ ...filters, minBedrooms: Number(e.target.value) || undefined })}
                    className="px-3 py-2 border border-gray-300 rounded-md text-sm"
                  />
                  <input
                    type="number"
                    placeholder="Max"
                    value={filters.maxBedrooms || ''}
                    onChange={(e) => setFilters({ ...filters, maxBedrooms: Number(e.target.value) || undefined })}
                    className="px-3 py-2 border border-gray-300 rounded-md text-sm"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Price (KES base)
                </label>
                <div className="grid grid-cols-2 gap-2">
                  <input
                    type="number"
                    placeholder="Min"
                    value={filters.minPrice || ''}
                    onChange={(e) => setFilters({ ...filters, minPrice: Number(e.target.value) || undefined })}
                    className="px-3 py-2 border border-gray-300 rounded-md text-sm"
                  />
                  <input
                    type="number"
                    placeholder="Max"
                    value={filters.maxPrice || ''}
                    onChange={(e) => setFilters({ ...filters, maxPrice: Number(e.target.value) || undefined })}
                    className="px-3 py-2 border border-gray-300 rounded-md text-sm"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Category
                </label>
                <select
                  value={filters.category || ''}
                  onChange={(e) => setFilters({ ...filters, category: e.target.value || undefined })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm"
                >
                  <option value="">All Categories</option>
                  <option value="Bungalow">Bungalow</option>
                  <option value="TwoStory">Two Story</option>
                  <option value="Mansion">Mansion</option>
                  <option value="Apartment">Apartment</option>
                  <option value="Commercial">Commercial</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Stories
                </label>
                <select
                  value={filters.stories || ''}
                  onChange={(e) => setFilters({ ...filters, stories: Number(e.target.value) || undefined })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm"
                >
                  <option value="">Any</option>
                  <option value="1">1 Story</option>
                  <option value="2">2 Stories</option>
                  <option value="3">3 Stories</option>
                </select>
              </div>
            </div>

            <div className="mt-4 flex space-x-4">
              <button
                onClick={applyFilters}
                className="px-6 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
              >
                Apply Filters
              </button>
              <button
                onClick={clearFilters}
                className="px-6 py-2 border border-gray-300 text-gray-700 rounded-md hover:bg-gray-50"
              >
                Clear Filters
              </button>
            </div>
          </div>
        )}

        {/* Designs Grid */}
        {isLoading ? (
          <div className="flex justify-center items-center h-64">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
          </div>
        ) : filteredDesigns.length === 0 ? (
          <div className="text-center py-12">
            <p className="text-gray-500 text-lg">No designs found matching your criteria.</p>
          </div>
        ) : (
          <>
            <div className="mb-4 text-sm text-gray-600">
              Showing {filteredDesigns.length} of {designs.length} designs
            </div>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {filteredDesigns.map((design) => (
                <Link
                  key={design.id}
                  href={`/designs/${design.id}`}
                  className="bg-white rounded-lg shadow-md overflow-hidden hover:shadow-xl transition-shadow"
                >
                  <div className="h-48 bg-gray-200 flex items-center justify-center">
                    {design.previewImages.length > 0 ? (
                      <img
                        src={design.previewImages[0]}
                        alt={design.title}
                        className="w-full h-full object-cover"
                      />
                    ) : (
                      <span className="text-gray-400">No Preview</span>
                    )}
                  </div>
                  <div className="p-6">
                    <div className="flex justify-between items-start mb-2">
                      <h3 className="text-xl font-semibold text-gray-900">{design.title}</h3>
                      <span className="px-2 py-1 text-xs font-semibold text-blue-600 bg-blue-100 rounded">
                        {design.category}
                      </span>
                    </div>
                    <p className="text-gray-600 text-sm mb-4 line-clamp-2">{design.description}</p>
                    <div className="flex justify-between items-center text-sm text-gray-500 mb-4">
                      <span>{design.bedrooms} Bedrooms</span>
                      <span>{design.bathrooms} Bathrooms</span>
                      <span>{design.squareFootage.toLocaleString()} sqft</span>
                    </div>
                    <div className="flex justify-between items-center pt-4 border-t border-gray-200">
                      <div className="text-2xl font-bold text-blue-600">
                        {formatCurrency(design.price, currency, rates)}
                      </div>
                      <button className="px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded hover:bg-blue-700">
                        View Details
                      </button>
                    </div>
                  </div>
                </Link>
              ))}
            </div>
          </>
        )}
      </div>
    </Layout>
  );
}
