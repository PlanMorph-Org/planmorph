'use client';

import { useEffect, useState, useRef } from 'react';
import { useParams } from 'next/navigation';
import Link from 'next/link';
import StudentLayout from '@/src/components/StudentLayout';
import api from '@/src/lib/api';
import toast, { Toaster } from 'react-hot-toast';
import { motion, AnimatePresence } from 'framer-motion';
import type { MentorshipProject, ProjectIteration, ProjectMessage } from '@/src/types';

type Tab = 'details' | 'iterations' | 'messages';

export default function StudentProjectDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [project, setProject] = useState<MentorshipProject | null>(null);
  const [iterations, setIterations] = useState<ProjectIteration[]>([]);
  const [messages, setMessages] = useState<ProjectMessage[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<Tab>('details');

  // Submit iteration
  const [submitNotes, setSubmitNotes] = useState('');
  const [submitting, setSubmitting] = useState(false);

  // Messages
  const [newMessage, setNewMessage] = useState('');
  const [sendingMessage, setSendingMessage] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    loadProject();
  }, [id]);

  useEffect(() => {
    if (activeTab === 'iterations') loadIterations();
    if (activeTab === 'messages') loadMessages();
  }, [activeTab, id]);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  async function loadProject() {
    try {
      const res = await api.get(`/student/projects/${id}`);
      setProject(res.data);
    } catch {
      toast.error('Failed to load project.');
    } finally {
      setLoading(false);
    }
  }

  async function loadIterations() {
    try {
      const res = await api.get(`/student/projects/${id}/iterations`);
      setIterations(res.data);
    } catch {
      // silent
    }
  }

  async function loadMessages() {
    try {
      const res = await api.get(`/student/projects/${id}/messages`);
      setMessages(res.data);
    } catch {
      // silent
    }
  }

  async function handleSubmitIteration(e: React.FormEvent) {
    e.preventDefault();
    if (!submitNotes.trim()) {
      toast.error('Please add notes describing your submission.');
      return;
    }
    setSubmitting(true);
    try {
      await api.post(`/student/projects/${id}/submit`, { notes: submitNotes });
      toast.success('Iteration submitted for mentor review.');
      setSubmitNotes('');
      await loadProject();
      await loadIterations();
      setActiveTab('iterations');
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to submit iteration.');
    } finally {
      setSubmitting(false);
    }
  }

  async function handleSendMessage(e: React.FormEvent) {
    e.preventDefault();
    if (!newMessage.trim()) return;
    setSendingMessage(true);
    try {
      await api.post(`/student/projects/${id}/messages`, { content: newMessage });
      setNewMessage('');
      await loadMessages();
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to send message.');
    } finally {
      setSendingMessage(false);
    }
  }

  const statusColors: Record<string, string> = {
    InProgress: 'bg-indigo/15 text-indigo border-indigo/20',
    UnderMentorReview: 'bg-amber-500/15 text-amber-400 border-amber-500/20',
    RevisionRequested: 'bg-orange-500/15 text-orange-400 border-orange-500/20',
    MentorApproved: 'bg-verified/15 text-verified border-verified/20',
    ClientReview: 'bg-brand-accent/15 text-brand-accent border-brand-accent/20',
    Completed: 'bg-verified/15 text-verified border-verified/20',
    Paid: 'bg-verified/15 text-verified border-verified/20',
    StudentAssigned: 'bg-brand-accent/15 text-brand-accent border-brand-accent/20',
  };

  const iterationStatusColors: Record<string, string> = {
    Submitted: 'bg-brand-accent/15 text-brand-accent border-brand-accent/20',
    UnderReview: 'bg-amber-500/15 text-amber-400 border-amber-500/20',
    Approved: 'bg-verified/15 text-verified border-verified/20',
    RevisionRequested: 'bg-orange-500/15 text-orange-400 border-orange-500/20',
    Superseded: 'bg-white/10 text-white/30 border-white/15',
  };

  const canSubmitIteration = project && ['InProgress', 'RevisionRequested', 'StudentAssigned'].includes(project.status);

  const tabs: { id: Tab; label: string }[] = [
    { id: 'details', label: 'Details' },
    { id: 'iterations', label: 'Iterations' },
    { id: 'messages', label: 'Messages' },
  ];

  return (
    <StudentLayout>
      <Toaster position="top-right" toastOptions={{ style: { background: '#1F2937', color: '#E5E7EB', border: '1px solid rgba(255,255,255,0.08)' } }} />
      <div className="pt-24 pb-16 px-4 sm:px-6 lg:px-8 max-w-4xl mx-auto">
        {loading ? (
          <div className="space-y-6">
            <div className="h-8 w-64 shimmer-bg rounded-lg" />
            <div className="h-48 shimmer-bg rounded-xl" />
          </div>
        ) : !project ? (
          <div className="glass-card rounded-xl p-12 text-center">
            <p className="text-white/40">Project not found.</p>
            <Link href="/student/projects" className="text-indigo text-sm mt-2 inline-block hover:text-indigo-light transition-colors">
              Back to projects
            </Link>
          </div>
        ) : (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5, ease: [0.16, 1, 0.3, 1] }}
          >
            {/* Header */}
            <div className="mb-6">
              <Link href="/student/projects" className="text-xs text-white/30 hover:text-white/50 transition-colors mb-3 inline-block">
                &larr; Back to projects
              </Link>
              <div className="flex items-start justify-between gap-4">
                <div>
                  <h1 className="text-xl font-display font-bold text-white mb-1">{project.title}</h1>
                  <div className="flex items-center gap-2 text-xs text-white/30">
                    <span>{project.projectNumber}</span>
                    <span className="w-1 h-1 rounded-full bg-white/15" />
                    <span>{project.category}</span>
                  </div>
                </div>
                <span className={`inline-block px-2.5 py-1 text-xs font-medium rounded-full border ${statusColors[project.status] ?? 'bg-white/10 text-white/50 border-white/15'}`}>
                  {project.status.replace(/([A-Z])/g, ' $1').trim()}
                </span>
              </div>
            </div>

            {/* Tabs */}
            <div className="flex gap-1 mb-6 p-1 glass-card rounded-lg w-fit">
              {tabs.map(tab => (
                <button
                  key={tab.id}
                  onClick={() => setActiveTab(tab.id)}
                  className={`px-4 py-2 text-sm font-medium rounded-md transition-all duration-200 ${activeTab === tab.id ? 'bg-indigo/15 text-indigo' : 'text-white/40 hover:text-white/60'}`}
                >
                  {tab.label}
                </button>
              ))}
            </div>

            <AnimatePresence mode="wait">
              {/* Details Tab */}
              {activeTab === 'details' && (
                <motion.div
                  key="details"
                  initial={{ opacity: 0, y: 10 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0, y: -10 }}
                  transition={{ duration: 0.3 }}
                  className="space-y-4"
                >
                  <div className="glass-card rounded-xl p-6">
                    <h3 className="text-sm font-semibold text-white/50 uppercase tracking-wider mb-3">Description</h3>
                    <p className="text-sm text-white/60 whitespace-pre-wrap leading-relaxed">{project.description}</p>
                  </div>

                  {project.requirements && (
                    <div className="glass-card rounded-xl p-6">
                      <h3 className="text-sm font-semibold text-white/50 uppercase tracking-wider mb-3">Requirements</h3>
                      <p className="text-sm text-white/60 whitespace-pre-wrap leading-relaxed">{project.requirements}</p>
                    </div>
                  )}

                  {project.scope && (
                    <div className="glass-card rounded-xl p-6">
                      <h3 className="text-sm font-semibold text-white/50 uppercase tracking-wider mb-3">Scope</h3>
                      <p className="text-sm text-white/60 whitespace-pre-wrap leading-relaxed">{project.scope}</p>
                    </div>
                  )}

                  <div className="glass-card rounded-xl p-6">
                    <h3 className="text-sm font-semibold text-white/50 uppercase tracking-wider mb-3">Project Info</h3>
                    <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
                      <div className="p-3 rounded-lg bg-white/[0.02]">
                        <div className="text-xs text-white/30 mb-1">Your Fee</div>
                        <div className="text-sm font-medium text-white/70">KES {project.studentFee?.toLocaleString() ?? '—'}</div>
                      </div>
                      <div className="p-3 rounded-lg bg-white/[0.02]">
                        <div className="text-xs text-white/30 mb-1">Priority</div>
                        <div className="text-sm text-white/70">{project.priority}</div>
                      </div>
                      <div className="p-3 rounded-lg bg-white/[0.02]">
                        <div className="text-xs text-white/30 mb-1">Delivery Days</div>
                        <div className="text-sm text-white/70">{project.estimatedDeliveryDays}</div>
                      </div>
                      <div className="p-3 rounded-lg bg-white/[0.02]">
                        <div className="text-xs text-white/30 mb-1">Max Revisions</div>
                        <div className="text-sm text-white/70">{project.maxRevisions}</div>
                      </div>
                      <div className="p-3 rounded-lg bg-white/[0.02]">
                        <div className="text-xs text-white/30 mb-1">Current Revision</div>
                        <div className="text-sm text-white/70">{project.currentRevisionCount}</div>
                      </div>
                      {project.studentDeadline && (
                        <div className="p-3 rounded-lg bg-white/[0.02]">
                          <div className="text-xs text-white/30 mb-1">Your Deadline</div>
                          <div className="text-sm text-white/70">{new Date(project.studentDeadline).toLocaleDateString()}</div>
                        </div>
                      )}
                    </div>
                  </div>

                  {/* Submit Iteration */}
                  {canSubmitIteration && (
                    <div className="glass-card rounded-xl p-6">
                      <h3 className="text-sm font-semibold text-indigo uppercase tracking-wider mb-3">Submit Work for Review</h3>
                      <form onSubmit={handleSubmitIteration} className="space-y-4">
                        <div>
                          <label className="block text-xs font-medium text-white/40 mb-1.5">Notes about this submission</label>
                          <textarea
                            value={submitNotes}
                            onChange={(e) => setSubmitNotes(e.target.value)}
                            rows={4}
                            placeholder="Describe what you've completed, any decisions made, or questions for your mentor..."
                            className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20 resize-none"
                          />
                        </div>
                        <button
                          type="submit"
                          disabled={submitting}
                          className="px-6 py-2.5 bg-indigo text-white text-sm font-medium rounded-lg hover:bg-indigo-light transition-all disabled:opacity-50"
                        >
                          {submitting ? 'Submitting...' : 'Submit for Mentor Review'}
                        </button>
                      </form>
                    </div>
                  )}
                </motion.div>
              )}

              {/* Iterations Tab */}
              {activeTab === 'iterations' && (
                <motion.div
                  key="iterations"
                  initial={{ opacity: 0, y: 10 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0, y: -10 }}
                  transition={{ duration: 0.3 }}
                >
                  {iterations.length === 0 ? (
                    <div className="glass-card rounded-xl p-12 text-center">
                      <p className="text-white/40 text-sm">No iterations submitted yet.</p>
                      <p className="text-white/25 text-xs mt-1">Submit your first iteration from the Details tab.</p>
                    </div>
                  ) : (
                    <div className="space-y-3">
                      {iterations.map((iter) => (
                        <div key={iter.id} className="glass-card rounded-xl p-5">
                          <div className="flex items-start justify-between gap-4 mb-3">
                            <div>
                              <div className="flex items-center gap-2">
                                <span className="text-sm font-medium text-white">Iteration #{iter.iterationNumber}</span>
                                <span className={`inline-block px-2 py-0.5 text-[10px] font-medium rounded-full border ${iterationStatusColors[iter.status] ?? 'bg-white/10 text-white/30 border-white/15'}`}>
                                  {iter.status.replace(/([A-Z])/g, ' $1').trim()}
                                </span>
                              </div>
                              <div className="text-xs text-white/25 mt-0.5">
                                Submitted {new Date(iter.createdAt).toLocaleString()}
                              </div>
                            </div>
                          </div>

                          {iter.notes && (
                            <div className="mb-3">
                              <div className="text-xs text-white/30 mb-1">Your Notes</div>
                              <p className="text-sm text-white/50 whitespace-pre-wrap">{iter.notes}</p>
                            </div>
                          )}

                          {iter.reviewNotes && (
                            <div className="p-3 rounded-lg bg-white/[0.02] border-l-2 border-indigo/30">
                              <div className="text-xs text-indigo/60 mb-1">Mentor Feedback</div>
                              <p className="text-sm text-white/50 whitespace-pre-wrap">{iter.reviewNotes}</p>
                              {iter.reviewedAt && (
                                <div className="text-xs text-white/20 mt-1">
                                  Reviewed {new Date(iter.reviewedAt).toLocaleString()}
                                </div>
                              )}
                            </div>
                          )}
                        </div>
                      ))}
                    </div>
                  )}
                </motion.div>
              )}

              {/* Messages Tab */}
              {activeTab === 'messages' && (
                <motion.div
                  key="messages"
                  initial={{ opacity: 0, y: 10 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0, y: -10 }}
                  transition={{ duration: 0.3 }}
                >
                  <div className="glass-card rounded-xl overflow-hidden">
                    {/* Messages Area */}
                    <div className="h-[400px] overflow-y-auto p-5 space-y-3">
                      {messages.length === 0 ? (
                        <div className="h-full flex items-center justify-center">
                          <p className="text-white/30 text-sm">No messages yet. Start a conversation with your mentor.</p>
                        </div>
                      ) : (
                        messages.map((msg) => {
                          const isOwn = msg.senderRole === 'Student';
                          return (
                            <div key={msg.id} className={`flex ${isOwn ? 'justify-end' : 'justify-start'}`}>
                              <div className={`max-w-[75%] p-3 rounded-xl ${isOwn
                                ? 'bg-indigo/15 rounded-br-sm'
                                : msg.isSystemMessage
                                  ? 'bg-white/5 border border-white/6 rounded-bl-sm'
                                  : 'bg-white/[0.06] rounded-bl-sm'
                                }`}
                              >
                                {!isOwn && (
                                  <div className="text-xs font-medium text-indigo/60 mb-1">
                                    {msg.senderName} {msg.isSystemMessage && '(System)'}
                                  </div>
                                )}
                                <p className="text-sm text-white/60 whitespace-pre-wrap">{msg.content}</p>
                                <div className="text-[10px] text-white/20 mt-1 text-right">
                                  {new Date(msg.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                                </div>
                              </div>
                            </div>
                          );
                        })
                      )}
                      <div ref={messagesEndRef} />
                    </div>

                    {/* Message Input */}
                    <form onSubmit={handleSendMessage} className="border-t border-white/6 p-3 flex gap-2">
                      <input
                        type="text"
                        value={newMessage}
                        onChange={(e) => setNewMessage(e.target.value)}
                        placeholder="Type a message to your mentor..."
                        className="flex-1 px-4 py-2.5 glass-input rounded-lg text-sm text-white placeholder:text-white/20"
                      />
                      <button
                        type="submit"
                        disabled={sendingMessage || !newMessage.trim()}
                        className="px-4 py-2.5 bg-indigo text-white text-sm font-medium rounded-lg hover:bg-indigo-light transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                      >
                        {sendingMessage ? (
                          <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24">
                            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                          </svg>
                        ) : 'Send'}
                      </button>
                    </form>
                  </div>
                </motion.div>
              )}
            </AnimatePresence>
          </motion.div>
        )}
      </div>
    </StudentLayout>
  );
}
