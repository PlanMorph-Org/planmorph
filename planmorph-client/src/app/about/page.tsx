'use client';

import Layout from '@/src/components/Layout';

export default function AboutPage() {
  return (
    <Layout>
      <div className="bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
          <div className="text-center mb-12">
            <h1 className="text-4xl font-bold text-gray-900 mb-4">About PlanMorph</h1>
            <p className="text-xl text-gray-600 max-w-3xl mx-auto">
              Transforming the way people access quality architectural designs worldwide
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-12 mb-16">
            <div>
              <h2 className="text-2xl font-bold text-gray-900 mb-4">Our Mission</h2>
              <p className="text-gray-600 mb-4">
                PlanMorph is dedicated to making professional architectural designs accessible and affordable
                for clients everywhere. We bridge the gap between talented architects and people who want to
                build their dream homes.
              </p>
              <p className="text-gray-600">
                By providing a marketplace for verified, high-quality designs and connecting clients with
                trusted contractors (currently in Kenya), we're making the home-building process smoother and more transparent.
              </p>
            </div>

            <div>
              <h2 className="text-2xl font-bold text-gray-900 mb-4">How It Works</h2>
              <ol className="space-y-4">
                <li className="flex">
                  <span className="flex-shrink-0 w-8 h-8 bg-blue-600 text-white rounded-full flex items-center justify-center font-semibold mr-3">1</span>
                  <div>
                    <h3 className="font-semibold text-gray-900">Browse Designs</h3>
                    <p className="text-gray-600">Explore our curated collection of verified architectural designs</p>
                  </div>
                </li>
                <li className="flex">
                  <span className="flex-shrink-0 w-8 h-8 bg-blue-600 text-white rounded-full flex items-center justify-center font-semibold mr-3">2</span>
                  <div>
                    <h3 className="font-semibold text-gray-900">Purchase Plans</h3>
                    <p className="text-gray-600">Get instant access to complete architectural and engineering drawings</p>
                  </div>
                </li>
                <li className="flex">
                  <span className="flex-shrink-0 w-8 h-8 bg-blue-600 text-white rounded-full flex items-center justify-center font-semibold mr-3">3</span>
                  <div>
                    <h3 className="font-semibold text-gray-900">Build Your Dream</h3>
                    <p className="text-gray-600">Optionally connect with trusted contractors for construction services (Kenya only for now)</p>
                  </div>
                </li>
              </ol>
            </div>
          </div>

          <div className="bg-blue-50 rounded-lg p-8 mb-12">
            <h2 className="text-2xl font-bold text-gray-900 mb-6 text-center">Why Choose PlanMorph?</h2>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
              <div className="text-center">
                <div className="w-16 h-16 bg-blue-600 rounded-full flex items-center justify-center mx-auto mb-4">
                  <svg className="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                </div>
                <h3 className="font-semibold text-gray-900 mb-2">Verified Quality</h3>
                <p className="text-gray-600">All designs approved by licensed professionals</p>
              </div>
              <div className="text-center">
                <div className="w-16 h-16 bg-blue-600 rounded-full flex items-center justify-center mx-auto mb-4">
                  <svg className="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                </div>
                <h3 className="font-semibold text-gray-900 mb-2">Affordable Pricing</h3>
                <p className="text-gray-600">Transparent pricing with no hidden costs</p>
              </div>
              <div className="text-center">
                <div className="w-16 h-16 bg-blue-600 rounded-full flex items-center justify-center mx-auto mb-4">
                  <svg className="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z" />
                  </svg>
                </div>
                <h3 className="font-semibold text-gray-900 mb-2">Full Support</h3>
                <p className="text-gray-600">Modification requests and construction assistance</p>
              </div>
            </div>
          </div>

          <div className="text-center">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">Ready to Get Started?</h2>
            <p className="text-gray-600 mb-6">
              Join customers worldwide building their dream homes with PlanMorph
            </p>
            <a
              href="/designs"
              className="inline-block px-8 py-3 bg-blue-600 text-white font-semibold rounded-lg hover:bg-blue-700 transition"
            >
              Browse Designs
            </a>
          </div>
        </div>
      </div>
    </Layout>
  );
}
