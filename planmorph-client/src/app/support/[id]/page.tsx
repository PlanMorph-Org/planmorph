'use client';

import { useEffect, useState } from 'react';
import { useRouter, useParams } from 'next/navigation';
import Layout from '@/src/components/Layout';
import api from '@/src/lib/api';
import { Ticket } from '@/src/types';
import toast, { Toaster } from 'react-hot-toast';
import { useRoleGuard } from '@/src/hooks/useRoleGuard';
import { motion } from 'framer-motion';

const getStatusStyle = (status: string) => {
  switch (status) {
    case 'Open': return 'bg-blue-500/15 text-blue-400 border-blue-500/20';
    case 'Assigned': return 'bg-golden/15 text-golden border-golden/20';
    case 'InProgress': return 'bg-brand-accent/15 text-brand-accent border-brand-accent/20';
    case 'Resolved': return 'bg-verified/15 text-verified border-verified/20';
    case 'Closed': return 'bg-white/10 text-white/50 border-white/10';
    default: return 'bg-white/10 text-white/50 border-white/10';
  }
};

const getPriorityStyle = (priority: string) => {
  switch (priority) {
    case 'Low': return 'bg-white/10 text-white/50';
    case 'Medium': return 'bg-golden/15 text-golden';
    case 'High': return 'bg-orange-500/15 text-orange-400';
    case 'Urgent': return 'bg-red-500/15 text-red-400';
    default: return 'bg-white/10 text-white/50';
  }
};

export default function TicketDetailPage() {
  const router = useRouter();
  const params = useParams();
  const ticketId = params.id as string;

  const { isAuthorized, isChecking } = useRoleGuard({
    requiredRole: 'Client',
    redirectTo: '/login',
  });

  const [ticket, setTicket] = useState<Ticket | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [newMessage, setNewMessage] = useState('');
  const [isSendingMessage, setIsSendingMessage] = useState(false);
  const [isClosing, setIsClosing] = useState(false);

  useEffect(() => {
    if (isAuthorized && ticketId) loadTicket();
  }, [isAuthorized, ticketId]);

  const loadTicket = async () => {
    setIsLoading(true);
    try {
      const response = await api.get<Ticket>(`/ticket/${ticketId}`);
      setTicket(response.data);
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to load ticket');
      router.push('/support');
    } finally {
      setIsLoading(false);
    }
  };

  const handleSendMessage = async (e: React.FormEvent) => {
    e.preventDefault();

    if (newMessage.trim().length < 1) {
      toast.error('Message cannot be empty');
      return;
    }

    setIsSendingMessage(true);
    try {
      await api.post(`/ticket/${ticketId}/messages`, { content: newMessage });
      setNewMessage('');
      await loadTicket();
      toast.success('Message sent');
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to send message');
    } finally {
      setIsSendingMessage(false);
    }
  };

  const handleCloseTicket = async () => {
    setIsClosing(true);
    try {
      await api.put(`/ticket/${ticketId}/close`);
      await loadTicket();
      toast.success('Ticket closed');
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to close ticket');
    } finally {
      setIsClosing(false);
    }
  };

  if (isChecking || isLoading) {
    return (
      <Layout>
        <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
          <div className="h-8 w-40 shimmer-bg rounded mb-6" />
          <div className="glass-card rounded-xl p-6 space-y-4">
            <div className="h-4 w-1/3 shimmer-bg rounded" />
            <div className="h-20 shimmer-bg rounded" />
          </div>
        </div>
      </Layout>
    );
  }

  if (!ticket) {
    return (
      <Layout>
        <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
          <p className="text-white/50">Ticket not found</p>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <Toaster
        position="top-right"
        toastOptions={{
          style: {
            background: '#1F2937',
            color: '#E5E7EB',
            border: '1px solid rgba(255,255,255,0.08)',
          },
        }}
      />

      {/* Header */}
      <section className="relative py-14 border-b border-white/6">
        <div className="absolute inset-0 bg-hero-gradient" />
        <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <button
            onClick={() => router.push('/support')}
            className="text-xs text-white/40 hover:text-white transition-colors mb-3"
          >
            &larr; Back to Tickets
          </button>
          <motion.h1
            initial={{ opacity: 0, y: 16 }}
            animate={{ opacity: 1, y: 0 }}
            className="text-2xl md:text-3xl font-display font-bold text-white tracking-tight mb-1"
          >
            {ticket.subject}
          </motion.h1>
          <div className="flex items-center gap-3 flex-wrap mt-3">
            {ticket.ticketNumber && (
              <span className="text-xs font-mono font-semibold text-brand-accent">
                {ticket.ticketNumber}
              </span>
            )}
            <span className={`px-2.5 py-0.5 text-[10px] font-semibold rounded-full border ${getStatusStyle(ticket.status)}`}>
              {ticket.status}
            </span>
            <span className={`px-2 py-0.5 text-[10px] font-medium rounded-full ${getPriorityStyle(ticket.priority)}`}>
              {ticket.priority}
            </span>
            <span className="text-[10px] text-white/30">
              {ticket.category}
            </span>
          </div>
        </div>
      </section>

      <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Main Content */}
          <div className="lg:col-span-2 space-y-6">
            {/* Original Description */}
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              className="glass-card rounded-xl p-6"
            >
              <h3 className="text-sm font-semibold text-white/70 mb-3">
                Original Request
              </h3>
              <p className="text-sm text-white/60 leading-relaxed whitespace-pre-wrap">
                {ticket.description}
              </p>
              <p className="text-xs text-white/25 mt-4">
                Created {new Date(ticket.createdAt).toLocaleString()}
              </p>
            </motion.div>

            {/* Messages */}
            <div className="space-y-3">
              <h3 className="text-sm font-semibold text-white/70">
                Conversation ({ticket.messages.length})
              </h3>

              {ticket.messages.length === 0 ? (
                <div className="glass-card-light rounded-xl p-8 text-center">
                  <p className="text-sm text-white/40">
                    No messages yet. Add a message to continue the conversation.
                  </p>
                </div>
              ) : (
                <div className="space-y-3">
                  {ticket.messages.map((msg) => (
                    <motion.div
                      key={msg.id}
                      initial={{ opacity: 0, y: 10 }}
                      animate={{ opacity: 1, y: 0 }}
                      className={`glass-card-light rounded-xl p-4 ${
                        msg.isFromAdmin ? 'border-l-4 border-brand-accent' : ''
                      }`}
                    >
                      <div className="flex items-start justify-between mb-2">
                        <div className="flex items-center gap-2">
                          <span className="text-sm font-medium text-white/80">
                            {msg.authorName}
                          </span>
                          {msg.isFromAdmin && (
                            <span className="px-2 py-0.5 text-[10px] font-medium rounded-full bg-brand-accent/15 text-brand-accent border border-brand-accent/20">
                              Support Team
                            </span>
                          )}
                        </div>
                        <span className="text-xs text-white/25">
                          {new Date(msg.createdAt).toLocaleString()}
                        </span>
                      </div>
                      <p className="text-sm text-white/60 leading-relaxed whitespace-pre-wrap">
                        {msg.content}
                      </p>
                    </motion.div>
                  ))}
                </div>
              )}
            </div>

            {/* Add Message Form */}
            {ticket.status !== 'Closed' && (
              <motion.form
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                onSubmit={handleSendMessage}
                className="glass-card rounded-xl p-6"
              >
                <label className="block text-sm font-medium text-white/70 mb-2">
                  Add Message
                </label>
                <textarea
                  value={newMessage}
                  onChange={(e) => setNewMessage(e.target.value)}
                  placeholder="Type your message here..."
                  rows={4}
                  className="w-full px-4 py-3 glass-input rounded-lg text-sm text-white placeholder:text-white/20 resize-none mb-3"
                  disabled={isSendingMessage}
                />
                <button
                  type="submit"
                  disabled={isSendingMessage || newMessage.trim().length < 1}
                  className="w-full py-3 bg-brand-accent text-white font-medium rounded-lg hover:bg-blue-500 transition-all btn-glow disabled:opacity-50"
                >
                  {isSendingMessage ? 'Sending...' : 'Send Message'}
                </button>
              </motion.form>
            )}

            {/* Closed State Message */}
            {ticket.status === 'Closed' && (
              <div className="glass-card-light rounded-xl p-6 text-center border border-white/6">
                <p className="text-sm text-white/40 mb-1">This ticket has been closed.</p>
                <p className="text-xs text-white/25">
                  If you need further help, you can create a new ticket.
                </p>
              </div>
            )}
          </div>

          {/* Sidebar */}
          <div className="space-y-6">
            {/* Ticket Info */}
            <motion.div
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ delay: 0.1 }}
              className="glass-card rounded-xl p-5 space-y-4"
            >
              <h3 className="text-sm font-semibold text-white/70">
                Ticket Information
              </h3>

              <div>
                <p className="text-xs text-white/30 mb-1">Status</p>
                <span className={`inline-block px-2.5 py-1 text-xs font-semibold rounded-full border ${getStatusStyle(ticket.status)}`}>
                  {ticket.status}
                </span>
              </div>

              <div>
                <p className="text-xs text-white/30 mb-1">Priority</p>
                <span className={`inline-block px-2.5 py-1 text-xs font-medium rounded-full ${getPriorityStyle(ticket.priority)}`}>
                  {ticket.priority}
                </span>
              </div>

              <div>
                <p className="text-xs text-white/30 mb-1">Category</p>
                <p className="text-sm text-white/60">{ticket.category}</p>
              </div>

              <div className="pt-4 border-t border-white/6">
                <p className="text-xs text-white/30 mb-1">Created</p>
                <p className="text-xs text-white/50">
                  {new Date(ticket.createdAt).toLocaleString()}
                </p>
              </div>

              <div>
                <p className="text-xs text-white/30 mb-1">Last Updated</p>
                <p className="text-xs text-white/50">
                  {new Date(ticket.updatedAt).toLocaleString()}
                </p>
              </div>

              {ticket.closedAt && (
                <div>
                  <p className="text-xs text-white/30 mb-1">Closed</p>
                  <p className="text-xs text-white/50">
                    {new Date(ticket.closedAt).toLocaleString()}
                  </p>
                </div>
              )}
            </motion.div>

            {/* Close Ticket Action */}
            {ticket.status !== 'Closed' && (
              <motion.div
                initial={{ opacity: 0, x: 20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: 0.2 }}
                className="glass-card rounded-xl p-5"
              >
                <h3 className="text-sm font-semibold text-white/70 mb-3">
                  Actions
                </h3>
                <button
                  onClick={handleCloseTicket}
                  disabled={isClosing}
                  className="w-full py-2.5 border border-white/10 rounded-lg text-sm text-white/50 hover:text-white hover:bg-white/5 transition-all disabled:opacity-50"
                >
                  {isClosing ? 'Closing...' : 'Close Ticket'}
                </button>
                <p className="text-[10px] text-white/20 mt-2">
                  Close this ticket if your issue has been resolved.
                </p>
              </motion.div>
            )}
          </div>
        </div>
      </div>
    </Layout>
  );
}
