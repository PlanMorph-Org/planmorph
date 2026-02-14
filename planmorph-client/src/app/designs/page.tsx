'use client';

import Layout from '../../components/Layout';
import { useState, useEffect, useCallback, useRef } from 'react';
import Link from 'next/link';
import api from '../../lib/api';
import { Design } from '../../types';
import { useCurrencyStore } from '../../store/currencyStore';
import { formatCurrency } from '../../lib/currency';
import { CheckBadgeIcon, MagnifyingGlassIcon, FunnelIcon } from '@heroicons/react/24/solid';
import { motion, useInView, AnimatePresence } from 'framer-motion';

const fadeUp = {
  hidden: { opacity: 0, y: 24 },
  visible: (i: number = 0) => ({
    opacity: 1, y: 0,
    transition: { delay: i * 0.06, duration: 0.5, ease: [0.16, 1, 0.3, 1] as const },
  }),
};

const stagger = { visible: { transition: { staggerChildren: 0.06 } } };

function AnimatedGrid({ children, className = '' }: { children: React.ReactNode; className?: string }) {
  const ref = useRef(null);
  const inView = useInView(ref, { once: true, margin: '-40px' });
  return (
    <motion.div ref={ref} initial="hidden" animate={inView ? 'visible' : 'hidden'} variants={stagger} className={className}>
      {children}
    </motion.div>
  );
}

const categories = ['All', 'Residential', 'Commercial', 'Multi-unit', 'Mixed-use'];
const bedroomOptions = [1, 2, 3, 4, 5];
const storyOptions = [1, 2, 3];

export default function DesignsPage() {
  const [designs, setDesigns] = useState<Design[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [category, setCategory] = useState('All');
  const [minBedrooms, setMinBedrooms] = useState(0);
  const [maxPrice, setMaxPrice] = useState(0);
  const [stories, setStories] = useState(0);
  const [search, setSearch] = useState('');
  const [showFilters, setShowFilters] = useState(false);
  const { currency, rates } = useCurrencyStore();

  const loadDesigns = useCallback(async () => {
    setIsLoading(true);
    try {
      const params: Record<string, string | number> = {};
      if (category !== 'All') params.category = category;
      if (minBedrooms > 0) params.minBedrooms = minBedrooms;
      if (maxPrice > 0) params.maxPrice = maxPrice;
      if (stories > 0) params.stories = stories;
      const response = await api.get<Design[]>('/designs', { params });
      setDesigns(response.data);
    } catch (error) {
      console.error('Failed to load designs:', error);
    } finally {
      setIsLoading(false);
    }
  }, [category, minBedrooms, maxPrice, stories]);

  useEffect(() => {
    loadDesigns();
  }, [loadDesigns]);

  const filteredDesigns = designs.filter((d) =>
    search === '' || d.title.toLowerCase().includes(search.toLowerCase())
  );

  const SkeletonCard = () => (
    <div className="glass-card rounded-xl overflow-hidden">
      <div className="h-48 shimmer-bg" />
      <div className="p-5 space-y-3">
        <div className="h-4 w-3/4 shimmer-bg rounded" />
        <div className="h-3 w-1/2 shimmer-bg rounded" />
        <div className="h-5 w-1/3 shimmer-bg rounded" />
      </div>
    </div>
  );

  return (
    <Layout>
      {/* Header */}
      <section className="relative py-16 border-b border-white/6 overflow-hidden">
        <div className="absolute inset-0 bg-hero-gradient" />
        <div className="absolute inset-0 blueprint-grid opacity-20" />
        <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <motion.h1
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="text-3xl md:text-4xl font-display font-bold text-white tracking-tight mb-3"
          >
            Browse Packages
          </motion.h1>
          <motion.p
            initial={{ opacity: 0, y: 16 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.1 }}
            className="text-white/40 text-sm"
          >
            Engineer-reviewed, build-ready packages published by verified professionals.
          </motion.p>
        </div>
      </section>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
        {/* Filter Bar */}
        <motion.div
          initial={{ opacity: 0, y: 12 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.15 }}
          className="glass-card rounded-xl p-4 mb-8"
        >
          <div className="flex flex-col md:flex-row gap-4">
            {/* Search */}
            <div className="relative flex-1">
              <MagnifyingGlassIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-white/30" />
              <input
                type="text"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder="Search packages..."
                className="w-full pl-10 pr-4 py-2.5 glass-input rounded-lg text-sm text-white placeholder:text-white/25"
              />
            </div>

            {/* Category pills */}
            <div className="flex flex-wrap gap-2">
              {categories.map((cat) => (
                <button
                  key={cat}
                  onClick={() => setCategory(cat)}
                  className={`px-3.5 py-1.5 text-xs font-medium rounded-full transition-all duration-300 ${category === cat
                    ? 'bg-brand-accent text-white shadow-blue'
                    : 'text-white/40 hover:text-white bg-white/5 hover:bg-white/10'
                    }`}
                >
                  {cat}
                </button>
              ))}
            </div>

            {/* Toggle more filters */}
            <button
              onClick={() => setShowFilters(!showFilters)}
              className="inline-flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium text-white/40 hover:text-white bg-white/5 hover:bg-white/10 rounded-full transition-all duration-300"
            >
              <FunnelIcon className="w-3.5 h-3.5" />
              Filters
            </button>
          </div>

          {/* Extended filters */}
          <AnimatePresence>
            {showFilters && (
              <motion.div
                initial={{ height: 0, opacity: 0 }}
                animate={{ height: 'auto', opacity: 1 }}
                exit={{ height: 0, opacity: 0 }}
                transition={{ duration: 0.3, ease: [0.16, 1, 0.3, 1] }}
                className="overflow-hidden"
              >
                <div className="pt-4 mt-4 border-t border-white/6 grid grid-cols-1 sm:grid-cols-3 gap-4">
                  <div>
                    <label className="block text-xs text-white/30 mb-1.5">Min Bedrooms</label>
                    <select
                      value={minBedrooms}
                      onChange={(e) => setMinBedrooms(Number(e.target.value))}
                      className="w-full glass-input rounded-lg text-sm text-white py-2 px-3 bg-transparent"
                    >
                      <option value={0} className="bg-brand-light">Any</option>
                      {bedroomOptions.map((b) => (
                        <option key={b} value={b} className="bg-brand-light">{b}+</option>
                      ))}
                    </select>
                  </div>
                  <div>
                    <label className="block text-xs text-white/30 mb-1.5">Max Price (KES)</label>
                    <input
                      type="number"
                      value={maxPrice || ''}
                      onChange={(e) => setMaxPrice(Number(e.target.value))}
                      placeholder="No limit"
                      className="w-full glass-input rounded-lg text-sm text-white py-2 px-3 placeholder:text-white/20"
                    />
                  </div>
                  <div>
                    <label className="block text-xs text-white/30 mb-1.5">Stories</label>
                    <select
                      value={stories}
                      onChange={(e) => setStories(Number(e.target.value))}
                      className="w-full glass-input rounded-lg text-sm text-white py-2 px-3 bg-transparent"
                    >
                      <option value={0} className="bg-brand-light">Any</option>
                      {storyOptions.map((s) => (
                        <option key={s} value={s} className="bg-brand-light">{s}</option>
                      ))}
                    </select>
                  </div>
                </div>
              </motion.div>
            )}
          </AnimatePresence>
        </motion.div>

        {/* Results count */}
        {!isLoading && (
          <p className="text-xs text-white/25 mb-6">
            {filteredDesigns.length} package{filteredDesigns.length !== 1 ? 's' : ''} found
          </p>
        )}

        {/* Skeleton loading */}
        {isLoading ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
            {[...Array(8)].map((_, i) => <SkeletonCard key={i} />)}
          </div>
        ) : filteredDesigns.length === 0 ? (
          <div className="text-center py-20">
            <div className="w-16 h-16 mx-auto mb-4 glass-card rounded-full flex items-center justify-center">
              <MagnifyingGlassIcon className="w-7 h-7 text-white/20" />
            </div>
            <p className="text-white/40 mb-2">No packages found</p>
            <p className="text-white/25 text-sm">Try adjusting your filters or search term.</p>
          </div>
        ) : (
          <AnimatedGrid className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
            {filteredDesigns.map((design, i) => (
              <motion.div key={design.id} variants={fadeUp} custom={i}>
                <Link
                  href={`/designs/${design.id}`}
                  className="group glass-card rounded-xl overflow-hidden card-hover block"
                >
                  <div className="relative h-48 bg-brand-light overflow-hidden">
                    {design.previewImages.length > 0 ? (
                      <img
                        src={design.previewImages[0]}
                        alt={design.title}
                        className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-110"
                      />
                    ) : (
                      <div className="w-full h-full flex items-center justify-center text-white/15">
                        No Preview
                      </div>
                    )}
                    <div className="absolute inset-0 bg-gradient-to-t from-brand/60 to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300" />
                    <div className="absolute top-3 right-3 flex items-center gap-1 glass-card text-verified text-xs font-semibold px-2.5 py-1 rounded-full">
                      <CheckBadgeIcon className="h-3.5 w-3.5" />
                      Verified
                    </div>
                  </div>
                  <div className="p-5">
                    <h3 className="text-sm font-semibold text-white mb-1 group-hover:text-brand-accent transition-colors duration-300 line-clamp-2">
                      {design.title}
                    </h3>
                    <p className="text-xs text-white/30 font-mono mb-3">
                      {design.bedrooms} bed &middot; {design.bathrooms} bath &middot; {design.squareFootage?.toLocaleString()} sqft
                    </p>
                    <div className="text-lg font-semibold text-gradient-golden">
                      {formatCurrency(design.price, currency, rates)}
                    </div>
                  </div>
                </Link>
              </motion.div>
            ))}
          </AnimatedGrid>
        )}
      </div>
    </Layout>
  );
}
