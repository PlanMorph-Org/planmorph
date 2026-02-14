'use client';

import Layout from '../../components/Layout';
import { motion, useInView } from 'framer-motion';
import { useRef } from 'react';
import { ShieldCheckIcon, DocumentCheckIcon, UserGroupIcon, BuildingOffice2Icon } from '@heroicons/react/24/outline';

const fadeUp = {
  hidden: { opacity: 0, y: 30 },
  visible: (i: number = 0) => ({
    opacity: 1, y: 0,
    transition: { delay: i * 0.1, duration: 0.6, ease: [0.16, 1, 0.3, 1] as const },
  }),
};

const stagger = { visible: { transition: { staggerChildren: 0.08 } } };

function AnimatedSection({ children, className = '' }: { children: React.ReactNode; className?: string }) {
  const ref = useRef(null);
  const inView = useInView(ref, { once: true, margin: '-60px' });
  return (
    <motion.section ref={ref} initial="hidden" animate={inView ? 'visible' : 'hidden'} variants={stagger} className={className}>
      {children}
    </motion.section>
  );
}

export default function AboutPage() {
  const values = [
    {
      icon: ShieldCheckIcon,
      title: 'Verification Gate',
      description: 'Engineer review is a publishing gate. Only packages that pass ship.',
      gradient: 'from-brand-accent/20 to-blue-500/10',
      iconColor: 'text-brand-accent',
    },
    {
      icon: DocumentCheckIcon,
      title: 'Complete Packages',
      description: 'Plan sets, structural documentation, and BOQs packaged for delivery.',
      gradient: 'from-golden/20 to-amber-500/10',
      iconColor: 'text-golden',
    },
    {
      icon: UserGroupIcon,
      title: 'Credentialed Publishers',
      description: 'Architects and engineers are reviewed before they publish on PlanMorph.',
      gradient: 'from-verified/20 to-emerald-500/10',
      iconColor: 'text-verified',
    },
    {
      icon: BuildingOffice2Icon,
      title: 'Distribution Layer',
      description: 'Licensing, delivery, and documentation clarity—built into the workflow.',
      gradient: 'from-purple-500/20 to-violet-500/10',
      iconColor: 'text-purple-400',
    },
  ];

  return (
    <Layout>
      {/* Hero */}
      <section className="relative py-24 lg:py-32 overflow-hidden">
        <div className="absolute inset-0 bg-hero-gradient" />
        <div className="absolute inset-0 blueprint-grid opacity-30" />
        <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[700px] h-[700px] bg-brand-accent/[0.03] rounded-full blur-3xl" />

        <div className="relative max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
          <motion.p
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5, ease: [0.16, 1, 0.3, 1] }}
            className="inline-flex items-center gap-2 px-4 py-1.5 rounded-full bg-white/5 border border-white/8 text-xs font-medium text-white/50 mb-8"
          >
            About PlanMorph
          </motion.p>
          <motion.h1
            initial={{ opacity: 0, y: 30 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.1, duration: 0.7, ease: [0.16, 1, 0.3, 1] }}
            className="text-4xl md:text-5xl lg:text-6xl font-display font-bold tracking-tight leading-[1.08] text-white mb-6"
          >
            The distribution layer for <span className="text-gradient-golden">build-ready</span> design packages.
          </motion.h1>
          <motion.p
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.25, duration: 0.6, ease: [0.16, 1, 0.3, 1] }}
            className="text-lg text-white/40 max-w-2xl mx-auto leading-relaxed"
          >
            PlanMorph is the publishing and distribution infrastructure for architectural and civil design packages—engineer-reviewed, packaged with complete documentation, and delivered with clear licensing.
          </motion.p>
        </div>
      </section>

      {/* Mission */}
      <AnimatedSection className="py-20 border-t border-white/6">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          <motion.div variants={fadeUp} className="glass-card rounded-2xl p-10 relative overflow-hidden">
            <div className="absolute top-0 left-0 w-1 h-full bg-gradient-to-b from-brand-accent via-golden to-verified" />
            <h2 className="text-2xl font-display font-bold text-white mb-4">Our Mission</h2>
            <p className="text-white/50 leading-relaxed text-lg">
              Construction is slowed down by fragmented design delivery: inconsistent plan sets, unclear scope, and files scattered across inboxes and drives. PlanMorph standardizes professional publishing with complete packages, engineer review, and fast distribution—so buyers can execute and professionals can monetize their work with credibility.
            </p>
          </motion.div>
        </div>
      </AnimatedSection>

      {/* Values */}
      <AnimatedSection className="py-20">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <motion.h2 variants={fadeUp} className="text-3xl font-display font-bold text-white text-center mb-14">
            Why <span className="text-gradient-blue">PlanMorph</span>?
          </motion.h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {values.map((v, i) => (
              <motion.div key={v.title} variants={fadeUp} custom={i} className="glass-card-light rounded-xl p-6 card-hover text-center">
                <div className={`w-12 h-12 rounded-xl bg-gradient-to-br ${v.gradient} flex items-center justify-center mx-auto mb-4`}>
                  <v.icon className={`w-6 h-6 ${v.iconColor}`} />
                </div>
                <h3 className="text-sm font-semibold text-white mb-2">{v.title}</h3>
                <p className="text-xs text-white/40 leading-relaxed">{v.description}</p>
              </motion.div>
            ))}
          </div>
        </div>
      </AnimatedSection>

      {/* How Verification Works */}
      <AnimatedSection className="py-20 border-t border-white/6">
        <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
          <motion.h2 variants={fadeUp} className="text-3xl font-display font-bold text-white mb-10">
            How publishing works.
          </motion.h2>
          <div className="space-y-6">
            {[
              { step: '01', title: 'Publisher submits a complete package', desc: 'Includes plan sets, documentation, and the deliverables required for distribution.', color: 'from-brand-accent to-blue-400' },
              { step: '02', title: 'Engineer review gates publication', desc: 'Structural integrity and documentation completeness are reviewed before listing.', color: 'from-golden to-amber-400' },
              { step: '03', title: 'Package published for distribution', desc: 'Approved packages ship with a verification record and clear delivery expectations.', color: 'from-verified to-emerald-400' },
            ].map((item, i) => (
              <motion.div key={item.step} variants={fadeUp} custom={i} className="flex items-start gap-4 text-left glass-card-light rounded-xl p-5 card-hover">
                <div className={`flex-shrink-0 w-10 h-10 rounded-lg bg-gradient-to-br ${item.color} text-white flex items-center justify-center text-sm font-bold font-mono`}>
                  {item.step}
                </div>
                <div>
                  <h3 className="text-sm font-semibold text-white">{item.title}</h3>
                  <p className="text-sm text-white/40 mt-1">{item.desc}</p>
                </div>
              </motion.div>
            ))}
          </div>
        </div>
      </AnimatedSection>

      {/* CTA */}
      <AnimatedSection className="py-20 border-t border-white/6">
        <div className="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
          <motion.h2 variants={fadeUp} className="text-3xl font-display font-bold text-white mb-4">
            Ready to move faster with build-ready packages?
          </motion.h2>
          <motion.p variants={fadeUp} custom={1} className="text-white/40 mb-8">
            Browse packages or publish as a verified professional.
          </motion.p>
          <motion.div variants={fadeUp} custom={2} className="flex flex-wrap justify-center gap-4">
            <a href="/designs" className="px-8 py-3.5 bg-brand-accent text-white font-semibold rounded-xl hover:bg-blue-500 transition-all duration-300 shadow-blue btn-glow">
              Browse Packages
            </a>
            <a href="/register" className="px-8 py-3.5 text-white/50 hover:text-white font-medium border border-white/10 rounded-xl hover:bg-white/5 transition-all duration-300">
              Create Account
            </a>
          </motion.div>
        </div>
      </AnimatedSection>
    </Layout>
  );
}
