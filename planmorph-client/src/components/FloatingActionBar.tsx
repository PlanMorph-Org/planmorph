'use client';

import React, { useState, useEffect, useCallback, useRef } from 'react';
import Link from 'next/link';
import { usePathname, useRouter } from 'next/navigation';
import { useAuthStore } from '../store/authStore';
import CurrencySelector from './CurrencySelector';
import { motion, AnimatePresence } from 'framer-motion';

/* ────── Types ────── */
interface NavItem {
    href: string;
    label: string;
    icon?: React.ReactNode;
}

interface FloatingActionBarProps {
    navItems: NavItem[];
    accentColor?: 'blue' | 'golden' | 'teal' | 'indigo';
    portalLabel?: string;
    logoHref?: string;
    showCurrency?: boolean;
    authMode?: 'client' | 'architect' | 'engineer' | 'student';
    /** For architect/engineer portals — pre-loaded user info from role guard */
    portalUser?: { firstName?: string; lastName?: string } | null;
}

/* ────── Accent configs ────── */
const accents = {
    blue: {
        activeBg: 'bg-brand-accent/15',
        activeText: 'text-brand-accent',
        indicator: 'bg-brand-accent',
        glow: 'shadow-[0_0_24px_rgba(59,130,246,0.12)]',
        pill: 'text-brand-accent bg-brand-accent/10 border-brand-accent/20',
        avatarGradient: 'from-brand-accent to-blue-400',
        avatarText: 'text-white',
        btnBg: 'bg-brand-accent hover:bg-blue-500',
    },
    golden: {
        activeBg: 'bg-golden/15',
        activeText: 'text-golden',
        indicator: 'bg-golden',
        glow: 'shadow-[0_0_24px_rgba(212,168,67,0.12)]',
        pill: 'text-golden bg-golden/10 border-golden/20',
        avatarGradient: 'from-golden to-golden-light',
        avatarText: 'text-brand',
        btnBg: 'bg-golden hover:bg-golden-light',
    },
    teal: {
        activeBg: 'bg-slate-teal/15',
        activeText: 'text-slate-teal',
        indicator: 'bg-slate-teal',
        glow: 'shadow-[0_0_24px_rgba(13,148,136,0.12)]',
        pill: 'text-slate-teal bg-slate-teal/10 border-slate-teal/20',
        avatarGradient: 'from-slate-teal to-emerald-400',
        avatarText: 'text-white',
        btnBg: 'bg-slate-teal hover:bg-teal-500',
    },
    indigo: {
        activeBg: 'bg-indigo/15',
        activeText: 'text-indigo',
        indicator: 'bg-indigo',
        glow: 'shadow-[0_0_24px_rgba(99,102,241,0.12)]',
        pill: 'text-indigo bg-indigo/10 border-indigo/20',
        avatarGradient: 'from-indigo to-indigo-light',
        avatarText: 'text-white',
        btnBg: 'bg-indigo hover:bg-indigo-light',
    },
};

const HIDE_DELAY = 1500;

export default function FloatingActionBar({
    navItems,
    accentColor = 'blue',
    portalLabel,
    logoHref = '/',
    showCurrency = true,
    authMode = 'client',
    portalUser,
}: FloatingActionBarProps) {
    const pathname = usePathname();
    const router = useRouter();
    const { user, isAuthenticated, logout } = useAuthStore();
    const accent = accents[accentColor];

    const [visible, setVisible] = useState(true);
    const [expanded, setExpanded] = useState(false);
    const [portalsOpen, setPortalsOpen] = useState(false);
    const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
    const barRef = useRef<HTMLDivElement>(null);
    const portalsRef = useRef<HTMLDivElement>(null);

    /* ── Auto-hide logic ── */
    const show = useCallback(() => {
        setVisible(true);
        if (timerRef.current) clearTimeout(timerRef.current);
        timerRef.current = setTimeout(() => setVisible(false), HIDE_DELAY);
    }, []);

    useEffect(() => {
        show(); // show on mount
        const events = ['mousemove', 'scroll', 'touchstart', 'keydown'] as const;
        events.forEach((e) => window.addEventListener(e, show, { passive: true }));
        return () => {
            events.forEach((e) => window.removeEventListener(e, show));
            if (timerRef.current) clearTimeout(timerRef.current);
        };
    }, [show]);

    // Keep visible while hovering the bar itself
    const handleBarEnter = () => {
        if (timerRef.current) clearTimeout(timerRef.current);
        setVisible(true);
    };
    const handleBarLeave = () => {
        timerRef.current = setTimeout(() => setVisible(false), HIDE_DELAY);
    };

    // Close expanded menu on route change
    useEffect(() => {
        setExpanded(false);
        setPortalsOpen(false);
    }, [pathname]);

    // Close portals dropdown on outside click
    useEffect(() => {
        const handleClickOutside = (e: MouseEvent) => {
            if (portalsRef.current && !portalsRef.current.contains(e.target as Node)) {
                setPortalsOpen(false);
            }
        };
        if (portalsOpen) {
            document.addEventListener('mousedown', handleClickOutside);
            return () => document.removeEventListener('mousedown', handleClickOutside);
        }
    }, [portalsOpen]);

    const isActive = (href: string) => pathname === href;

    /* ── Auth helpers ── */
    const displayUser = authMode === 'client' ? user : portalUser;
    const displayName = displayUser?.firstName ?? '';
    const displayInitial = displayName?.[0] ?? '?';

    const handleLogout = () => {
        if (authMode === 'client') {
            logout();
            router.push('/');
        } else {
            localStorage.removeItem('token');
            localStorage.removeItem('user');
            const loginRoutes: Record<string, string> = {
                architect: '/architect/login',
                engineer: '/engineer/login',
                student: '/student/login',
            };
            router.push(loginRoutes[authMode] ?? '/');
        }
        setExpanded(false);
    };

    const isLoggedIn = authMode === 'client' ? isAuthenticated : !!portalUser;

    return (
        <>
            {/* ── DESKTOP BAR (top, centered) ── */}
            <motion.div
                ref={barRef}
                onMouseEnter={handleBarEnter}
                onMouseLeave={handleBarLeave}
                initial={{ y: -80, opacity: 0 }}
                animate={{ y: visible ? 0 : -80, opacity: visible ? 1 : 0 }}
                transition={{ duration: 0.4, ease: [0.16, 1, 0.3, 1] as const }}
                className="hidden sm:flex fixed top-4 left-1/2 -translate-x-1/2 z-50 items-center gap-1 px-3 py-2 glass-dock rounded-2xl"
            >
                {/* Logo */}
                <Link href={logoHref} className="flex items-center gap-2 px-2 group shrink-0">
                    <div className="relative">
                        <div className={`absolute inset-0 ${accentColor === 'golden' ? 'bg-golden/20' : accentColor === 'teal' ? 'bg-slate-teal/20' : accentColor === 'indigo' ? 'bg-indigo/20' : 'bg-brand-accent/20'} rounded-full blur-lg opacity-0 group-hover:opacity-100 transition-opacity duration-500`} />
                        <img src="/planmorph.svg" alt="PlanMorph" className="h-7 w-auto brightness-0 invert rounded-full relative z-10" />
                    </div>
                    <span className="text-base font-display font-bold text-white tracking-tight">PlanMorph</span>
                    {portalLabel && (
                        <span className={`ml-0.5 px-2 py-0.5 text-[9px] font-semibold uppercase tracking-widest rounded-full border ${accent.pill}`}>
                            {portalLabel}
                        </span>
                    )}
                </Link>

                {/* Separator */}
                <div className="w-px h-5 bg-white/10 mx-1" />

                {/* Nav links */}
                {navItems.map((item) => (
                    <Link
                        key={item.href}
                        href={item.href}
                        className={`relative px-3 py-1.5 text-sm font-medium rounded-lg transition-all duration-300 whitespace-nowrap ${isActive(item.href) ? `text-white ${accent.activeBg}` : 'text-white/55 hover:text-white hover:bg-white/5'
                            }`}
                    >
                        {item.label}
                        {isActive(item.href) && (
                            <motion.div
                                layoutId="dock-indicator"
                                className={`absolute bottom-0 left-2 right-2 h-0.5 ${accent.indicator} rounded-full`}
                                transition={{ type: 'spring', stiffness: 500, damping: 35 }}
                            />
                        )}
                    </Link>
                ))}

                {/* Portals dropdown — client mode only */}
                {authMode === 'client' && (
                    <div ref={portalsRef} className="relative">
                        <button
                            onClick={() => setPortalsOpen(!portalsOpen)}
                            className={`relative flex items-center gap-1 px-3 py-1.5 text-sm font-medium rounded-lg transition-all duration-300 whitespace-nowrap ${portalsOpen ? 'text-white bg-white/10' : 'text-white/55 hover:text-white hover:bg-white/5'}`}
                        >
                            Portals
                            <svg className={`w-3 h-3 transition-transform duration-200 ${portalsOpen ? 'rotate-180' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24" strokeWidth={2}>
                                <path strokeLinecap="round" strokeLinejoin="round" d="M19.5 8.25l-7.5 7.5-7.5-7.5" />
                            </svg>
                        </button>
                        <AnimatePresence>
                            {portalsOpen && (
                                <motion.div
                                    initial={{ opacity: 0, y: -8, scale: 0.96 }}
                                    animate={{ opacity: 1, y: 0, scale: 1 }}
                                    exit={{ opacity: 0, y: -8, scale: 0.96 }}
                                    transition={{ duration: 0.15, ease: [0.16, 1, 0.3, 1] }}
                                    className="absolute top-full left-1/2 -translate-x-1/2 mt-2 w-52 glass-modal rounded-xl p-1.5 shadow-lg"
                                >
                                    <Link href="/architect/login" onClick={() => setPortalsOpen(false)}
                                        className="flex items-center gap-2.5 px-3 py-2.5 rounded-lg text-white/55 hover:text-white hover:bg-white/5 transition-all duration-200">
                                        <div className="w-6 h-6 rounded-md bg-golden/10 flex items-center justify-center flex-shrink-0">
                                            <svg className="w-3.5 h-3.5 text-golden" fill="none" stroke="currentColor" viewBox="0 0 24 24" strokeWidth={1.5}>
                                                <path strokeLinecap="round" strokeLinejoin="round" d="M2.25 21h19.5m-18-18v18m10.5-18v18m6-13.5V21M6.75 6.75h.75m-.75 3h.75m-.75 3h.75m3-6h.75m-.75 3h.75m-.75 3h.75M6.75 21v-3.375c0-.621.504-1.125 1.125-1.125h2.25c.621 0 1.125.504 1.125 1.125V21M3 3h12m-.75 4.5H21m-3.75 3h.008v.008h-.008v-.008zm0 3h.008v.008h-.008v-.008zm0 3h.008v.008h-.008v-.008z" />
                                            </svg>
                                        </div>
                                        <div>
                                            <div className="text-sm font-medium">Architect</div>
                                            <div className="text-[10px] text-white/25">Upload & sell designs</div>
                                        </div>
                                    </Link>
                                    <Link href="/engineer/login" onClick={() => setPortalsOpen(false)}
                                        className="flex items-center gap-2.5 px-3 py-2.5 rounded-lg text-white/55 hover:text-white hover:bg-white/5 transition-all duration-200">
                                        <div className="w-6 h-6 rounded-md bg-slate-teal/10 flex items-center justify-center flex-shrink-0">
                                            <svg className="w-3.5 h-3.5 text-slate-teal" fill="none" stroke="currentColor" viewBox="0 0 24 24" strokeWidth={1.5}>
                                                <path strokeLinecap="round" strokeLinejoin="round" d="M11.42 15.17l-5.384 3.318A1.5 1.5 0 014.5 17.16V6.84a1.5 1.5 0 011.536-1.328l5.384 3.318a1.5 1.5 0 010 2.556zM16.5 12a4.5 4.5 0 11-9 0 4.5 4.5 0 019 0zm-4.5-2.25a2.25 2.25 0 100 4.5 2.25 2.25 0 000-4.5z" />
                                            </svg>
                                        </div>
                                        <div>
                                            <div className="text-sm font-medium">Engineer</div>
                                            <div className="text-[10px] text-white/25">Verify structures</div>
                                        </div>
                                    </Link>
                                    <div className="mx-2 my-1 border-t border-white/6" />
                                    <Link href="/student/register" onClick={() => setPortalsOpen(false)}
                                        className="flex items-center gap-2.5 px-3 py-2.5 rounded-lg text-white/55 hover:text-white hover:bg-white/5 transition-all duration-200">
                                        <div className="w-6 h-6 rounded-md bg-indigo/10 flex items-center justify-center flex-shrink-0">
                                            <svg className="w-3.5 h-3.5 text-indigo" fill="none" stroke="currentColor" viewBox="0 0 24 24" strokeWidth={1.5}>
                                                <path strokeLinecap="round" strokeLinejoin="round" d="M4.26 10.147a60.438 60.438 0 00-.491 6.347A48.62 48.62 0 0112 20.904a48.62 48.62 0 018.232-4.41 60.46 60.46 0 00-.491-6.347m-15.482 0a50.636 50.636 0 00-2.658-.813A59.906 59.906 0 0112 3.493a59.903 59.903 0 0110.399 5.84c-.896.248-1.783.52-2.658.814m-15.482 0A50.717 50.717 0 0112 13.489a50.702 50.702 0 017.74-3.342" />
                                            </svg>
                                        </div>
                                        <div>
                                            <div className="text-sm font-medium">Student Program</div>
                                            <div className="text-[10px] text-white/25">Learn while you earn</div>
                                        </div>
                                    </Link>
                                </motion.div>
                            )}
                        </AnimatePresence>
                    </div>
                )}

                {/* Separator */}
                <div className="w-px h-5 bg-white/10 mx-1" />

                {/* Right section */}
                {showCurrency && <CurrencySelector />}

                {isLoggedIn ? (
                    <div className="flex items-center gap-2">
                        <div className="flex items-center gap-1.5 px-2.5 py-1 rounded-lg bg-white/5">
                            <div className={`w-5 h-5 rounded-full bg-gradient-to-br ${accent.avatarGradient} flex items-center justify-center text-[9px] font-bold ${accent.avatarText}`}>
                                {displayInitial}
                            </div>
                            <span className="text-xs text-white/60 hidden lg:inline">{displayName}</span>
                        </div>
                        {authMode === 'client' && (
                            <>
                                <Link
                                    href="/my-orders"
                                    className={`text-xs font-medium rounded-lg px-2.5 py-1.5 transition-all duration-300 ${isActive('/my-orders') ? 'text-white bg-white/10' : 'text-white/50 hover:text-white hover:bg-white/5'
                                        }`}
                                >
                                    Orders
                                </Link>
                                {/* Admin Management Button */}
                                {user?.role === 'Admin' && (
                                    <Link
                                        href="/admin/login"
                                        className={`text-xs font-medium rounded-lg px-2.5 py-1.5 transition-all duration-300 ${isActive('/admin') ? 'text-white bg-red-500/20 border border-red-500/30' : 'text-red-400 hover:text-red-300 hover:bg-red-500/10'
                                            }`}
                                        title="Administrative Dashboard - Management Access Only"
                                    >
                                        Management
                                    </Link>
                                )}
                            </>
                        )}
                        <button
                            onClick={handleLogout}
                            className="text-xs font-medium text-white/40 hover:text-white transition-colors duration-300 px-2 py-1.5"
                        >
                            Logout
                        </button>
                    </div>
                ) : authMode === 'client' ? (
                    <div className="flex items-center gap-1.5">
                        <Link href="/login" className="text-xs font-medium text-white/55 hover:text-white px-2.5 py-1.5 rounded-lg transition-colors duration-300">
                            Sign In
                        </Link>
                        <Link href="/register" className={`text-xs font-medium px-3.5 py-1.5 rounded-lg text-white ${accent.btnBg} transition-all duration-300 btn-glow`}>
                            Get Started
                        </Link>
                    </div>
                ) : null}
            </motion.div>

            {/* ── MOBILE BAR (bottom, centered) ── */}
            <motion.div
                onTouchStart={handleBarEnter}
                initial={{ y: 80, opacity: 0 }}
                animate={{ y: visible ? 0 : 80, opacity: visible ? 1 : 0 }}
                transition={{ duration: 0.4, ease: [0.16, 1, 0.3, 1] as const }}
                className="flex sm:hidden fixed bottom-4 left-3 right-3 z-50 flex-col glass-dock rounded-2xl"
            >
                {/* Expanded menu */}
                <AnimatePresence>
                    {expanded && (
                        <motion.div
                            initial={{ height: 0, opacity: 0 }}
                            animate={{ height: 'auto', opacity: 1 }}
                            exit={{ height: 0, opacity: 0 }}
                            transition={{ duration: 0.25, ease: [0.16, 1, 0.3, 1] as const }}
                            className="overflow-hidden border-b border-white/8"
                        >
                            <div className="px-4 py-3 space-y-1">
                                {navItems.map((item) => (
                                    <Link
                                        key={item.href}
                                        href={item.href}
                                        onClick={() => setExpanded(false)}
                                        className={`block px-3 py-2 text-sm font-medium rounded-lg transition-all duration-200 ${isActive(item.href) ? `text-white ${accent.activeBg}` : 'text-white/55 hover:text-white hover:bg-white/5'
                                            }`}
                                    >
                                        {item.label}
                                    </Link>
                                ))}

                                {/* Portals section — client mode only */}
                                {authMode === 'client' && (
                                    <div className="pt-2 border-t border-white/6 mt-2 space-y-1">
                                        <div className="px-3 py-1 text-[10px] font-semibold uppercase tracking-widest text-white/20">Portals</div>
                                        <Link href="/architect/login" onClick={() => setExpanded(false)}
                                            className="flex items-center gap-2.5 px-3 py-2 rounded-lg text-white/55 hover:text-white hover:bg-white/5 transition-all duration-200">
                                            <div className="w-5 h-5 rounded-md bg-golden/10 flex items-center justify-center flex-shrink-0">
                                                <svg className="w-3 h-3 text-golden" fill="none" stroke="currentColor" viewBox="0 0 24 24" strokeWidth={1.5}>
                                                    <path strokeLinecap="round" strokeLinejoin="round" d="M2.25 21h19.5m-18-18v18m10.5-18v18m6-13.5V21M6.75 6.75h.75m-.75 3h.75m-.75 3h.75m3-6h.75m-.75 3h.75m-.75 3h.75M6.75 21v-3.375c0-.621.504-1.125 1.125-1.125h2.25c.621 0 1.125.504 1.125 1.125V21M3 3h12m-.75 4.5H21m-3.75 3h.008v.008h-.008v-.008zm0 3h.008v.008h-.008v-.008zm0 3h.008v.008h-.008v-.008z" />
                                                </svg>
                                            </div>
                                            <span className="text-sm font-medium">Architect Portal</span>
                                        </Link>
                                        <Link href="/engineer/login" onClick={() => setExpanded(false)}
                                            className="flex items-center gap-2.5 px-3 py-2 rounded-lg text-white/55 hover:text-white hover:bg-white/5 transition-all duration-200">
                                            <div className="w-5 h-5 rounded-md bg-slate-teal/10 flex items-center justify-center flex-shrink-0">
                                                <svg className="w-3 h-3 text-slate-teal" fill="none" stroke="currentColor" viewBox="0 0 24 24" strokeWidth={1.5}>
                                                    <path strokeLinecap="round" strokeLinejoin="round" d="M11.42 15.17l-5.384 3.318A1.5 1.5 0 014.5 17.16V6.84a1.5 1.5 0 011.536-1.328l5.384 3.318a1.5 1.5 0 010 2.556zM16.5 12a4.5 4.5 0 11-9 0 4.5 4.5 0 019 0zm-4.5-2.25a2.25 2.25 0 100 4.5 2.25 2.25 0 000-4.5z" />
                                                </svg>
                                            </div>
                                            <span className="text-sm font-medium">Engineer Portal</span>
                                        </Link>
                                        <Link href="/student/register" onClick={() => setExpanded(false)}
                                            className="flex items-center gap-2.5 px-3 py-2 rounded-lg text-white/55 hover:text-white hover:bg-white/5 transition-all duration-200">
                                            <div className="w-5 h-5 rounded-md bg-indigo/10 flex items-center justify-center flex-shrink-0">
                                                <svg className="w-3 h-3 text-indigo" fill="none" stroke="currentColor" viewBox="0 0 24 24" strokeWidth={1.5}>
                                                    <path strokeLinecap="round" strokeLinejoin="round" d="M4.26 10.147a60.438 60.438 0 00-.491 6.347A48.62 48.62 0 0112 20.904a48.62 48.62 0 018.232-4.41 60.46 60.46 0 00-.491-6.347m-15.482 0a50.636 50.636 0 00-2.658-.813A59.906 59.906 0 0112 3.493a59.903 59.903 0 0110.399 5.84c-.896.248-1.783.52-2.658.814m-15.482 0A50.717 50.717 0 0112 13.489a50.702 50.702 0 017.74-3.342" />
                                                </svg>
                                            </div>
                                            <span className="text-sm font-medium">Student Program</span>
                                        </Link>
                                    </div>
                                )}

                                <div className="pt-2 border-t border-white/6 mt-2 space-y-1">
                                    {showCurrency && (
                                        <div className="px-3 py-1.5">
                                            <CurrencySelector />
                                        </div>
                                    )}
                                    {isLoggedIn ? (
                                        <>
                                            <div className="flex items-center gap-2 px-3 py-1.5">
                                                <div className={`w-5 h-5 rounded-full bg-gradient-to-br ${accent.avatarGradient} flex items-center justify-center text-[9px] font-bold ${accent.avatarText}`}>
                                                    {displayInitial}
                                                </div>
                                                <span className="text-xs text-white/50">{displayName}</span>
                                            </div>
                                            {authMode === 'client' && (
                                                <>
                                                    <Link
                                                        href="/my-orders"
                                                        onClick={() => setExpanded(false)}
                                                        className="block px-3 py-2 text-sm text-white/55 hover:text-white rounded-lg hover:bg-white/5 transition-all"
                                                    >
                                                        My Orders
                                                    </Link>
                                                    {/* Admin Management Button for Mobile */}
                                                    {user?.role === 'Admin' && (
                                                        <Link
                                                            href="/admin"
                                                            onClick={() => setExpanded(false)}
                                                            className="block px-3 py-2 text-sm text-red-400 hover:text-red-300 rounded-lg hover:bg-red-500/10 transition-all"
                                                            title="Administrative Dashboard - Management Access Only"
                                                        >
                                                            🔒 Management
                                                        </Link>
                                                    )}
                                                </>
                                            )}
                                            <button
                                                onClick={handleLogout}
                                                className="block w-full text-left px-3 py-2 text-sm text-white/40 hover:text-white rounded-lg hover:bg-white/5 transition-all"
                                            >
                                                Logout
                                            </button>
                                        </>
                                    ) : authMode === 'client' ? (
                                        <div className="flex gap-2 px-3 py-1.5">
                                            <Link
                                                href="/login"
                                                onClick={() => setExpanded(false)}
                                                className="flex-1 text-center text-xs font-medium text-white/60 px-3 py-2 rounded-lg border border-white/10 hover:bg-white/5 transition-all"
                                            >
                                                Sign In
                                            </Link>
                                            <Link
                                                href="/register"
                                                onClick={() => setExpanded(false)}
                                                className={`flex-1 text-center text-xs font-medium text-white px-3 py-2 rounded-lg ${accent.btnBg} transition-all`}
                                            >
                                                Get Started
                                            </Link>
                                        </div>
                                    ) : null}
                                </div>
                            </div>
                        </motion.div>
                    )}
                </AnimatePresence>

                {/* Bottom bar row */}
                <div className="flex items-center justify-between px-3 py-2.5">
                    {/* Logo */}
                    <Link href={logoHref} className="flex items-center gap-1.5 shrink-0">
                        <img src="/planmorph.svg" alt="PlanMorph" className="h-6 w-auto brightness-0 invert rounded-full" />
                        <span className="text-sm font-display font-bold text-white">PM</span>
                        {portalLabel && (
                            <span className={`px-1.5 py-0.5 text-[8px] font-semibold uppercase tracking-widest rounded-full border ${accent.pill}`}>
                                {portalLabel}
                            </span>
                        )}
                    </Link>

                    {/* Quick nav icons — show 2-3 key links inline */}
                    <div className="flex items-center gap-1">
                        {navItems.slice(0, 3).map((item) => (
                            <Link
                                key={item.href}
                                href={item.href}
                                className={`px-2.5 py-1.5 text-[11px] font-medium rounded-lg transition-all duration-200 ${isActive(item.href) ? `text-white ${accent.activeBg}` : 'text-white/45 hover:text-white'
                                    }`}
                            >
                                {item.label.length > 10 ? item.label.split(' ')[0] : item.label}
                            </Link>
                        ))}
                    </div>

                    {/* Expand/collapse toggle */}
                    <button
                        onClick={() => setExpanded(!expanded)}
                        className="p-2 rounded-lg text-white/50 hover:text-white hover:bg-white/5 transition-all"
                        aria-label="Toggle menu"
                    >
                        <AnimatePresence mode="wait">
                            {expanded ? (
                                <motion.svg key="close" initial={{ rotate: -90, opacity: 0 }} animate={{ rotate: 0, opacity: 1 }} exit={{ rotate: 90, opacity: 0 }} transition={{ duration: 0.15 }} className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                                </motion.svg>
                            ) : (
                                <motion.svg key="menu" initial={{ rotate: 90, opacity: 0 }} animate={{ rotate: 0, opacity: 1 }} exit={{ rotate: -90, opacity: 0 }} transition={{ duration: 0.15 }} className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
                                </motion.svg>
                            )}
                        </AnimatePresence>
                    </button>
                </div>
            </motion.div>
        </>
    );
}
