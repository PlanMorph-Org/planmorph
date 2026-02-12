'use client';

import Layout from '@/src/components/Layout';

export default function PrivacyPolicyPage() {
  return (
    <Layout>
      <div className="max-w-4xl mx-auto py-12 px-4 sm:px-6 lg:px-8">
        <div className="bg-white shadow-sm rounded-lg p-8">
          <h1 className="text-4xl font-bold text-gray-900 mb-4">Privacy Policy</h1>
          <p className="text-sm text-gray-600 mb-8">Last Updated: February 11, 2026</p>

          <div className="prose prose-blue max-w-none space-y-8">
            {/* Introduction */}
            <section>
              <h2 className="text-2xl font-bold text-gray-900 mt-8 mb-4">1. Introduction</h2>
              <p className="text-gray-700 leading-relaxed">
                Welcome to PlanMorph (&quot;we,&quot; &quot;our,&quot; or &quot;us&quot;). PlanMorph operates an online marketplace connecting
                architects and engineers with clients seeking architectural designs and construction services. We are
                committed to protecting your personal information and your right to privacy.
              </p>
              <p className="text-gray-700 leading-relaxed mt-4">
                This Privacy Policy explains how we collect, use, disclose, and safeguard your information when you use
                our platform at planmorph.com (the &quot;Platform&quot;). Please read this privacy policy carefully. If you do
                not agree with the terms of this privacy policy, please do not access the Platform.
              </p>
            </section>

            {/* Information We Collect */}
            <section>
              <h2 className="text-2xl font-bold text-gray-900 mt-8 mb-4">2. Information We Collect</h2>

              <h3 className="text-xl font-semibold text-gray-900 mt-6 mb-3">2.1 Personal Information You Provide</h3>

              <div className="bg-gray-50 rounded-lg p-4 my-4">
                <h4 className="font-semibold text-gray-900 mb-2">All Users (Clients, Architects, Engineers, Contractors)</h4>
                <ul className="list-disc list-inside space-y-1 text-gray-700">
                  <li>Full name (first name and last name)</li>
                  <li>Email address</li>
                  <li>Phone number</li>
                  <li>Password (encrypted and securely stored)</li>
                  <li>Account preferences and settings</li>
                  <li>User role and account status</li>
                </ul>
              </div>

              <div className="bg-blue-50 rounded-lg p-4 my-4">
                <h4 className="font-semibold text-gray-900 mb-2">Professional Users (Architects & Engineers)</h4>
                <ul className="list-disc list-inside space-y-1 text-gray-700">
                  <li>Professional license number and documentation</li>
                  <li>Years of professional experience</li>
                  <li>Portfolio URL (if provided)</li>
                  <li>Specialization and areas of expertise</li>
                  <li>Professional documents (CV, cover letter, work experience)</li>
                  <li>Document metadata (file size, upload timestamps)</li>
                </ul>
              </div>
            </section>

            {/* How We Use Your Information */}
            <section>
              <h2 className="text-2xl font-bold text-gray-900 mt-8 mb-4">3. How We Use Your Information</h2>

              <div className="space-y-4">
                <div>
                  <h4 className="font-semibold text-gray-900 mb-2">To Provide and Maintain Our Services</h4>
                  <ul className="list-disc list-inside space-y-1 text-gray-700">
                    <li>Process user registrations and manage accounts</li>
                    <li>Authenticate users and maintain security</li>
                    <li>Enable design browsing, filtering, and purchasing</li>
                    <li>Facilitate construction service requests (Kenya only)</li>
                    <li>Process payments through our payment partners</li>
                    <li>Deliver purchased design files and documentation</li>
                  </ul>
                </div>

                <div>
                  <h4 className="font-semibold text-gray-900 mb-2">To Communicate With You</h4>
                  <ul className="list-disc list-inside space-y-1 text-gray-700">
                    <li>Send order confirmations and receipts</li>
                    <li>Notify architects of design approval or rejection decisions</li>
                    <li>Notify professionals of account approval status</li>
                    <li>Send password reset instructions</li>
                    <li>Provide customer support and respond to inquiries</li>
                  </ul>
                </div>
              </div>
            </section>

            {/* How We Share Your Information */}
            <section>
              <h2 className="text-2xl font-bold text-gray-900 mt-8 mb-4">4. How We Share Your Information</h2>
              <p className="text-gray-700 leading-relaxed mb-4">
                <strong>We do not sell, rent, or trade your personal information to third parties.</strong> We may share
                your information in the following circumstances:
              </p>

              <div className="space-y-4">
                <div className="border-l-4 border-blue-500 pl-4">
                  <h4 className="font-semibold text-gray-900 mb-2">Paystack (Payment Processing)</h4>
                  <p className="text-gray-700 text-sm">
                    We use Paystack to process payments securely. Paystack is PCI-DSS compliant and handles all payment
                    card data. We never store credit card numbers on our servers.
                  </p>
                </div>

                <div className="border-l-4 border-green-500 pl-4">
                  <h4 className="font-semibold text-gray-900 mb-2">DigitalOcean Spaces (File Storage)</h4>
                  <p className="text-gray-700 text-sm">
                    Design files and professional documents are stored on DigitalOcean Spaces. Download URLs are
                    time-limited and cryptographically signed (60-minute expiration).
                  </p>
                </div>

                <div className="border-l-4 border-purple-500 pl-4">
                  <h4 className="font-semibold text-gray-900 mb-2">Email Service Providers</h4>
                  <p className="text-gray-700 text-sm">
                    We use email service providers to send transactional emails including order confirmations,
                    account notifications, and password resets.
                  </p>
                </div>
              </div>
            </section>

            {/* Data Security */}
            <section>
              <h2 className="text-2xl font-bold text-gray-900 mt-8 mb-4">5. Data Security</h2>
              <p className="text-gray-700 leading-relaxed mb-4">
                We implement appropriate technical and organizational security measures to protect your personal information:
              </p>

              <div className="bg-green-50 rounded-lg p-4 space-y-2">
                <div className="flex items-start">
                  <span className="text-green-600 mr-2">✓</span>
                  <span className="text-gray-700"><strong>Encryption:</strong> Passwords are securely hashed using ASP.NET Identity</span>
                </div>
                <div className="flex items-start">
                  <span className="text-green-600 mr-2">✓</span>
                  <span className="text-gray-700"><strong>HTTPS/TLS:</strong> All data transmission is encrypted in transit</span>
                </div>
                <div className="flex items-start">
                  <span className="text-green-600 mr-2">✓</span>
                  <span className="text-gray-700"><strong>JWT Authentication:</strong> Secure, stateless authentication (60-minute expiration)</span>
                </div>
                <div className="flex items-start">
                  <span className="text-green-600 mr-2">✓</span>
                  <span className="text-gray-700"><strong>Signed URLs:</strong> File downloads are cryptographically signed with limited validity</span>
                </div>
                <div className="flex items-start">
                  <span className="text-green-600 mr-2">✓</span>
                  <span className="text-gray-700"><strong>Role-Based Access:</strong> Strict access controls restrict data access</span>
                </div>
              </div>

              <p className="text-gray-600 text-sm mt-4 italic">
                Despite our efforts, no electronic transmission or storage system is 100% secure. We cannot guarantee
                absolute security of your information.
              </p>
            </section>

            {/* Your Privacy Rights */}
            <section>
              <h2 className="text-2xl font-bold text-gray-900 mt-8 mb-4">6. Your Privacy Rights</h2>
              <p className="text-gray-700 leading-relaxed mb-4">
                Depending on your location, you may have the following rights regarding your personal information:
              </p>

              <ul className="space-y-2 text-gray-700">
                <li className="flex items-start">
                  <span className="text-blue-600 mr-2">•</span>
                  <span><strong>Access:</strong> Request a copy of the personal information we hold about you</span>
                </li>
                <li className="flex items-start">
                  <span className="text-blue-600 mr-2">•</span>
                  <span><strong>Correction:</strong> Request correction of inaccurate or incomplete information</span>
                </li>
                <li className="flex items-start">
                  <span className="text-blue-600 mr-2">•</span>
                  <span><strong>Deletion:</strong> Request deletion of your personal information (subject to legal obligations)</span>
                </li>
                <li className="flex items-start">
                  <span className="text-blue-600 mr-2">•</span>
                  <span><strong>Data Portability:</strong> Request transfer of your information to another service</span>
                </li>
              </ul>

              <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mt-4">
                <h4 className="font-semibold text-gray-900 mb-2">How to Exercise Your Rights</h4>
                <p className="text-gray-700 text-sm mb-2">To exercise any of these rights, please contact us at:</p>
                <p className="text-gray-700 text-sm">
                  <strong>Email:</strong> privacy@planmorph.com<br />
                  <strong>Subject Line:</strong> &quot;Privacy Rights Request&quot;
                </p>
                <p className="text-gray-700 text-sm mt-2">
                  We will respond to your request within 30 days.
                </p>
              </div>
            </section>

            {/* Regional Privacy Rights */}
            <section>
              <h2 className="text-2xl font-bold text-gray-900 mt-8 mb-4">7. Regional Privacy Rights</h2>

              <div className="space-y-4">
                <div className="border rounded-lg p-4">
                  <h4 className="font-semibold text-gray-900 mb-2">European Economic Area (EEA) - GDPR</h4>
                  <p className="text-gray-700 text-sm">
                    If you are located in the EEA, you have additional rights under the General Data Protection
                    Regulation (GDPR) including the right to lodge a complaint with your local supervisory authority
                    and the right to data portability in a structured, machine-readable format.
                  </p>
                  <p className="text-gray-700 text-sm mt-2">
                    <strong>Data Protection Officer:</strong> dpo@planmorph.com
                  </p>
                </div>

                <div className="border rounded-lg p-4">
                  <h4 className="font-semibold text-gray-900 mb-2">California - CCPA/CPRA</h4>
                  <p className="text-gray-700 text-sm">
                    If you are a California resident, you have rights under the California Consumer Privacy Act
                    (CCPA) including the right to know what personal information is collected and the right to
                    deletion.
                  </p>
                  <p className="text-gray-700 text-sm mt-2 font-semibold">
                    We Do NOT Sell Personal Information.
                  </p>
                </div>

                <div className="border rounded-lg p-4">
                  <h4 className="font-semibold text-gray-900 mb-2">Kenya - Data Protection Act</h4>
                  <p className="text-gray-700 text-sm">
                    If you are located in Kenya, you have rights under the Kenya Data Protection Act (2019)
                    including the right to access, object to processing, correct, delete, restrict processing,
                    and data portability. You may lodge a complaint with the Office of the Data Protection Commissioner.
                  </p>
                  <p className="text-gray-700 text-sm mt-2">
                    <strong>Kenya Contact:</strong> kenya@planmorph.com
                  </p>
                </div>
              </div>
            </section>

            {/* Children's Privacy */}
            <section>
              <h2 className="text-2xl font-bold text-gray-900 mt-8 mb-4">8. Children's Privacy</h2>
              <div className="bg-red-50 border border-red-200 rounded-lg p-4">
                <p className="text-gray-700 leading-relaxed">
                  PlanMorph is not intended for individuals under the age of 18. We do not knowingly collect personal
                  information from children under 18. If you are a parent or guardian and believe your child has
                  provided us with personal information, please contact us at privacy@planmorph.com, and we will
                  delete such information.
                </p>
              </div>
            </section>

            {/* Changes to Privacy Policy */}
            <section>
              <h2 className="text-2xl font-bold text-gray-900 mt-8 mb-4">9. Changes to This Privacy Policy</h2>
              <p className="text-gray-700 leading-relaxed">
                We may update this Privacy Policy from time to time. The &quot;Last Updated&quot; date at the top of this policy
                indicates when it was last revised. We will notify you of material changes by posting a prominent
                notice on our Platform or sending an email to your registered email address.
              </p>
            </section>

            {/* Contact Us */}
            <section>
              <h2 className="text-2xl font-bold text-gray-900 mt-8 mb-4">10. Contact Us</h2>
              <p className="text-gray-700 leading-relaxed mb-4">
                If you have questions, concerns, or requests regarding this Privacy Policy or our privacy practices,
                please contact us:
              </p>

              <div className="bg-gray-100 rounded-lg p-6 space-y-2">
                <div>
                  <strong className="text-gray-900">General Privacy Inquiries:</strong>
                  <p className="text-gray-700">Email: privacy@planmorph.com</p>
                  <p className="text-gray-700">Support Email: support@planmorph.com</p>
                  <p className="text-gray-700">Website: https://planmorph.com/contact</p>
                </div>
                <div className="mt-4">
                  <strong className="text-gray-900">Data Protection Officer:</strong>
                  <p className="text-gray-700">Email: dpo@planmorph.com</p>
                </div>
                <div className="mt-4">
                  <strong className="text-gray-900">Response Time:</strong>
                  <p className="text-gray-700">
                    We aim to respond to all privacy inquiries within 5 business days and fulfill rights requests
                    within 30 days.
                  </p>
                </div>
              </div>
            </section>

            {/* Footer Note */}
            <div className="mt-12 pt-8 border-t border-gray-200">
              <p className="text-sm text-gray-600 text-center">
                For the most current version of this policy, please visit:{' '}
                <a href="/privacy-policy" className="text-blue-600 hover:underline">
                  https://planmorph.com/privacy-policy
                </a>
              </p>
            </div>
          </div>
        </div>
      </div>
    </Layout>
  );
}
