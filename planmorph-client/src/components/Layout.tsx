'use client';

import React from 'react';
import Link from 'next/link';
import FloatingActionBar from './FloatingActionBar';

interface LayoutProps {
  children: React.ReactNode;
}

const navLinks = [
  { href: '/designs', label: 'Browse Designs' },
  { href: '/about', label: 'About' },
];

export default function Layout({ children }: LayoutProps) {
  return (
    <div className="min-h-screen bg-brand">
      <FloatingActionBar
        navItems={navLinks}
        accentColor="blue"
        logoHref="/"
        authMode="client"
        showCurrency
      />

      {/* Main Content */}
      <main>{children}</main>

      {/* Footer */}
      <footer className="relative bg-brand-light border-t border-white/6 mt-16 overflow-hidden">
        {/* Blueprint grid background */}
        <div className="absolute inset-0 blueprint-grid opacity-30" />

        <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-14">
          <div className="grid grid-cols-1 md:grid-cols-5 gap-8">
            <div>
              <div className="flex items-center gap-2.5 mb-4">
                <img src="/planmorph.svg" alt="PlanMorph" className="h-7 w-auto brightness-0 invert rounded-full" />
                <span className="text-lg font-display font-bold text-white">PlanMorph</span>
              </div>
              <p className="text-white/40 text-sm leading-relaxed">
                Build with certainty.
              </p>
              <p className="text-white/25 text-xs mt-3">
                Verified architectural designs from licensed professionals.
              </p>
              {/* Decorative element */}
              <div className="mt-5 flex gap-1">
                {[...Array(5)].map((_, i) => (
                  <div
                    key={i}
                    className="h-1 rounded-full bg-gradient-to-r from-brand-accent to-golden"
                    style={{ width: `${20 - i * 3}px`, opacity: 1 - i * 0.15 }}
                  />
                ))}
              </div>
            </div>
            <div>
              <h3 className="text-xs font-semibold uppercase tracking-widest text-golden/70 mb-4">Browse</h3>
              <ul className="space-y-2.5">
                <li>
                  <Link href="/designs" className="text-white/50 hover:text-white text-sm transition-colors duration-300 hover-underline">
                    Browse Designs
                  </Link>
                </li>
                <li>
                  <Link href="/about" className="text-white/50 hover:text-white text-sm transition-colors duration-300 hover-underline">
                    About
                  </Link>
                </li>
              </ul>
            </div>
            <div>
              <h3 className="text-xs font-semibold uppercase tracking-widest text-golden/70 mb-4">For Professionals</h3>
              <ul className="space-y-2.5">
                <li>
                  <Link href="/architect/login" className="text-white/50 hover:text-white text-sm transition-colors duration-300 hover-underline">
                    Architect Portal
                  </Link>
                </li>
                <li>
                  <Link href="/engineer/login" className="text-white/50 hover:text-white text-sm transition-colors duration-300 hover-underline">
                    Engineer Portal
                  </Link>
                </li>
              </ul>
            </div>
            <div>
              <h3 className="text-xs font-semibold uppercase tracking-widest text-golden/70 mb-4">Support</h3>
              <ul className="space-y-2.5">
                <li>
                  <Link href="/support" className="text-white/50 hover:text-white text-sm transition-colors duration-300 hover-underline">
                    Help Center
                  </Link>
                </li>
                <li>
                  <Link href="/support/create" className="text-white/50 hover:text-white text-sm transition-colors duration-300 hover-underline">
                    Contact Support
                  </Link>
                </li>
              </ul>
            </div>
            <div>
              <h3 className="text-xs font-semibold uppercase tracking-widest text-golden/70 mb-4">Legal</h3>
              <ul className="space-y-2.5">
                <li>
                  <Link href="/privacy-policy" className="text-white/50 hover:text-white text-sm transition-colors duration-300 hover-underline">
                    Privacy Policy
                  </Link>
                </li>
                <li>
                  <Link href="/terms-of-service" className="text-white/50 hover:text-white text-sm transition-colors duration-300 hover-underline">
                    Terms of Service
                  </Link>
                </li>
              </ul>
              <div className="mt-5">
                <p className="text-white/30 text-xs">info@planmorph.software</p>
                <p className="text-white/30 text-xs">support@planmorph.software</p>
              </div>
            </div>
          </div>
          <div className="mt-12 pt-8 border-t border-white/6 flex flex-col sm:flex-row justify-between items-center gap-4">
            <p className="text-white/25 text-xs">&copy; 2025 PlanMorph. All rights reserved.</p>
            <p className="text-white/25 text-xs">
              By using this site, you agree to our{' '}
              <Link href="/terms-of-service" className="text-brand-accent/70 hover:text-brand-accent transition-colors">
                Terms of Service
              </Link>
              {' '}and{' '}
              <Link href="/privacy-policy" className="text-brand-accent/70 hover:text-brand-accent transition-colors">
                Privacy Policy
              </Link>.
            </p>
          </div>
        </div>
      </footer>
    </div>
  );
}
