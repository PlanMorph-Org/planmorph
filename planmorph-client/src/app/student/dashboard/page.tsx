'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import StudentLayout from '@/src/components/StudentLayout';
import api from '@/src/lib/api';
import { motion } from 'framer-motion';
import type { StudentProfile, MentorshipProject } from '@/src/types';

export default function StudentDashboardPage() {
  const router = useRouter();
  const [profile, setProfile] = useState<StudentProfile | null>(null);
  const [projects, setProjects] = useState<MentorshipProject[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function loadData() {
      try {
        const [profileRes, projectsRes] = await Promise.all([
          api.get('/student/profile'),
          api.get('/student/projects'),
        ]);
        setProfile(profileRes.data);
        setProjects(projectsRes.data);
      } catch {
        // Profile may not exist yet if just approved
      } finally {
        setLoading(false);
      }
    }
    loadData();
  }, []);

  const fadeUp = {
    hidden: { opacity: 0, y: 20 },
    visible: (i: number) => ({
      opacity: 1,
      y: 0,
      transition: { delay: i * 0.08, duration: 0.5, ease: [0.16, 1, 0.3, 1] },
    }),
  };

  const statusColors: Record<string, string> = {
    InProgress: 'bg-indigo/15 text-indigo border-indigo/20',
    UnderMentorReview: 'bg-amber-500/15 text-amber-400 border-amber-500/20',
    RevisionRequested: 'bg-orange-500/15 text-orange-400 border-orange-500/20',
    MentorApproved: 'bg-verified/15 text-verified border-verified/20',
    Completed: 'bg-verified/15 text-verified border-verified/20',
    Paid: 'bg-verified/15 text-verified border-verified/20',
    StudentAssigned: 'bg-brand-accent/15 text-brand-accent border-brand-accent/20',
  };

  const mentorshipStatusColors: Record<string, string> = {
    Unmatched: 'bg-white/10 text-white/50 border-white/15',
    Matched: 'bg-indigo/15 text-indigo border-indigo/20',
    Active: 'bg-verified/15 text-verified border-verified/20',
    Suspended: 'bg-red-500/15 text-red-400 border-red-500/20',
  };

  return (
    <StudentLayout>
      <div className="pt-24 pb-16 px-4 sm:px-6 lg:px-8 max-w-6xl mx-auto">
        {loading ? (
          <div className="space-y-6">
            <div className="h-8 w-64 shimmer-bg rounded-lg" />
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
              {[1, 2, 3].map(i => <div key={i} className="h-28 shimmer-bg rounded-xl" />)}
            </div>
            <div className="h-64 shimmer-bg rounded-xl" />
          </div>
        ) : (
          <>
            {/* Welcome */}
            <motion.div custom={0} initial="hidden" animate="visible" variants={fadeUp} className="mb-8">
              <h1 className="text-2xl font-display font-bold text-white">
                Welcome back, {profile?.firstName ?? 'Student'}
              </h1>
              <p className="text-sm text-white/35 mt-1">Here&apos;s an overview of your mentorship activity.</p>
            </motion.div>

            {/* Stats */}
            <motion.div custom={1} initial="hidden" animate="visible" variants={fadeUp} className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-8">
              <div className="glass-card rounded-xl p-5">
                <div className="flex items-center gap-3 mb-3">
                  <div className="w-10 h-10 rounded-lg bg-indigo/10 flex items-center justify-center">
                    <svg className="w-5 h-5 text-indigo" fill="none" stroke="currentColor" viewBox="0 0 24 24" strokeWidth={1.5}>
                      <path strokeLinecap="round" strokeLinejoin="round" d="M9 12h3.75M9 15h3.75M9 18h3.75m3 .75H18a2.25 2.25 0 002.25-2.25V6.108c0-1.135-.845-2.098-1.976-2.192a48.424 48.424 0 00-1.123-.08m-5.801 0c-.065.21-.1.433-.1.664 0 .414.336.75.75.75h4.5a.75.75 0 00.75-.75 2.25 2.25 0 00-.1-.664m-5.8 0A2.251 2.251 0 0113.5 2.25H15a2.25 2.25 0 012.15 1.586m-5.8 0c-.376.023-.75.05-1.124.08C9.095 4.01 8.25 4.973 8.25 6.108V8.25m0 0H4.875c-.621 0-1.125.504-1.125 1.125v11.25c0 .621.504 1.125 1.125 1.125h9.75c.621 0 1.125-.504 1.125-1.125V9.375c0-.621-.504-1.125-1.125-1.125H8.25z" />
                    </svg>
                  </div>
                  <div>
                    <div className="text-2xl font-bold text-white">{profile?.totalProjectsCompleted ?? 0}</div>
                    <div className="text-xs text-white/35">Projects Completed</div>
                  </div>
                </div>
              </div>

              <div className="glass-card rounded-xl p-5">
                <div className="flex items-center gap-3 mb-3">
                  <div className="w-10 h-10 rounded-lg bg-verified/10 flex items-center justify-center">
                    <svg className="w-5 h-5 text-verified" fill="none" stroke="currentColor" viewBox="0 0 24 24" strokeWidth={1.5}>
                      <path strokeLinecap="round" strokeLinejoin="round" d="M2.25 18.75a60.07 60.07 0 0115.797 2.101c.727.198 1.453-.342 1.453-1.096V18.75M3.75 4.5v.75A.75.75 0 013 6h-.75m0 0v-.375c0-.621.504-1.125 1.125-1.125H20.25M2.25 6v9m18-10.5v.75c0 .414.336.75.75.75h.75m-1.5-1.5h.375c.621 0 1.125.504 1.125 1.125v9.75c0 .621-.504 1.125-1.125 1.125h-.375m1.5-1.5H21a.75.75 0 00-.75.75v.75m0 0H3.75m0 0h-.375a1.125 1.125 0 01-1.125-1.125V15m1.5 1.5v-.75A.75.75 0 003 15h-.75M15 10.5a3 3 0 11-6 0 3 3 0 016 0zm3 0h.008v.008H18V10.5zm-12 0h.008v.008H6V10.5z" />
                    </svg>
                  </div>
                  <div>
                    <div className="text-2xl font-bold text-white">KES {(profile?.totalEarnings ?? 0).toLocaleString()}</div>
                    <div className="text-xs text-white/35">Total Earnings</div>
                  </div>
                </div>
              </div>

              <div className="glass-card rounded-xl p-5">
                <div className="flex items-center gap-3 mb-3">
                  <div className="w-10 h-10 rounded-lg bg-brand-accent/10 flex items-center justify-center">
                    <svg className="w-5 h-5 text-brand-accent" fill="none" stroke="currentColor" viewBox="0 0 24 24" strokeWidth={1.5}>
                      <path strokeLinecap="round" strokeLinejoin="round" d="M15.75 6a3.75 3.75 0 11-7.5 0 3.75 3.75 0 017.5 0zM4.501 20.118a7.5 7.5 0 0114.998 0A17.933 17.933 0 0112 21.75c-2.676 0-5.216-.584-7.499-1.632z" />
                    </svg>
                  </div>
                  <div>
                    <span className={`inline-block px-2 py-0.5 text-xs font-medium rounded-full border ${mentorshipStatusColors[profile?.mentorshipStatus ?? 'Unmatched'] ?? mentorshipStatusColors.Unmatched}`}>
                      {profile?.mentorshipStatus ?? 'Unmatched'}
                    </span>
                    <div className="text-xs text-white/35 mt-1">Mentorship Status</div>
                  </div>
                </div>
              </div>
            </motion.div>

            {/* Mentor Info */}
            {profile?.mentorName && (
              <motion.div custom={2} initial="hidden" animate="visible" variants={fadeUp} className="glass-card rounded-xl p-5 mb-8">
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 rounded-full bg-gradient-to-br from-indigo to-indigo-light flex items-center justify-center text-sm font-bold text-white">
                    {profile.mentorName[0]}
                  </div>
                  <div>
                    <div className="text-sm font-medium text-white">{profile.mentorName}</div>
                    <div className="text-xs text-white/35">Your Mentor</div>
                  </div>
                </div>
              </motion.div>
            )}

            {/* Projects Table */}
            <motion.div custom={3} initial="hidden" animate="visible" variants={fadeUp}>
              <div className="flex items-center justify-between mb-4">
                <h2 className="text-lg font-display font-semibold text-white">Your Projects</h2>
                <span className="text-xs text-white/30">{projects.length} project{projects.length !== 1 ? 's' : ''}</span>
              </div>

              {projects.length === 0 ? (
                <div className="glass-card rounded-xl p-12 text-center">
                  <div className="w-16 h-16 mx-auto mb-4 rounded-full bg-white/5 flex items-center justify-center">
                    <svg className="w-8 h-8 text-white/20" fill="none" stroke="currentColor" viewBox="0 0 24 24" strokeWidth={1.5}>
                      <path strokeLinecap="round" strokeLinejoin="round" d="M2.25 12.75V12A2.25 2.25 0 014.5 9.75h15A2.25 2.25 0 0121.75 12v.75m-8.69-6.44l-2.12-2.12a1.5 1.5 0 00-1.061-.44H4.5A2.25 2.25 0 002.25 6v12a2.25 2.25 0 002.25 2.25h15A2.25 2.25 0 0021.75 18V9a2.25 2.25 0 00-2.25-2.25h-5.379a1.5 1.5 0 01-1.06-.44z" />
                    </svg>
                  </div>
                  <p className="text-white/40 text-sm">No projects assigned yet.</p>
                  <p className="text-white/25 text-xs mt-1">Your mentor will assign you projects when available.</p>
                </div>
              ) : (
                <div className="glass-card rounded-xl overflow-hidden">
                  <div className="overflow-x-auto">
                    <table className="w-full">
                      <thead>
                        <tr className="border-b border-white/6">
                          <th className="text-left text-xs font-medium text-white/30 px-5 py-3">Project</th>
                          <th className="text-left text-xs font-medium text-white/30 px-5 py-3 hidden sm:table-cell">Category</th>
                          <th className="text-left text-xs font-medium text-white/30 px-5 py-3">Status</th>
                          <th className="text-left text-xs font-medium text-white/30 px-5 py-3 hidden sm:table-cell">Date</th>
                        </tr>
                      </thead>
                      <tbody>
                        {projects.map((project) => (
                          <tr
                            key={project.id}
                            onClick={() => router.push(`/student/projects/${project.id}`)}
                            className="border-b border-white/4 hover:bg-white/[0.02] cursor-pointer transition-colors"
                          >
                            <td className="px-5 py-4">
                              <div className="text-sm font-medium text-white">{project.title}</div>
                              <div className="text-xs text-white/30 mt-0.5">{project.projectNumber}</div>
                            </td>
                            <td className="px-5 py-4 hidden sm:table-cell">
                              <span className="text-xs text-white/40">{project.category}</span>
                            </td>
                            <td className="px-5 py-4">
                              <span className={`inline-block px-2 py-0.5 text-xs font-medium rounded-full border ${statusColors[project.status] ?? 'bg-white/10 text-white/50 border-white/15'}`}>
                                {project.status.replace(/([A-Z])/g, ' $1').trim()}
                              </span>
                            </td>
                            <td className="px-5 py-4 hidden sm:table-cell">
                              <span className="text-xs text-white/30">{new Date(project.createdAt).toLocaleDateString()}</span>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>
              )}
            </motion.div>
          </>
        )}
      </div>
    </StudentLayout>
  );
}
