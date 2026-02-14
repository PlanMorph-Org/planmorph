import './global.css';

import type { Metadata } from 'next';
import type { ReactNode } from 'react';
import { Inter, Outfit } from 'next/font/google';

const inter = Inter({
  subsets: ['latin'],
  display: 'swap',
  variable: '--font-inter',
});

const outfit = Outfit({
  subsets: ['latin'],
  display: 'swap',
  variable: '--font-outfit',
  weight: ['400', '500', '600', '700', '800'],
});

export const metadata: Metadata = {
  title: 'PlanMorph — Build-Ready Design Infrastructure',
  description: 'Publish, license, and download engineer-reviewed architectural and civil design packages. PlanMorph is the distribution layer for construction-ready documentation.',
  keywords: [
    'architectural plans',
    'structural plans',
    'civil designs',
    'design packages',
    'build-ready',
    'BOQ',
    'bill of quantities',
  ],
  icons: {
    icon: '/icon.svg',
  },
};

export default function RootLayout({
  children,
}: {
  children: ReactNode;
}) {
  return (
    <html lang="en" className={`${inter.variable} ${outfit.variable}`}>
      <body className="min-h-screen bg-brand text-gray-200 font-sans antialiased">
        {children}
      </body>
    </html>
  );
}
