'use client';

import Layout from '../components/Layout';
import Link from 'next/link';
import { useEffect, useState, useRef } from 'react';
import api from '../lib/api';
import { Design } from '../types';
import { useCurrencyStore } from '../store/currencyStore';
import { formatCurrency } from '../lib/currency';
import { ShieldCheckIcon, DocumentCheckIcon, ArrowDownTrayIcon, UserGroupIcon } from '@heroicons/react/24/outline';
import { CheckBadgeIcon } from '@heroicons/react/24/solid';
import { motion, useInView } from 'framer-motion';

/* ────── Animation helpers ────── */
const fadeUp = {
  hidden: { opacity: 0, y: 30 },
  visible: (i: number = 0) => ({
    opacity: 1,
    y: 0,
    transition: { delay: i * 0.1, duration: 0.6, ease: [0.16, 1, 0.3, 1] as const },
  }),
};

const stagger = {
  visible: { transition: { staggerChildren: 0.08 } },
};

function AnimatedSection({ children, className = '' }: { children: React.ReactNode; className?: string }) {
  const ref = useRef(null);
  const inView = useInView(ref, { once: true, margin: '-60px' });
  return (
    <motion.section
      ref={ref}
      initial="hidden"
      animate={inView ? 'visible' : 'hidden'}
      variants={stagger}
      className={className}
    >
      {children}
    </motion.section>
  );
}

/* ────── Floating Blueprint Particles ────── */
function BlueprintParticles() {
  return (
    <div className="absolute inset-0 overflow-hidden pointer-events-none">
      {/* Grid pattern */}
      <div className="absolute inset-0 blueprint-grid animate-grid-reveal" />
      {/* Floating geometric shapes */}
      {[...Array(6)].map((_, i) => (
        <div
          key={i}
          className="absolute opacity-[0.06]"
          style={{
            left: `${15 + i * 15}%`,
            top: `${20 + (i % 3) * 25}%`,
            animation: `float ${5 + i * 0.8}s ease-in-out ${i * 0.5}s infinite`,
          }}
        >
          {i % 3 === 0 ? (
            <svg width="40" height="40" viewBox="0 0 40 40" fill="none">
              <rect x="2" y="2" width="36" height="36" stroke="currentColor" strokeWidth="1" className="text-brand-accent" />
              <line x1="2" y1="20" x2="38" y2="20" stroke="currentColor" strokeWidth="0.5" className="text-brand-accent" />
              <line x1="20" y1="2" x2="20" y2="38" stroke="currentColor" strokeWidth="0.5" className="text-brand-accent" />
            </svg>
          ) : i % 3 === 1 ? (
            <svg width="50" height="50" viewBox="0 0 50 50" fill="none">
              <polygon points="25,2 48,48 2,48" stroke="currentColor" strokeWidth="1" fill="none" className="text-golden" />
            </svg>
          ) : (
            <svg width="44" height="44" viewBox="0 0 44 44" fill="none">
              <circle cx="22" cy="22" r="20" stroke="currentColor" strokeWidth="1" className="text-brand-accent" />
              <line x1="22" y1="2" x2="22" y2="42" stroke="currentColor" strokeWidth="0.5" className="text-brand-accent" />
              <line x1="2" y1="22" x2="42" y2="22" stroke="currentColor" strokeWidth="0.5" className="text-brand-accent" />
            </svg>
          )}
        </div>
      ))}
    </div>
  );
}

/* ────── Main Page ────── */
export default function Home() {
  const [featuredDesigns, setFeaturedDesigns] = useState<Design[]>([]);
  const { currency, rates } = useCurrencyStore();

  useEffect(() => {
    loadFeaturedDesigns();
  }, []);

  const loadFeaturedDesigns = async () => {
    try {
      const response = await api.get<Design[]>('/designs');
      setFeaturedDesigns(response.data.slice(0, 4));
    } catch (error) {
      console.error('Failed to load designs:', error);
    }
  };

  const trustSignals = [
    {
      icon: ShieldCheckIcon,
      title: 'Engineer-Reviewed',
      description: 'Structural integrity is reviewed before a design is published',
      color: 'from-brand-accent/20 to-blue-500/10',
      iconColor: 'text-brand-accent',
    },
    {
      icon: DocumentCheckIcon,
      title: 'Complete Packages',
      description: 'Architectural drawings, structural plans, and BOQ in one purchase',
      color: 'from-golden/20 to-amber-500/10',
      iconColor: 'text-golden',
    },
    {
      icon: ArrowDownTrayIcon,
      title: 'Instant Download',
      description: 'Access all documents immediately after purchase',
      color: 'from-verified/20 to-emerald-500/10',
      iconColor: 'text-verified',
    },
    {
      icon: UserGroupIcon,
      title: 'Vetted Professionals',
      description: 'Professionals are reviewed before they can sell on PlanMorph',
      color: 'from-purple-500/20 to-violet-500/10',
      iconColor: 'text-purple-400',
    },
  ];

  return (
    <Layout>
      {/* ═══════════ HERO ═══════════ */}
      <section className="relative min-h-[90vh] flex items-center overflow-hidden">
        {/* Background layers */}
        <div className="absolute inset-0 bg-hero-gradient" />
        <div className="absolute inset-0 bg-gradient-to-b from-brand-accent/5 via-transparent to-transparent" />
        <BlueprintParticles />

        {/* Decorative structural beam */}
        <div className="absolute top-0 left-0 right-0 h-1 bg-gradient-to-r from-transparent via-brand-accent/30 to-transparent" />

        {/* Radial glow */}
        <div className="absolute top-1/3 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[800px] h-[800px] bg-brand-accent/[0.03] rounded-full blur-3xl" />

        <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-28 lg:py-40 w-full">
          <div className="max-w-3xl">
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.5, ease: [0.16, 1, 0.3, 1] }}
              className="inline-flex items-center gap-2 px-4 py-1.5 rounded-full bg-white/5 border border-white/8 text-xs font-medium text-white/60 mb-8"
            >
              <span className="w-1.5 h-1.5 rounded-full bg-verified animate-pulse" />
              Verified plan sets from licensed professionals
            </motion.div>

            <motion.h1
              initial={{ opacity: 0, y: 30 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.15, duration: 0.7, ease: [0.16, 1, 0.3, 1] }}
              className="text-4xl md:text-5xl lg:text-7xl font-display font-bold tracking-tight leading-[1.08] mb-7"
            >
              Every building
              <br />
              deserves a{' '}
              <span className="text-gradient-golden">verified</span> plan.
            </motion.h1>

            <motion.p
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.3, duration: 0.6, ease: [0.16, 1, 0.3, 1] }}
              className="text-lg md:text-xl text-white/40 mb-12 leading-relaxed max-w-2xl"
            >
              Buy build-ready plan sets you can actually act on: architectural drawings, structural plans, and BOQ—delivered instantly after purchase.
            </motion.p>

            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.45, duration: 0.6, ease: [0.16, 1, 0.3, 1] }}
              className="flex flex-wrap gap-4"
            >
              <Link
                href="/designs"
                className="group px-8 py-3.5 bg-brand-accent text-white font-semibold rounded-xl hover:bg-blue-500 transition-all duration-300 shadow-blue btn-glow inline-flex items-center gap-2"
              >
                Browse Verified Designs
                <svg className="w-4 h-4 transition-transform duration-300 group-hover:translate-x-1" viewBox="0 0 16 16" fill="none">
                  <path d="M3 8h10M9 4l4 4-4 4" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
                </svg>
              </Link>
              <Link
                href="/architect/register"
                className="px-8 py-3.5 text-white/50 hover:text-white font-medium transition-all duration-300 inline-flex items-center gap-2 hover:bg-white/5 rounded-xl"
              >
                Sell as a Professional
                <span aria-hidden="true">&rarr;</span>
              </Link>
            </motion.div>
          </div>

          {/* Decorative architecture illustration (right side on desktop) */}
          <motion.div
            initial={{ opacity: 0, scale: 0.9 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ delay: 0.6, duration: 1, ease: [0.16, 1, 0.3, 1] }}
            className="hidden lg:block absolute right-8 top-1/2 -translate-y-1/2"
          >
            <div className="relative w-80 h-80">
              {/* Structural wireframe */}
              <svg viewBox="0 0 320 320" fill="none" className="w-full h-full opacity-[0.08]">
                {/* Building outline */}
                <rect x="80" y="60" width="160" height="220" stroke="currentColor" strokeWidth="1" className="text-brand-accent" />
                <rect x="100" y="80" width="50" height="40" stroke="currentColor" strokeWidth="0.5" className="text-golden" />
                <rect x="170" y="80" width="50" height="40" stroke="currentColor" strokeWidth="0.5" className="text-golden" />
                <rect x="100" y="140" width="50" height="40" stroke="currentColor" strokeWidth="0.5" className="text-golden" />
                <rect x="170" y="140" width="50" height="40" stroke="currentColor" strokeWidth="0.5" className="text-golden" />
                <rect x="130" y="220" width="60" height="60" stroke="currentColor" strokeWidth="0.5" className="text-golden" />
                {/* Roof */}
                <polygon points="60,60 160,10 260,60" stroke="currentColor" strokeWidth="1" fill="none" className="text-brand-accent" />
                {/* Foundation */}
                <line x1="60" y1="280" x2="260" y2="280" stroke="currentColor" strokeWidth="1.5" className="text-brand-accent" />
                {/* Dimension lines */}
                <line x1="60" y1="295" x2="260" y2="295" stroke="currentColor" strokeWidth="0.5" strokeDasharray="4 4" className="text-steel" />
                <line x1="60" y1="290" x2="60" y2="300" stroke="currentColor" strokeWidth="0.5" className="text-steel" />
                <line x1="260" y1="290" x2="260" y2="300" stroke="currentColor" strokeWidth="0.5" className="text-steel" />
              </svg>
              {/* Glow behind */}
              <div className="absolute inset-0 bg-brand-accent/5 rounded-full blur-3xl" />
            </div>
          </motion.div>
        </div>

        {/* Bottom fade */}
        <div className="absolute bottom-0 left-0 right-0 h-32 bg-gradient-to-t from-brand to-transparent" />
      </section>

      {/* ═══════════ TRUST BAR ═══════════ */}
      <AnimatedSection className="relative py-16 border-y border-white/6">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-2 lg:grid-cols-4 gap-6">
            {trustSignals.map((signal, i) => (
              <motion.div
                key={signal.title}
                variants={fadeUp}
                custom={i}
                className="glass-card-light rounded-xl p-5 card-hover"
              >
                <div className={`w-10 h-10 rounded-lg bg-gradient-to-br ${signal.color} flex items-center justify-center mb-3`}>
                  <signal.icon className={`w-5 h-5 ${signal.iconColor}`} />
                </div>
                <h3 className="text-sm font-semibold text-white mb-1">{signal.title}</h3>
                <p className="text-xs text-white/40 leading-relaxed">{signal.description}</p>
              </motion.div>
            ))}
          </div>
        </div>
      </AnimatedSection>

      {/* ═══════════ HOW IT WORKS ═══════════ */}
      <AnimatedSection className="py-24 relative">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <motion.div variants={fadeUp} className="text-center mb-16">
            <h2 className="text-3xl md:text-4xl font-display font-bold text-white tracking-tight">
              Three steps to your{' '}
              <span className="text-gradient-blue">build-ready</span> plan.
            </h2>
          </motion.div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-8 relative">
            {/* Connecting line (desktop) */}
            <div className="hidden md:block absolute top-12 left-[16.67%] right-[16.67%] h-px">
              <motion.div
                initial={{ scaleX: 0 }}
                whileInView={{ scaleX: 1 }}
                viewport={{ once: true }}
                transition={{ delay: 0.5, duration: 1, ease: [0.16, 1, 0.3, 1] }}
                className="h-full bg-gradient-to-r from-brand-accent/30 via-golden/30 to-verified/30 origin-left"
              />
            </div>

            {[
              {
                step: '01',
                title: 'Browse verified designs',
                description: 'Explore build-ready designs from licensed architects, each verified by structural engineers.',
                gradient: 'from-brand-accent to-blue-400',
              },
              {
                step: '02',
                title: 'Purchase complete packages',
                description: 'Get instant access to architectural drawings, structural plans, and a bill of quantities.',
                gradient: 'from-golden to-amber-400',
              },
              {
                step: '03',
                title: 'Build with confidence',
                description: 'Use your complete, verified plan set to begin construction with certainty.',
                gradient: 'from-verified to-emerald-400',
              },
            ].map((item, i) => (
              <motion.div key={item.step} variants={fadeUp} custom={i} className="glass-card-light rounded-2xl p-8 card-hover text-center relative">
                <div className={`w-12 h-12 rounded-xl bg-gradient-to-br ${item.gradient} flex items-center justify-center mx-auto mb-5`}>
                  <span className="text-white text-sm font-bold font-mono">{item.step}</span>
                </div>
                <h3 className="text-lg font-display font-semibold text-white mt-2 mb-3">{item.title}</h3>
                <p className="text-sm text-white/40 leading-relaxed">{item.description}</p>
              </motion.div>
            ))}
          </div>
        </div>
      </AnimatedSection>

      {/* ═══════════ FEATURED DESIGNS ═══════════ */}
      <AnimatedSection className="py-24 border-t border-white/6 relative">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-end mb-12">
            <motion.div variants={fadeUp}>
              <h2 className="text-3xl md:text-4xl font-display font-bold text-white tracking-tight">Featured Designs</h2>
              <p className="text-sm text-white/35 mt-2">Verified packages, ready to download.</p>
            </motion.div>
            <motion.div variants={fadeUp} custom={1}>
              <Link
                href="/designs"
                className="hidden sm:inline-flex items-center text-sm font-medium text-brand-accent hover:text-blue-400 transition-colors duration-300 gap-1.5 hover-underline"
              >
                Browse all designs
                <span aria-hidden="true">&rarr;</span>
              </Link>
            </motion.div>
          </div>

          {featuredDesigns.length === 0 ? (
            <div className="text-center py-16 text-white/30">
              <p>No designs available yet. Check back soon.</p>
            </div>
          ) : (
            <motion.div variants={stagger} className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
              {featuredDesigns.map((design, i) => (
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
                        <div className="w-full h-full flex items-center justify-center text-white/20 text-sm">
                          No Preview
                        </div>
                      )}
                      {/* Gradient overlay */}
                      <div className="absolute inset-0 bg-gradient-to-t from-brand/60 to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300" />
                      <div className="absolute top-3 right-3 flex items-center gap-1 glass-card text-verified text-xs font-semibold px-2.5 py-1 rounded-full">
                        <CheckBadgeIcon className="h-3.5 w-3.5" />
                        Verified
                      </div>
                    </div>
                    <div className="p-5">
                      <h3 className="text-sm font-semibold text-white mb-1 group-hover:text-brand-accent transition-colors duration-300">{design.title}</h3>
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
            </motion.div>
          )}

          <motion.div variants={fadeUp} className="text-center mt-10 sm:hidden">
            <Link
              href="/designs"
              className="inline-flex items-center text-sm font-medium text-brand-accent hover:text-blue-400 transition-colors duration-300 gap-1.5"
            >
              Browse all designs
              <span aria-hidden="true">&rarr;</span>
            </Link>
          </motion.div>
        </div>
      </AnimatedSection>

      {/* ═══════════ VERIFICATION EXPLAINER ═══════════ */}
      <AnimatedSection className="py-24 relative">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="max-w-2xl mx-auto text-center">
            <motion.h2 variants={fadeUp} className="text-3xl md:text-4xl font-display font-bold text-white tracking-tight mb-8">
              What &ldquo;verified&rdquo; means on PlanMorph.
            </motion.h2>
            <div className="space-y-5 text-left">
              {[
                {
                  num: 1,
                  title: 'Architect uploads a design',
                  desc: 'Licensed architects submit complete plan sets including architectural drawings and documentation.',
                  color: 'from-brand-accent to-blue-400',
                },
                {
                  num: 2,
                  title: 'Engineer reviews structural integrity',
                  desc: 'A licensed structural engineer reviews the design for soundness and completeness.',
                  color: 'from-golden to-amber-400',
                },
                {
                  num: 3,
                  title: 'Design published with verification badge',
                  desc: 'Only designs that pass review are published. Every plan you see has completed this process.',
                  color: 'from-verified to-emerald-400',
                },
              ].map((item, i) => (
                <motion.div key={item.num} variants={fadeUp} custom={i} className="flex gap-4 items-start glass-card-light rounded-xl p-5 card-hover">
                  <div className={`flex-shrink-0 w-9 h-9 rounded-lg bg-gradient-to-br ${item.color} text-white flex items-center justify-center text-sm font-bold`}>
                    {item.num}
                  </div>
                  <div>
                    <h3 className="text-sm font-semibold text-white">{item.title}</h3>
                    <p className="text-sm text-white/40 mt-1">{item.desc}</p>
                  </div>
                </motion.div>
              ))}
            </div>
          </div>
        </div>
      </AnimatedSection>

      {/* ═══════════ CLIENT HOME ═══════════ */}
      <AnimatedSection className="py-24 border-t border-white/6 relative overflow-hidden">
        <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[700px] h-[700px] bg-golden/[0.03] rounded-full blur-3xl" />

        <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-10 items-center">
            <motion.div variants={fadeUp}>
              <h2 className="text-3xl md:text-4xl font-display font-bold text-white tracking-tight mb-4">
                Clients get a <span className="text-gradient-golden">home</span> for every project.
              </h2>
              <p className="text-white/35 max-w-xl leading-relaxed">
                Your purchases, files, and next steps live in one place—so you can move from browsing to building without chaos.
              </p>

              <div className="mt-7 space-y-3">
                {[
                  'Access purchased files anytime in My Orders',
                  'Preview PDFs, images, and videos in-browser before downloading',
                  'Request construction services (Kenya only) when available for your order',
                ].map((item) => (
                  <div key={item} className="flex items-start gap-3">
                    <div className="mt-0.5 h-5 w-5 rounded-full bg-white/5 border border-white/10 flex items-center justify-center">
                      <CheckBadgeIcon className="h-4 w-4 text-verified" />
                    </div>
                    <p className="text-sm text-white/45">{item}</p>
                  </div>
                ))}
              </div>

              <div className="mt-9 flex flex-wrap gap-3">
                <Link
                  href="/register"
                  className="px-6 py-3 text-sm font-semibold bg-brand-accent text-white rounded-xl hover:bg-blue-500 transition-all duration-300 btn-glow"
                >
                  Create a Client Account
                </Link>
                <Link
                  href="/my-orders"
                  className="px-6 py-3 text-sm font-medium text-white/60 hover:text-white border border-white/10 rounded-xl hover:border-white/20 hover:bg-white/5 transition-all duration-300"
                >
                  Go to My Orders
                </Link>
              </div>
            </motion.div>

            <motion.div variants={fadeUp} custom={1} className="glass-card rounded-2xl p-8 relative overflow-hidden">
              <div className="absolute inset-0 bg-gradient-to-br from-golden/10 via-transparent to-brand-accent/10" />
              <div className="relative">
                <div className="flex items-center justify-between gap-4 mb-6">
                  <div>
                    <p className="text-xs uppercase tracking-widest text-white/35">Client Portal</p>
                    <h3 className="text-lg font-display font-semibold text-white">My Orders</h3>
                  </div>
                  <div className="w-11 h-11 rounded-xl bg-white/5 border border-white/10 flex items-center justify-center">
                    <ArrowDownTrayIcon className="h-5 w-5 text-golden" />
                  </div>
                </div>

                <div className="space-y-4">
                  {[
                    { label: 'Order status', value: 'Paid • Files ready' },
                    { label: 'Files', value: 'Plan set • BOQ (as listed)' },
                    { label: 'Next step', value: 'Download • Share with contractor' },
                  ].map((row) => (
                    <div key={row.label} className="flex items-center justify-between glass-card-light rounded-xl px-4 py-3">
                      <span className="text-xs text-white/35">{row.label}</span>
                      <span className="text-xs text-white/65 font-medium">{row.value}</span>
                    </div>
                  ))}
                </div>

                <p className="text-[11px] text-white/25 mt-6 leading-relaxed">
                  Sales accuracy matters: your order shows what you purchased, what was delivered, and your verification context—clearly and in one place.
                </p>
              </div>
            </motion.div>
          </div>
        </div>
      </AnimatedSection>

      {/* ═══════════ FOR PROFESSIONALS ═══════════ */}
      <AnimatedSection className="py-24 border-t border-white/6 relative overflow-hidden">
        {/* Background glow */}
        <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] bg-brand-accent/[0.03] rounded-full blur-3xl" />

        <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <motion.div variants={fadeUp} className="text-center mb-14">
            <h2 className="text-3xl md:text-4xl font-display font-bold text-white tracking-tight mb-4">
              Your expertise deserves a{' '}
              <span className="text-gradient-golden">professional</span> home.
            </h2>
            <p className="text-white/35 max-w-xl mx-auto">
              Join licensed professionals who list, verify, and sell build-ready designs.
            </p>
          </motion.div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 max-w-3xl mx-auto">
            {/* Architect Card */}
            <motion.div variants={fadeUp} custom={0} className="glass-card rounded-2xl p-8 card-hover relative overflow-hidden group">
              {/* Decorative accent */}
              <div className="absolute top-0 left-0 w-1 h-full bg-gradient-to-b from-brand-accent via-blue-400 to-transparent" />
              <div className="absolute top-0 right-0 w-32 h-32 bg-brand-accent/5 rounded-full blur-2xl -translate-y-1/2 translate-x-1/2 group-hover:bg-brand-accent/10 transition-colors duration-500" />

              <h3 className="text-lg font-display font-semibold text-white mb-2 relative">Architect Portal</h3>
              <p className="text-white/40 text-sm mb-6 leading-relaxed relative">
                Upload your designs and earn 70% on every sale. Build your professional portfolio with verified, commercially proven work.
              </p>
              <div className="flex gap-3 relative">
                <Link
                  href="/architect/register"
                  className="px-5 py-2.5 text-sm font-medium bg-brand-accent text-white rounded-lg hover:bg-blue-500 transition-all duration-300 btn-glow"
                >
                  Apply
                </Link>
                <Link
                  href="/architect/login"
                  className="px-5 py-2.5 text-sm font-medium text-white/60 hover:text-white border border-white/10 rounded-lg hover:border-white/20 hover:bg-white/5 transition-all duration-300"
                >
                  Sign In
                </Link>
              </div>
            </motion.div>

            {/* Engineer Card */}
            <motion.div variants={fadeUp} custom={1} className="glass-card rounded-2xl p-8 card-hover relative overflow-hidden group">
              <div className="absolute top-0 left-0 w-1 h-full bg-gradient-to-b from-slate-teal via-emerald-400 to-transparent" />
              <div className="absolute top-0 right-0 w-32 h-32 bg-slate-teal/5 rounded-full blur-2xl -translate-y-1/2 translate-x-1/2 group-hover:bg-slate-teal/10 transition-colors duration-500" />

              <h3 className="text-lg font-display font-semibold text-white mb-2 relative">Engineer Portal</h3>
              <p className="text-white/40 text-sm mb-6 leading-relaxed relative">
                Review structural integrity and BOQs. Your verification ensures every published design is build-ready.
              </p>
              <div className="flex gap-3 relative">
                <Link
                  href="/engineer/register"
                  className="px-5 py-2.5 text-sm font-medium bg-slate-teal text-white rounded-lg hover:bg-teal-500 transition-all duration-300 btn-glow"
                >
                  Apply
                </Link>
                <Link
                  href="/engineer/login"
                  className="px-5 py-2.5 text-sm font-medium text-white/60 hover:text-white border border-white/10 rounded-lg hover:border-white/20 hover:bg-white/5 transition-all duration-300"
                >
                  Sign In
                </Link>
              </div>
            </motion.div>
          </div>
        </div>
      </AnimatedSection>
    </Layout>
  );
}
