'use client';

import Layout from '@/src/components/Layout';

export default function TermsOfServicePage() {
  return (
    <Layout>
      <div className="max-w-4xl mx-auto py-12 px-4 sm:px-6 lg:px-8">
        <div className="bg-white shadow-sm rounded-lg p-8">
          <h1 className="text-4xl font-bold text-gray-900 mb-4">Terms of Service</h1>
          <p className="text-sm text-gray-600 mb-8">Last Updated: February 11, 2026</p>

          <div className="prose prose-blue max-w-none space-y-8">
            {/* Agreement to Terms */}
            <section>
              <h2 className="text-2xl font-bold text-gray-900 mt-8 mb-4">1. Agreement to Terms</h2>
              <p className="text-gray-700 leading-relaxed">
                Welcome to PlanMorph. These Terms of Service (&quot;Terms&quot;) govern your access to and use of the
                PlanMorph platform, website, and services (collectively, the &quot;Platform&quot;). By accessing or using our
                Platform, you agree to be bound by these Terms and our Privacy Policy.
              </p>
              <div className="bg-yellow-50 border-l-4 border-yellow-400 p-4 mt-4">
                <p className="text-gray-800 font-semibold">
                  IF YOU DO NOT AGREE TO THESE TERMS, YOU MAY NOT ACCESS OR USE THE PLATFORM.
                </p>
              </div>
            </section>

            {/* Eligibility and Registration */}
            <section>
              <h2 className="text-2xl font-bold text-gray-900 mt-8 mb-4">2. Eligibility and Registration</h2>

              <div className="bg-blue-50 rounded-lg p-4 mb-4">
                <h4 className="font-semibold text-gray-900 mb-2">Age Requirement</h4>
                <p className="text-gray-700 text-sm">
                  You must be at least 18 years old to use the Platform. By using the Platform, you represent and
                  warrant that you are at least 18 years of age.
                </p>
              </div>

              <h3 className="text-xl font-semibold text-gray-900 mt-6 mb-3">Account Types</h3>
              <div className="grid md:grid-cols-3 gap-4">
                <div className="border rounded-lg p-4">
                  <h4 className="font-semibold text-blue-600 mb-2">Client Accounts</h4>
                  <ul className="text-sm text-gray-700 space-y-1 list-disc list-inside">
                    <li>Browse and purchase designs</li>
                    <li>Download purchased files</li>
                    <li>Request construction services</li>
                    <li>Track orders</li>
                  </ul>
                </div>
                <div className="border rounded-lg p-4">
                  <h4 className="font-semibold text-green-600 mb-2">Architect/Engineer</h4>
                  <ul className="text-sm text-gray-700 space-y-1 list-disc list-inside">
                    <li>Upload and sell designs</li>
                    <li>Earn 70% commission</li>
                    <li>Manage portfolio</li>
                    <li>Track earnings</li>
                  </ul>
                </div>
                <div className="border rounded-lg p-4">
                  <h4 className="font-semibold text-purple-600 mb-2">Contractor</h4>
                  <ul className="text-sm text-gray-700 space-y-1 list-disc list-inside">
                    <li>Receive project assignments</li>
                    <li>View project details</li>
                    <li>Construction services (Kenya)</li>
                  </ul>
                </div>
              </div>
            </section>

            {/* Platform Services */}
            <section>
              <h2 className="text-2xl font-bold text-gray-900 mt-8 mb-4">3. Platform Description</h2>
              <p className="text-gray-700 leading-relaxed mb-4">
                PlanMorph is a global online marketplace that connects clients seeking architectural designs with
                architects and engineers offering professionally designed plans.
              </p>

              <div className="bg-green-50 border border-green-200 rounded-lg p-4">
                <h4 className="font-semibold text-gray-900 mb-2">Construction Services</h4>
                <p className="text-gray-700 text-sm">
                  <strong>Important:</strong> Construction services are currently available <strong>ONLY in Kenya</strong>.
                  Clients in other countries may purchase designs but cannot request construction services through
                  the Platform.
                </p>
              </div>
            </section>

            {/* Payments and Financial Terms */}
            <section>
              <h2 className="text-2xl font-bold text-gray-900 mt-8 mb-4">4. Payments and Financial Terms</h2>

              <h3 className="text-xl font-semibold text-gray-900 mt-6 mb-3">Commission Structure</h3>
              <div className="bg-gradient-to-r from-blue-50 to-green-50 rounded-lg p-6 space-y-4">
                <div className="flex items-start">
                  <div className="flex-shrink-0">
                    <div className="flex items-center justify-center h-12 w-12 rounded-md bg-blue-600 text-white text-xl font-bold">
                      70%
                    </div>
                  </div>
                  <div className="ml-4">
                    <h4 className="text-lg font-semibold text-gray-900">Architects & Engineers</h4>
                    <p className="text-gray-700 text-sm">
                      Receive 70% of design sale price. Example: KES 50,000 design = KES 35,000 to architect
                    </p>
                  </div>
                </div>

                <div className="flex items-start">
                  <div className="flex-shrink-0">
                    <div className="flex items-center justify-center h-12 w-12 rounded-md bg-green-600 text-white text-xl font-bold">
                      30%
                    </div>
                  </div>
                  <div className="ml-4">
                    <h4 className="text-lg font-semibold text-gray-900">Platform Commission</h4>
                    <p className="text-gray-700 text-sm">
                      PlanMorph retains 30% to maintain platform, provide support, and ensure quality
                    </p>
                  </div>
                </div>

                <div className="flex items-start">
                  <div className="flex-shrink-0">
                    <div className="flex items-center justify-center h-12 w-12 rounded-md bg-purple-600 text-white text-xl font-bold">
                      2%
                    </div>
                  </div>
                  <div className="ml-4">
                    <h4 className="text-lg font-semibold text-gray-900">Construction Commission</h4>
                    <p className="text-gray-700 text-sm">
                      2% commission on estimated construction costs for contractor matching and support
                    </p>
                  </div>
                </div>
              </div>

              <h3 className="text-xl font-semibold text-gray-900 mt-6 mb-3">Payment Processing</h3>
              <p className="text-gray-700 leading-relaxed">
                All payments are processed in Kenya Shillings (KES) through Paystack. Prices are displayed in
                multiple currencies for convenience, but billing is always in KES.
              </p>

              <h3 className="text-xl font-semibold text-gray-900 mt-6 mb-3">Refund Policy</h3>
              <div className="bg-red-50 border border-red-200 rounded-lg p-4">
                <p className="text-gray-700 text-sm">
                  <strong>No refunds</strong> after design files have been downloaded or accessed. Refunds may be
                  granted if files are corrupted, design significantly misrepresented, or duplicate purchase made
                  in error (within 24 hours). Refund requests must be submitted within 7 days of purchase.
                </p>
              </div>
            </section>

            {/* Intellectual Property */}
            <section>
              <h2 className="text-2xl font-bold text-gray-900 mt-8 mb-4">5. Design Submissions and Intellectual Property</h2>

              <div className="space-y-4">
                <div className="border-l-4 border-blue-500 pl-4">
                  <h4 className="font-semibold text-gray-900 mb-2">Architect Retains Copyright</h4>
                  <p className="text-gray-700 text-sm">
                    Architects retain full copyright and ownership of their original designs. PlanMorph does not
                    claim ownership of design content.
                  </p>
                </div>

                <div className="border-l-4 border-green-500 pl-4">
                  <h4 className="font-semibold text-gray-900 mb-2">Platform License</h4>
                  <p className="text-gray-700 text-sm">
                    By uploading a design, Architects grant PlanMorph a non-exclusive, worldwide, royalty-free
                    license to display designs in the marketplace, generate previews, promote designs, and
                    distribute to buyers.
                  </p>
                </div>

                <div className="border-l-4 border-purple-500 pl-4">
                  <h4 className="font-semibold text-gray-900 mb-2">Client License</h4>
                  <p className="text-gray-700 text-sm">
                    When a Client purchases a design, they receive a <strong>non-exclusive, non-transferable
                    license</strong> to use the design for <strong>one residential or commercial construction
                    project</strong>. Clients may NOT resell, redistribute, or use the design for multiple projects
                    without additional licenses.
                  </p>
                </div>
              </div>
            </section>

            {/* Construction Services */}
            <section>
              <h2 className="text-2xl font-bold text-gray-900 mt-8 mb-4">6. Construction Services (Kenya Only)</h2>

              <div className="bg-green-50 border-2 border-green-500 rounded-lg p-6 mb-4">
                <h4 className="font-semibold text-gray-900 mb-2 flex items-center">
                  <svg className="w-6 h-6 text-green-600 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                  </svg>
                  Geographic Limitation
                </h4>
                <p className="text-gray-700">
                  Construction services are currently available <strong>exclusively in Kenya</strong>. Clients located
                  in other countries may purchase designs but cannot request construction services through the Platform.
                </p>
              </div>

              <h3 className="text-xl font-semibold text-gray-900 mt-6 mb-3">PlanMorph's Role</h3>
              <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
                <p className="text-gray-700 text-sm mb-2">
                  <strong>PlanMorph is a facilitator only.</strong> We:
                </p>
                <ul className="list-disc list-inside space-y-1 text-gray-700 text-sm">
                  <li>Facilitate connection between clients and contractors</li>
                  <li>Provide platform for contract management</li>
                  <li>Collect 2% platform commission</li>
                  <li>Mediate disputes when necessary</li>
                </ul>

                <p className="text-gray-700 text-sm mt-4 mb-2">
                  <strong>PlanMorph is NOT:</strong>
                </p>
                <ul className="list-disc list-inside space-y-1 text-gray-700 text-sm">
                  <li>A construction company or contractor</li>
                  <li>Responsible for construction quality or workmanship</li>
                  <li>Liable for delays, cost overruns, or construction defects</li>
                  <li>A party to the construction contract</li>
                </ul>
              </div>
            </section>

            {/* Disclaimers */}
            <section>
              <h2 className="text-2xl font-bold text-gray-900 mt-8 mb-4">7. Disclaimers and Limitations of Liability</h2>

              <div className="bg-red-50 border-2 border-red-500 rounded-lg p-6">
                <h4 className="font-semibold text-gray-900 mb-3">Platform &quot;As Is&quot;</h4>
                <p className="text-gray-700 text-sm mb-4">
                  THE PLATFORM IS PROVIDED ON AN &quot;AS IS&quot; AND &quot;AS AVAILABLE&quot; BASIS WITHOUT WARRANTIES OF ANY KIND.
                </p>

                <h4 className="font-semibold text-gray-900 mb-3">Design Accuracy Disclaimer</h4>
                <p className="text-gray-700 text-sm mb-2">
                  <strong>PlanMorph does not guarantee:</strong>
                </p>
                <ul className="list-disc list-inside space-y-1 text-gray-700 text-sm mb-4">
                  <li>Accuracy or completeness of design specifications</li>
                  <li>Compliance with local building codes or regulations</li>
                  <li>Structural soundness or engineering integrity</li>
                  <li>Suitability for any particular location or climate</li>
                </ul>

                <p className="text-gray-700 text-sm">
                  <strong>Clients are responsible for:</strong> Verifying designs with local engineers and architects,
                  obtaining necessary permits, ensuring compliance with local codes, and modifying designs for
                  site-specific conditions.
                </p>
              </div>

              <div className="bg-gray-100 rounded-lg p-4 mt-4">
                <h4 className="font-semibold text-gray-900 mb-2">Total Liability Cap</h4>
                <p className="text-gray-700 text-sm">
                  Our total liability for all claims shall not exceed the greater of: (1) Fees paid by you in the
                  12 months preceding the claim, OR (2) KES 50,000 (fifty thousand Kenya Shillings).
                </p>
              </div>
            </section>

            {/* User Conduct */}
            <section>
              <h2 className="text-2xl font-bold text-gray-900 mt-8 mb-4">8. User Conduct and Prohibited Activities</h2>

              <div className="grid md:grid-cols-2 gap-4">
                <div className="bg-red-50 rounded-lg p-4">
                  <h4 className="font-semibold text-red-800 mb-2">❌ Prohibited Activities</h4>
                  <ul className="text-sm text-gray-700 space-y-1 list-disc list-inside">
                    <li>Provide false or misleading information</li>
                    <li>Upload designs you don't own</li>
                    <li>Infringe on copyrights or trademarks</li>
                    <li>Engage in payment fraud</li>
                    <li>Redistribute purchased designs</li>
                    <li>Use automated bots or scrapers</li>
                  </ul>
                </div>

                <div className="bg-green-50 rounded-lg p-4">
                  <h4 className="font-semibold text-green-800 mb-2">✓ Consequences of Violations</h4>
                  <ul className="text-sm text-gray-700 space-y-1 list-disc list-inside">
                    <li>Warning and request to cease</li>
                    <li>Temporary account suspension</li>
                    <li>Permanent account termination</li>
                    <li>Removal of designs or content</li>
                    <li>Forfeiture of unpaid commissions</li>
                    <li>Legal action and damages</li>
                  </ul>
                </div>
              </div>
            </section>

            {/* Dispute Resolution */}
            <section>
              <h2 className="text-2xl font-bold text-gray-900 mt-8 mb-4">9. Dispute Resolution</h2>

              <div className="space-y-4">
                <div className="bg-blue-50 rounded-lg p-4">
                  <h4 className="font-semibold text-gray-900 mb-2">Step 1: Informal Resolution</h4>
                  <p className="text-gray-700 text-sm">
                    Before filing any formal dispute, contact us at disputes@planmorph.com to attempt informal
                    resolution. We will work in good faith to resolve disputes within 30 days.
                  </p>
                </div>

                <div className="bg-blue-50 rounded-lg p-4">
                  <h4 className="font-semibold text-gray-900 mb-2">Step 2: Mediation</h4>
                  <p className="text-gray-700 text-sm">
                    If informal resolution fails, disputes shall be submitted to mediation before a neutral mediator
                    agreed upon by both parties. Mediation costs shall be shared equally.
                  </p>
                </div>

                <div className="bg-blue-50 rounded-lg p-4">
                  <h4 className="font-semibold text-gray-900 mb-2">Step 3: Arbitration</h4>
                  <p className="text-gray-700 text-sm">
                    If mediation is unsuccessful, disputes shall be resolved through binding arbitration:
                  </p>
                  <ul className="list-disc list-inside text-sm text-gray-700 mt-2 space-y-1">
                    <li><strong>Kenya:</strong> Nairobi Centre for International Arbitration (NCIA)</li>
                    <li><strong>International:</strong> International Chamber of Commerce (ICC)</li>
                    <li>Arbitrator's decision is final and binding</li>
                  </ul>
                </div>
              </div>

              <div className="bg-yellow-50 border border-yellow-400 rounded-lg p-4 mt-4">
                <h4 className="font-semibold text-gray-900 mb-2">Class Action Waiver</h4>
                <p className="text-gray-700 text-sm">
                  YOU AGREE THAT DISPUTES WILL BE RESOLVED ON AN INDIVIDUAL BASIS ONLY. You waive any right to
                  bring or participate in any class action, collective action, or representative proceeding.
                </p>
              </div>
            </section>

            {/* Governing Law */}
            <section>
              <h2 className="text-2xl font-bold text-gray-900 mt-8 mb-4">10. Governing Law and Jurisdiction</h2>
              <div className="bg-gray-100 rounded-lg p-4">
                <p className="text-gray-700 leading-relaxed">
                  These Terms are governed by and construed in accordance with the laws of the <strong>Republic of
                  Kenya</strong>, without regard to conflict of law principles. For disputes not subject to arbitration,
                  you agree to submit to the exclusive jurisdiction of the courts located in <strong>Nairobi, Kenya</strong>.
                </p>
              </div>
            </section>

            {/* Changes to Terms */}
            <section>
              <h2 className="text-2xl font-bold text-gray-900 mt-8 mb-4">11. Modifications to Terms</h2>
              <p className="text-gray-700 leading-relaxed">
                We reserve the right to modify these Terms at any time. Changes will be effective immediately for
                non-material changes or 30 days after notice for material changes. Your continued use of the Platform
                after changes take effect constitutes acceptance of the modified Terms.
              </p>
            </section>

            {/* Contact Information */}
            <section>
              <h2 className="text-2xl font-bold text-gray-900 mt-8 mb-4">12. Contact Information</h2>
              <div className="bg-gray-100 rounded-lg p-6 space-y-3">
                <div>
                  <strong className="text-gray-900">General Support:</strong>
                  <p className="text-gray-700">Email: support@planmorph.com</p>
                  <p className="text-gray-700">Website: https://planmorph.com/contact</p>
                </div>
                <div>
                  <strong className="text-gray-900">Legal Inquiries:</strong>
                  <p className="text-gray-700">Email: legal@planmorph.com</p>
                </div>
                <div>
                  <strong className="text-gray-900">Dispute Resolution:</strong>
                  <p className="text-gray-700">Email: disputes@planmorph.com</p>
                </div>
                <div>
                  <strong className="text-gray-900">Copyright Claims (DMCA):</strong>
                  <p className="text-gray-700">Email: dmca@planmorph.com</p>
                </div>
              </div>
            </section>

            {/* Acceptance */}
            <div className="mt-12 pt-8 border-t-2 border-gray-300">
              <div className="bg-blue-50 border-2 border-blue-500 rounded-lg p-6 text-center">
                <p className="text-gray-900 font-semibold">
                  By using PlanMorph, you acknowledge that you have read, understood, and agree to be bound by
                  these Terms of Service.
                </p>
                <p className="text-sm text-gray-600 mt-2">
                  Effective Date: February 11, 2026 | Version: 1.0
                </p>
              </div>
            </div>

            {/* Footer Note */}
            <div className="mt-8">
              <p className="text-sm text-gray-600 text-center">
                For the most current version of these Terms, please visit:{' '}
                <a href="/terms-of-service" className="text-blue-600 hover:underline">
                  https://planmorph.com/terms-of-service
                </a>
              </p>
            </div>
          </div>
        </div>
      </div>
    </Layout>
  );
}
