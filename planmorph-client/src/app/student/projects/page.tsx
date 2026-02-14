'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import StudentLayout from '@/src/components/StudentLayout';
import api from '@/src/lib/api';
import { motion } from 'framer-motion';
import type { MentorshipProject } from '@/src/types';

export default function StudentProjectsPage() {
  const router = useRouter();
  const [projects, setProjects] = useState<MentorshipProject[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function loadProjects() {
      try {
        const res = await api.get('/student/projects');
        setProjects(res.data);
      } catch {
        // No projects found
      } finally {
        setLoading(false);
      }
    }
    loadProjects();
  }, []);

  const statusColors: Record<string, string> = {
    InProgress: 'bg-indigo/15 text-indigo border-indigo/20',
    UnderMentorReview: 'bg-amber-500/15 text-amber-400 border-amber-500/20',
    RevisionRequested: 'bg-orange-500/15 text-orange-400 border-orange-500/20',
    MentorApproved: 'bg-verified/15 text-verified border-verified/20',
    ClientReview: 'bg-brand-accent/15 text-brand-accent border-brand-accent/20',
    ClientRevisionRequested: 'bg-orange-500/15 text-orange-400 border-orange-500/20',
    Completed: 'bg-verified/15 text-verified border-verified/20',
    Paid: 'bg-verified/15 text-verified border-verified/20',
    StudentAssigned: 'bg-brand-accent/15 text-brand-accent border-brand-accent/20',
  };

  const priorityColors: Record<string, string> = {
    Low: 'text-white/30',
    Medium: 'text-amber-400',
    High: 'text-red-400',
  };

  return (
    <StudentLayout>
      <div className="pt-24 pb-16 px-4 sm:px-6 lg:px-8 max-w-6xl mx-auto">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5, ease: [0.16, 1, 0.3, 1] }}
        >
          <h1 className="text-2xl font-display font-bold text-white mb-2">My Projects</h1>
          <p className="text-sm text-white/35 mb-8">Projects assigned to you by your mentor.</p>

          {loading ? (
            <div className="space-y-4">
              {[1, 2, 3].map(i => <div key={i} className="h-24 shimmer-bg rounded-xl" />)}
            </div>
          ) : projects.length === 0 ? (
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
            <div className="space-y-3">
              {projects.map((project, idx) => (
                <motion.div
                  key={project.id}
                  initial={{ opacity: 0, y: 15 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: idx * 0.05, duration: 0.4, ease: [0.16, 1, 0.3, 1] }}
                  onClick={() => router.push(`/student/projects/${project.id}`)}
                  className="glass-card rounded-xl p-5 cursor-pointer hover:bg-white/[0.03] transition-colors"
                >
                  <div className="flex items-start justify-between gap-4">
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2 mb-1">
                        <h3 className="text-sm font-medium text-white truncate">{project.title}</h3>
                        <span className={`inline-block px-2 py-0.5 text-[10px] font-medium rounded-full border ${statusColors[project.status] ?? 'bg-white/10 text-white/50 border-white/15'}`}>
                          {project.status.replace(/([A-Z])/g, ' $1').trim()}
                        </span>
                      </div>
                      <div className="flex items-center gap-3 text-xs text-white/30">
                        <span>{project.projectNumber}</span>
                        <span className="w-1 h-1 rounded-full bg-white/15" />
                        <span>{project.category}</span>
                        <span className="w-1 h-1 rounded-full bg-white/15" />
                        <span className={priorityColors[project.priority] ?? 'text-white/30'}>{project.priority} priority</span>
                      </div>
                    </div>
                    <div className="text-right flex-shrink-0">
                      {project.studentDeadline && (
                        <div className="text-xs text-white/30">
                          Due: {new Date(project.studentDeadline).toLocaleDateString()}
                        </div>
                      )}
                      <div className="text-xs text-white/20 mt-0.5">
                        {new Date(project.createdAt).toLocaleDateString()}
                      </div>
                    </div>
                  </div>
                </motion.div>
              ))}
            </div>
          )}
        </motion.div>
      </div>
    </StudentLayout>
  );
}
