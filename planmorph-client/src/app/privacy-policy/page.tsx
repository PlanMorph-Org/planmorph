'use client';

import Layout from '@/src/components/Layout';

export default function PrivacyPolicyPage() {
  return (
    <Layout>
      <div className="max-w-4xl mx-auto py-12 px-4 sm:px-6 lg:px-8">
        <div className="glass-card rounded-2xl p-8 border border-white/10">
          <h1 className="text-3xl font-display font-bold text-white mb-2">Privacy Policy</h1>
          <p className="text-xs text-white/30 mb-8">Last Updated: February 11, 2026</p>

          <div className="max-w-none space-y-8 text-sm">
            <section>
              <h2 className="text-lg font-semibold text-white mt-6 mb-3">1. Introduction</h2>
              <p className="text-white/50 leading-relaxed">
                Welcome to PlanMorph (&quot;we,&quot; &quot;our,&quot; or &quot;us&quot;). PlanMorph operates a platform for publishing and licensing
                architectural and civil design packages, connecting verified professionals with clients seeking build-ready documentation and construction services. We are committed to protecting your personal information and your right to privacy.
              </p>
              <p className="text-white/50 leading-relaxed mt-3">
                This Privacy Policy explains how we collect, use, disclose, and safeguard your information when you use
                our platform at planmorph.software (the &quot;Platform&quot;). Please read this privacy policy carefully.
              </p>
            </section>

            <section>
              <h2 className="text-lg font-semibold text-white mt-6 mb-3">2. Information We Collect</h2>
              <h3 className="text-base font-semibold text-white/70 mt-4 mb-2">2.1 Personal Information You Provide</h3>

              <div className="glass-card-light rounded-lg p-4 my-3">
                <h4 className="font-semibold text-white/60 text-xs uppercase tracking-widest mb-2">All Users</h4>
                <ul className="list-disc list-inside space-y-1 text-white/40"><li>Full name</li><li>Email address</li><li>Phone number</li><li>Password (encrypted)</li><li>Account preferences</li><li>User role and status</li></ul>
              </div>
              <div className="glass-card-light rounded-lg p-4 my-3 border border-golden/10">
                <h4 className="font-semibold text-golden/60 text-xs uppercase tracking-widest mb-2">Professional Users (Architects & Engineers)</h4>
                <ul className="list-disc list-inside space-y-1 text-white/40"><li>Professional license number</li><li>Years of experience</li><li>Portfolio URL</li><li>Specialization</li><li>Professional documents (CV, cover letter, work experience)</li><li>Document metadata</li></ul>
              </div>
            </section>

            <section>
              <h2 className="text-lg font-semibold text-white mt-6 mb-3">3. How We Use Your Information</h2>
              <div className="space-y-3">
                <div><h4 className="font-semibold text-white/60 mb-1">To Provide and Maintain Our Services</h4>
                  <ul className="list-disc list-inside space-y-1 text-white/40"><li>Process registrations and manage accounts</li><li>Authenticate users and maintain security</li><li>Enable design browsing, filtering, and purchasing</li><li>Facilitate construction requests (Kenya only)</li><li>Process payments</li><li>Deliver purchased files</li></ul>
                </div>
                <div><h4 className="font-semibold text-white/60 mb-1">To Communicate With You</h4>
                  <ul className="list-disc list-inside space-y-1 text-white/40"><li>Order confirmations and receipts</li><li>Design approval/rejection notifications</li><li>Account approval status</li><li>Password reset instructions</li><li>Customer support</li></ul>
                </div>
              </div>
            </section>

            <section>
              <h2 className="text-lg font-semibold text-white mt-6 mb-3">4. How We Share Your Information</h2>
              <p className="text-white/50 leading-relaxed mb-3"><strong className="text-white/70">We do not sell, rent, or trade your personal information.</strong></p>
              <div className="space-y-3">
                {[
                  { color: 'border-brand-accent/40', title: 'Paystack (Payment Processing)', desc: 'PCI-DSS compliant. We never store credit card numbers.' },
                  { color: 'border-verified/40', title: 'DigitalOcean Spaces (File Storage)', desc: 'Time-limited, cryptographically signed URLs (60-min expiration).' },
                  { color: 'border-purple-500/40', title: 'Email Service Providers', desc: 'Transactional emails: confirmations, notifications, resets.' },
                ].map(item => (
                  <div key={item.title} className={`border-l-2 ${item.color} pl-4`}>
                    <h4 className="font-semibold text-white/60 mb-1">{item.title}</h4>
                    <p className="text-white/40 text-xs">{item.desc}</p>
                  </div>
                ))}
              </div>
            </section>

            <section>
              <h2 className="text-lg font-semibold text-white mt-6 mb-3">5. Data Security</h2>
              <div className="glass-card-light rounded-lg p-4 space-y-2 border border-verified/10">
                {['Encryption: Passwords securely hashed via ASP.NET Identity', 'HTTPS/TLS: All data encrypted in transit', 'JWT Authentication: Secure, stateless (60-min expiry)', 'Signed URLs: Cryptographically signed file downloads', 'Role-Based Access: Strict access controls'].map(item => (
                  <div key={item} className="flex items-start"><span className="text-verified mr-2 text-xs">✓</span><span className="text-white/40 text-xs">{item}</span></div>
                ))}
              </div>
              <p className="text-white/20 text-xs mt-2 italic">No system is 100% secure. We cannot guarantee absolute security.</p>
            </section>

            <section>
              <h2 className="text-lg font-semibold text-white mt-6 mb-3">6. Your Privacy Rights</h2>
              <ul className="space-y-1.5 text-white/40">
                {['Access: Request a copy of your personal information', 'Correction: Request correction of inaccurate data', 'Deletion: Request deletion (subject to legal obligations)', 'Data Portability: Request transfer to another service'].map(item => (
                  <li key={item} className="flex items-start"><span className="text-golden mr-2">•</span><span>{item}</span></li>
                ))}
              </ul>
              <div className="glass-card-light rounded-lg p-4 mt-3 border border-golden/10">
                <h4 className="font-semibold text-golden/60 text-xs uppercase tracking-widest mb-1">How to Exercise Your Rights</h4>
                <p className="text-white/40 text-xs">Email: privacy@planmorph.software — Subject: &quot;Privacy Rights Request&quot; — Response within 30 days.</p>
              </div>
            </section>

            <section>
              <h2 className="text-lg font-semibold text-white mt-6 mb-3">7. Regional Privacy Rights</h2>
              <div className="space-y-3">
                {[
                  { region: 'European Economic Area (EEA) — GDPR', text: 'Additional rights under GDPR. DPO: dpo@planmorph.software' },
                  { region: 'California — CCPA/CPRA', text: 'Right to know and delete. We Do NOT Sell Personal Information.' },
                  { region: 'Kenya — Data Protection Act', text: 'Rights under Kenya DPA (2019). Contact: kenya@planmorph.software' },
                ].map(item => (
                  <div key={item.region} className="glass-card-light rounded-lg p-4 border border-white/6">
                    <h4 className="font-semibold text-white/60 mb-1 text-xs">{item.region}</h4>
                    <p className="text-white/40 text-xs">{item.text}</p>
                  </div>
                ))}
              </div>
            </section>

            <section>
              <h2 className="text-lg font-semibold text-white mt-6 mb-3">8. Children&apos;s Privacy</h2>
              <div className="glass-card-light rounded-lg p-4 border border-rose-500/10">
                <p className="text-white/40 text-xs">PlanMorph is not intended for individuals under 18. Contact privacy@planmorph.software if a child has provided personal information.</p>
              </div>
            </section>

            <section>
              <h2 className="text-lg font-semibold text-white mt-6 mb-3">9. Changes to This Privacy Policy</h2>
              <p className="text-white/40">We may update this policy. The &quot;Last Updated&quot; date indicates the last revision. Material changes will be communicated via Platform notice or email.</p>
            </section>

            <section>
              <h2 className="text-lg font-semibold text-white mt-6 mb-3">10. Contact Us</h2>
              <div className="glass-card-light rounded-lg p-5 space-y-2 text-xs">
                <div><strong className="text-white/60">General Privacy:</strong> <span className="text-white/40">privacy@planmorph.software</span></div>
                <div><strong className="text-white/60">Support:</strong> <span className="text-white/40">support@planmorph.software</span></div>
                <div><strong className="text-white/60">Data Protection Officer:</strong> <span className="text-white/40">dpo@planmorph.software</span></div>
                <div><strong className="text-white/60">Response Time:</strong> <span className="text-white/40">5 business days (inquiries), 30 days (rights requests)</span></div>
              </div>
            </section>

            <div className="mt-10 pt-6 border-t border-white/6">
              <p className="text-xs text-white/20 text-center">
                Current version: <a href="/privacy-policy" className="text-golden/40 hover:text-golden transition-colors">planmorph.software/privacy-policy</a>
              </p>
            </div>
          </div>
        </div>
      </div>
    </Layout>
  );
}
