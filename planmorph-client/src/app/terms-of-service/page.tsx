'use client';

import Layout from '@/src/components/Layout';

export default function TermsOfServicePage() {
  return (
    <Layout>
      <div className="max-w-4xl mx-auto py-12 px-4 sm:px-6 lg:px-8">
        <div className="glass-card rounded-2xl p-8 border border-white/10">
          <h1 className="text-3xl font-display font-bold text-white mb-2">Terms of Service</h1>
          <p className="text-xs text-white/30 mb-8">Last Updated: February 11, 2026</p>

          <div className="max-w-none space-y-8 text-sm">
            {/* 1. Agreement to Terms */}
            <section>
              <h2 className="text-lg font-semibold text-white mt-6 mb-3">1. Agreement to Terms</h2>
              <p className="text-white/50 leading-relaxed">
                Welcome to PlanMorph. These Terms of Service (&quot;Terms&quot;) govern your access to and use of the
                PlanMorph platform, website, and services. By accessing or using our Platform, you agree to these Terms and our Privacy Policy.
              </p>
              <div className="glass-card-light rounded-lg p-4 mt-3 border border-golden/20">
                <p className="text-golden/80 font-semibold text-xs">IF YOU DO NOT AGREE TO THESE TERMS, YOU MAY NOT ACCESS OR USE THE PLATFORM.</p>
              </div>
            </section>

            {/* 2. Eligibility */}
            <section>
              <h2 className="text-lg font-semibold text-white mt-6 mb-3">2. Eligibility and Registration</h2>
              <div className="glass-card-light rounded-lg p-4 mb-4 border border-brand-accent/10">
                <h4 className="font-semibold text-white/60 text-xs uppercase tracking-widest mb-1">Age Requirement</h4>
                <p className="text-white/40 text-xs">You must be at least 18 years old to use the Platform.</p>
              </div>
              <h3 className="text-base font-semibold text-white/70 mt-4 mb-3">Account Types</h3>
              <div className="grid md:grid-cols-3 gap-3">
                {[
                  { title: 'Client', color: 'text-brand-accent', items: ['Browse & purchase designs', 'Download files', 'Request construction', 'Track orders'] },
                  { title: 'Architect / Engineer', color: 'text-golden', items: ['Upload & sell designs', 'Earn 70% commission', 'Manage portfolio', 'Track earnings'] },
                  { title: 'Contractor', color: 'text-purple-400', items: ['Receive assignments', 'View project details', 'Construction (Kenya)'] },
                ].map(card => (
                  <div key={card.title} className="glass-card-light rounded-lg p-4 border border-white/6">
                    <h4 className={`font-semibold ${card.color} mb-2 text-xs`}>{card.title}</h4>
                    <ul className="text-xs text-white/40 space-y-1 list-disc list-inside">{card.items.map(i => <li key={i}>{i}</li>)}</ul>
                  </div>
                ))}
              </div>
            </section>

            {/* 3. Platform Description */}
            <section>
              <h2 className="text-lg font-semibold text-white mt-6 mb-3">3. Platform Description</h2>
              <p className="text-white/50 leading-relaxed mb-3">PlanMorph is a global online marketplace connecting clients with architects and engineers.</p>
              <div className="glass-card-light rounded-lg p-4 border border-verified/10">
                <h4 className="font-semibold text-verified/60 text-xs uppercase tracking-widest mb-1">Construction Services</h4>
                <p className="text-white/40 text-xs">Currently available <strong className="text-white/60">ONLY in Kenya</strong>. Other countries: designs only.</p>
              </div>
            </section>

            {/* 4. Payments */}
            <section>
              <h2 className="text-lg font-semibold text-white mt-6 mb-3">4. Payments and Financial Terms</h2>
              <h3 className="text-base font-semibold text-white/70 mt-4 mb-3">Commission Structure</h3>
              <div className="glass-card-light rounded-lg p-5 space-y-4 border border-white/6">
                {[
                  { pct: '70%', title: 'Architects & Engineers', desc: 'Receive 70% of design sale price', bg: 'bg-brand-accent/20 text-brand-accent' },
                  { pct: '30%', title: 'Platform Commission', desc: 'PlanMorph retains 30% for maintenance and support', bg: 'bg-verified/20 text-verified' },
                  { pct: '2%', title: 'Construction Commission', desc: '2% on construction costs for contractor matching', bg: 'bg-purple-500/20 text-purple-400' },
                ].map(item => (
                  <div key={item.pct} className="flex items-start gap-3">
                    <div className={`flex items-center justify-center h-10 w-10 rounded-lg ${item.bg} text-sm font-bold shrink-0`}>{item.pct}</div>
                    <div><h4 className="text-sm font-semibold text-white/60">{item.title}</h4><p className="text-white/40 text-xs">{item.desc}</p></div>
                  </div>
                ))}
              </div>

              <h3 className="text-base font-semibold text-white/70 mt-4 mb-2">Payment Processing</h3>
              <p className="text-white/40 text-xs">All payments processed in KES via Paystack. Multi-currency display for convenience.</p>

              <h3 className="text-base font-semibold text-white/70 mt-4 mb-2">Refund Policy</h3>
              <div className="glass-card-light rounded-lg p-4 border border-rose-500/10">
                <p className="text-white/40 text-xs"><strong className="text-rose-400">No refunds</strong> after files downloaded. Exceptions: corrupted files, misrepresentation, or duplicate purchase (within 24h). Requests within 7 days.</p>
              </div>
            </section>

            {/* 5. Intellectual Property */}
            <section>
              <h2 className="text-lg font-semibold text-white mt-6 mb-3">5. Design Submissions and Intellectual Property</h2>
              <div className="space-y-3">
                {[
                  { color: 'border-brand-accent/40', title: 'Architect Retains Copyright', desc: 'Architects retain full copyright. PlanMorph does not claim ownership.' },
                  { color: 'border-verified/40', title: 'Platform License', desc: 'Non-exclusive, worldwide, royalty-free license to display, preview, promote, and distribute to buyers.' },
                  { color: 'border-purple-500/40', title: 'Client License', desc: 'Non-exclusive, non-transferable license for ONE construction project. No resale or redistribution.' },
                ].map(item => (
                  <div key={item.title} className={`border-l-2 ${item.color} pl-4`}>
                    <h4 className="font-semibold text-white/60 mb-1 text-xs">{item.title}</h4>
                    <p className="text-white/40 text-xs">{item.desc}</p>
                  </div>
                ))}
              </div>
            </section>

            {/* 6. Construction Services */}
            <section>
              <h2 className="text-lg font-semibold text-white mt-6 mb-3">6. Construction Services (Kenya Only)</h2>
              <div className="glass-card-light rounded-lg p-4 mb-3 border border-verified/20">
                <h4 className="font-semibold text-verified/60 text-xs uppercase tracking-widest mb-1">Geographic Limitation</h4>
                <p className="text-white/40 text-xs">Construction services <strong className="text-white/60">exclusively in Kenya</strong>.</p>
              </div>
              <div className="glass-card-light rounded-lg p-4 border border-golden/10">
                <p className="text-white/40 text-xs mb-2"><strong className="text-golden/60">PlanMorph is a facilitator only.</strong> We facilitate connection, provide contract management, collect 2% commission, and mediate disputes.</p>
                <p className="text-white/40 text-xs"><strong className="text-rose-400/60">PlanMorph is NOT:</strong> a construction company, responsible for quality, liable for delays/defects, or a party to construction contracts.</p>
              </div>
            </section>

            {/* 7. Disclaimers */}
            <section>
              <h2 className="text-lg font-semibold text-white mt-6 mb-3">7. Disclaimers and Limitations of Liability</h2>
              <div className="glass-card-light rounded-lg p-4 border border-rose-500/20">
                <h4 className="font-semibold text-rose-400/60 text-xs uppercase tracking-widest mb-2">Platform &quot;As Is&quot;</h4>
                <p className="text-white/40 text-xs mb-3">THE PLATFORM IS PROVIDED &quot;AS IS&quot; AND &quot;AS AVAILABLE&quot; WITHOUT WARRANTIES OF ANY KIND.</p>
                <p className="text-white/40 text-xs mb-1"><strong className="text-white/60">PlanMorph does not guarantee:</strong> design accuracy, building code compliance, structural soundness, or suitability for any location.</p>
                <p className="text-white/40 text-xs"><strong className="text-white/60">Clients are responsible for:</strong> verifying with local professionals, obtaining permits, ensuring compliance, modifying for site conditions.</p>
              </div>
              <div className="glass-card-light rounded-lg p-4 mt-3 border border-white/6">
                <h4 className="font-semibold text-white/60 text-xs mb-1">Total Liability Cap</h4>
                <p className="text-white/40 text-xs">Greater of: (1) fees paid in prior 12 months, or (2) KES 50,000.</p>
              </div>
            </section>

            {/* 8. User Conduct */}
            <section>
              <h2 className="text-lg font-semibold text-white mt-6 mb-3">8. User Conduct and Prohibited Activities</h2>
              <div className="grid md:grid-cols-2 gap-3">
                <div className="glass-card-light rounded-lg p-4 border border-rose-500/10">
                  <h4 className="font-semibold text-rose-400 mb-2 text-xs">❌ Prohibited</h4>
                  <ul className="text-xs text-white/40 space-y-1 list-disc list-inside"><li>False/misleading info</li><li>Upload designs you don&apos;t own</li><li>Copyright infringement</li><li>Payment fraud</li><li>Redistribute designs</li><li>Automated bots/scrapers</li></ul>
                </div>
                <div className="glass-card-light rounded-lg p-4 border border-verified/10">
                  <h4 className="font-semibold text-verified mb-2 text-xs">Consequences</h4>
                  <ul className="text-xs text-white/40 space-y-1 list-disc list-inside"><li>Warning</li><li>Temporary suspension</li><li>Permanent termination</li><li>Content removal</li><li>Commission forfeiture</li><li>Legal action</li></ul>
                </div>
              </div>
            </section>

            {/* 9. Dispute Resolution */}
            <section>
              <h2 className="text-lg font-semibold text-white mt-6 mb-3">9. Dispute Resolution</h2>
              <div className="space-y-2">
                {[
                  { step: '1', title: 'Informal Resolution', desc: 'Contact disputes@planmorph.com. Resolution within 30 days.' },
                  { step: '2', title: 'Mediation', desc: 'Neutral mediator. Costs shared equally.' },
                  { step: '3', title: 'Arbitration', desc: 'Kenya: NCIA. International: ICC. Decision is final.' },
                ].map(item => (
                  <div key={item.step} className="glass-card-light rounded-lg p-4 border border-white/6">
                    <h4 className="font-semibold text-white/60 mb-1 text-xs">Step {item.step}: {item.title}</h4>
                    <p className="text-white/40 text-xs">{item.desc}</p>
                  </div>
                ))}
              </div>
              <div className="glass-card-light rounded-lg p-4 mt-3 border border-golden/20">
                <h4 className="font-semibold text-golden/60 text-xs uppercase tracking-widest mb-1">Class Action Waiver</h4>
                <p className="text-white/40 text-xs">Disputes resolved individually only. No class actions.</p>
              </div>
            </section>

            {/* 10. Governing Law */}
            <section>
              <h2 className="text-lg font-semibold text-white mt-6 mb-3">10. Governing Law</h2>
              <div className="glass-card-light rounded-lg p-4 border border-white/6">
                <p className="text-white/40 text-xs">Governed by the laws of the <strong className="text-white/60">Republic of Kenya</strong>. Jurisdiction: <strong className="text-white/60">Nairobi, Kenya</strong>.</p>
              </div>
            </section>

            {/* 11. Modifications */}
            <section>
              <h2 className="text-lg font-semibold text-white mt-6 mb-3">11. Modifications to Terms</h2>
              <p className="text-white/40">We may modify these Terms. Material changes effective 30 days after notice. Continued use constitutes acceptance.</p>
            </section>

            {/* 12. Contact */}
            <section>
              <h2 className="text-lg font-semibold text-white mt-6 mb-3">12. Contact Information</h2>
              <div className="glass-card-light rounded-lg p-5 space-y-2 text-xs">
                <div><strong className="text-white/60">General Support:</strong> <span className="text-white/40">support@planmorph.com</span></div>
                <div><strong className="text-white/60">Legal Inquiries:</strong> <span className="text-white/40">legal@planmorph.com</span></div>
                <div><strong className="text-white/60">Dispute Resolution:</strong> <span className="text-white/40">disputes@planmorph.com</span></div>
                <div><strong className="text-white/60">Copyright Claims:</strong> <span className="text-white/40">dmca@planmorph.com</span></div>
              </div>
            </section>

            {/* Acceptance */}
            <div className="mt-10 pt-6 border-t border-white/6">
              <div className="glass-card-light rounded-lg p-5 text-center border border-golden/20">
                <p className="text-white/60 font-semibold text-xs">By using PlanMorph, you acknowledge that you have read, understood, and agree to these Terms of Service.</p>
                <p className="text-white/20 text-xs mt-1">Effective Date: February 11, 2026 | Version: 1.0</p>
              </div>
            </div>

            <div className="mt-4">
              <p className="text-xs text-white/20 text-center">
                Current version: <a href="/terms-of-service" className="text-golden/40 hover:text-golden transition-colors">planmorph.com/terms-of-service</a>
              </p>
            </div>
          </div>
        </div>
      </div>
    </Layout>
  );
}
