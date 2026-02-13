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
      title: 'Structural Integrity',
      description: 'Every plan is reviewed by a licensed structural engineer before it reaches you.',
      gradient: 'from-brand-accent/20 to-blue-500/10',
      iconColor: 'text-brand-accent',
    },
    {
      icon: DocumentCheckIcon,
      title: 'Complete Documentation',
      description: 'From architectural drawings to BOQs — everything you need to break ground.',
      gradient: 'from-golden/20 to-amber-500/10',
      iconColor: 'text-golden',
    },
    {
      icon: UserGroupIcon,
      title: 'Licensed Professionals',
      description: 'Every architect and engineer on the platform is credentialed and vetted.',
      gradient: 'from-verified/20 to-emerald-500/10',
      iconColor: 'text-verified',
    },
    {
      icon: BuildingOffice2Icon,
      title: 'Build-ready Plans',
      description: 'Take our plans straight to your contractor or county approvals office.',
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
            Building <span className="text-gradient-golden">trust</span> into every plan.
          </motion.h1>
          <motion.p
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.25, duration: 0.6, ease: [0.16, 1, 0.3, 1] }}
            className="text-lg text-white/40 max-w-2xl mx-auto leading-relaxed"
          >
            PlanMorph is a marketplace where every architectural design is structurally verified by a licensed engineer before it reaches you.
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
              Too many buildings begin without verified plans. We created PlanMorph so that every person building a home, office, or commercial space can access designs that have been reviewed for structural integrity by a licensed engineer — not just drawn by an architect.
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
            Our verification process.
          </motion.h2>
          <div className="space-y-6">
            {[
              { step: '01', title: 'Architect submits a complete plan set', desc: 'Includes architectural drawings, structural documentation, and a bill of quantities.', color: 'from-brand-accent to-blue-400' },
              { step: '02', title: 'Licensed engineer reviews the design', desc: 'Structural integrity, load calculations, and material specifications are checked.', color: 'from-golden to-amber-400' },
              { step: '03', title: 'Design published with verification badge', desc: 'Only designs that pass are published. Every plan on PlanMorph has been through this process.', color: 'from-verified to-emerald-400' },
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
            Ready to build with certainty?
          </motion.h2>
          <motion.p variants={fadeUp} custom={1} className="text-white/40 mb-8">
            Browse verified designs or apply as a professional.
          </motion.p>
          <motion.div variants={fadeUp} custom={2} className="flex flex-wrap justify-center gap-4">
            <a href="/designs" className="px-8 py-3.5 bg-brand-accent text-white font-semibold rounded-xl hover:bg-blue-500 transition-all duration-300 shadow-blue btn-glow">
              Browse Designs
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
