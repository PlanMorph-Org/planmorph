'use client';

import { useEffect, useState } from 'react';
import StudentLayout from '@/src/components/StudentLayout';
import api from '@/src/lib/api';
import toast, { Toaster } from 'react-hot-toast';
import { motion } from 'framer-motion';
import type { StudentProfile } from '@/src/types';

export default function StudentProfilePage() {
  const [profile, setProfile] = useState<StudentProfile | null>(null);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState(false);
  const [saving, setSaving] = useState(false);
  const [editForm, setEditForm] = useState({
    universityName: '',
    enrollmentStatus: '',
    expectedGraduation: '',
  });

  useEffect(() => {
    async function loadProfile() {
      try {
        const res = await api.get('/student/profile');
        setProfile(res.data);
        setEditForm({
          universityName: res.data.universityName ?? '',
          enrollmentStatus: res.data.enrollmentStatus ?? '',
          expectedGraduation: res.data.expectedGraduation?.split('T')[0] ?? '',
        });
      } catch {
        // Profile may not exist
      } finally {
        setLoading(false);
      }
    }
    loadProfile();
  }, []);

  const handleSave = async () => {
    setSaving(true);
    try {
      const payload: Record<string, string> = {
        universityName: editForm.universityName,
        enrollmentStatus: editForm.enrollmentStatus,
      };
      if (editForm.expectedGraduation) {
        payload.expectedGraduation = editForm.expectedGraduation;
      }
      await api.put('/student/profile', payload);
      const res = await api.get('/student/profile');
      setProfile(res.data);
      setEditing(false);
      toast.success('Profile updated successfully.');
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to update profile.');
    } finally {
      setSaving(false);
    }
  };

  const mentorshipStatusColors: Record<string, string> = {
    Unmatched: 'bg-white/10 text-white/50 border-white/15',
    Matched: 'bg-indigo/15 text-indigo border-indigo/20',
    Active: 'bg-verified/15 text-verified border-verified/20',
    Suspended: 'bg-red-500/15 text-red-400 border-red-500/20',
  };

  return (
    <StudentLayout>
      <Toaster position="top-right" toastOptions={{ style: { background: '#1F2937', color: '#E5E7EB', border: '1px solid rgba(255,255,255,0.08)' } }} />
      <div className="pt-24 pb-16 px-4 sm:px-6 lg:px-8 max-w-3xl mx-auto">
        {loading ? (
          <div className="space-y-6">
            <div className="h-8 w-48 shimmer-bg rounded-lg" />
            <div className="h-64 shimmer-bg rounded-xl" />
          </div>
        ) : !profile ? (
          <div className="glass-card rounded-xl p-12 text-center">
            <p className="text-white/40">Your profile is not yet available. Please wait for your application to be approved.</p>
          </div>
        ) : (
          <>
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.5, ease: [0.16, 1, 0.3, 1] }}
            >
              <h1 className="text-2xl font-display font-bold text-white mb-6">Your Profile</h1>

              {/* Profile Card */}
              <div className="glass-card rounded-xl p-6 mb-6">
                <div className="flex items-center gap-4 mb-6">
                  <div className="w-14 h-14 rounded-full bg-gradient-to-br from-indigo to-indigo-light flex items-center justify-center text-xl font-bold text-white">
                    {profile.firstName[0]}
                  </div>
                  <div>
                    <div className="text-lg font-semibold text-white">{profile.firstName} {profile.lastName}</div>
                    <div className="text-sm text-white/35">{profile.email}</div>
                  </div>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div className="p-3 rounded-lg bg-white/[0.02]">
                    <div className="text-xs text-white/30 mb-1">Student Type</div>
                    <div className="text-sm text-white/70">{profile.studentType}</div>
                  </div>
                  <div className="p-3 rounded-lg bg-white/[0.02]">
                    <div className="text-xs text-white/30 mb-1">Mentorship Status</div>
                    <span className={`inline-block px-2 py-0.5 text-xs font-medium rounded-full border ${mentorshipStatusColors[profile.mentorshipStatus] ?? mentorshipStatusColors.Unmatched}`}>
                      {profile.mentorshipStatus}
                    </span>
                  </div>
                  {profile.mentorName && (
                    <div className="p-3 rounded-lg bg-white/[0.02]">
                      <div className="text-xs text-white/30 mb-1">Mentor</div>
                      <div className="text-sm text-white/70">{profile.mentorName}</div>
                    </div>
                  )}
                  <div className="p-3 rounded-lg bg-white/[0.02]">
                    <div className="text-xs text-white/30 mb-1">Projects Completed</div>
                    <div className="text-sm text-white/70">{profile.totalProjectsCompleted}</div>
                  </div>
                  <div className="p-3 rounded-lg bg-white/[0.02]">
                    <div className="text-xs text-white/30 mb-1">Total Earnings</div>
                    <div className="text-sm text-white/70">KES {profile.totalEarnings.toLocaleString()}</div>
                  </div>
                  <div className="p-3 rounded-lg bg-white/[0.02]">
                    <div className="text-xs text-white/30 mb-1">Average Rating</div>
                    <div className="text-sm text-white/70">{profile.averageRating > 0 ? `${profile.averageRating.toFixed(1)} / 5.0` : 'No ratings yet'}</div>
                  </div>
                </div>
              </div>

              {/* Editable Section */}
              <div className="glass-card rounded-xl p-6">
                <div className="flex items-center justify-between mb-4">
                  <h2 className="text-sm font-semibold text-white/60 uppercase tracking-wider">Academic Information</h2>
                  {!editing && (
                    <button
                      onClick={() => setEditing(true)}
                      className="text-xs text-indigo hover:text-indigo-light transition-colors"
                    >
                      Edit
                    </button>
                  )}
                </div>

                {editing ? (
                  <div className="space-y-4">
                    <div>
                      <label className="block text-xs font-medium text-white/40 mb-1.5">University / Institution</label>
                      <input
                        type="text"
                        value={editForm.universityName}
                        onChange={(e) => setEditForm({ ...editForm, universityName: e.target.value })}
                        className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20"
                      />
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-white/40 mb-1.5">Enrollment Status</label>
                      <select
                        value={editForm.enrollmentStatus}
                        onChange={(e) => setEditForm({ ...editForm, enrollmentStatus: e.target.value })}
                        className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white bg-transparent appearance-none cursor-pointer"
                      >
                        <option value="Enrolled" className="bg-brand-light text-white">Enrolled</option>
                        <option value="Graduated" className="bg-brand-light text-white">Graduated</option>
                        <option value="Intern" className="bg-brand-light text-white">Intern</option>
                      </select>
                    </div>
                    {editForm.enrollmentStatus === 'Enrolled' && (
                      <div>
                        <label className="block text-xs font-medium text-white/40 mb-1.5">Expected Graduation</label>
                        <input
                          type="date"
                          value={editForm.expectedGraduation}
                          onChange={(e) => setEditForm({ ...editForm, expectedGraduation: e.target.value })}
                          className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white"
                        />
                      </div>
                    )}
                    <div className="flex gap-3 pt-2">
                      <button
                        onClick={handleSave}
                        disabled={saving}
                        className="px-5 py-2 bg-indigo text-white text-sm font-medium rounded-lg hover:bg-indigo-light transition-all disabled:opacity-50"
                      >
                        {saving ? 'Saving...' : 'Save Changes'}
                      </button>
                      <button
                        onClick={() => setEditing(false)}
                        className="px-5 py-2 text-sm text-white/40 hover:text-white transition-colors"
                      >
                        Cancel
                      </button>
                    </div>
                  </div>
                ) : (
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <div className="p-3 rounded-lg bg-white/[0.02]">
                      <div className="text-xs text-white/30 mb-1">University</div>
                      <div className="text-sm text-white/70">{profile.universityName}</div>
                    </div>
                    <div className="p-3 rounded-lg bg-white/[0.02]">
                      <div className="text-xs text-white/30 mb-1">Enrollment Status</div>
                      <div className="text-sm text-white/70">{profile.enrollmentStatus}</div>
                    </div>
                    {profile.expectedGraduation && (
                      <div className="p-3 rounded-lg bg-white/[0.02]">
                        <div className="text-xs text-white/30 mb-1">Expected Graduation</div>
                        <div className="text-sm text-white/70">{new Date(profile.expectedGraduation).toLocaleDateString()}</div>
                      </div>
                    )}
                    {profile.studentIdNumber && (
                      <div className="p-3 rounded-lg bg-white/[0.02]">
                        <div className="text-xs text-white/30 mb-1">Student ID</div>
                        <div className="text-sm text-white/70">{profile.studentIdNumber}</div>
                      </div>
                    )}
                  </div>
                )}
              </div>
            </motion.div>
          </>
        )}
      </div>
    </StudentLayout>
  );
}
