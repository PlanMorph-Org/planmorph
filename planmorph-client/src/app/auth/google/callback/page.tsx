'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';

/**
 * OAuth 2.0 callback page for Google Sign-In.
 *
 * Google redirects here with the id_token in the URL fragment
 * (e.g. #id_token=eyJ...&state=/login).
 *
 * Two modes:
 *  1. Popup – window.opener exists → postMessage the token to the parent and close.
 *  2. Full redirect (fallback when popup was blocked) → store the token in
 *     sessionStorage and redirect back to the page that initiated sign-in.
 */
export default function GoogleCallbackPage() {
  const router = useRouter();
  const [status, setStatus] = useState<'processing' | 'error'>('processing');
  const [errorMessage, setErrorMessage] = useState('');

  useEffect(() => {
    const hash = window.location.hash.substring(1);
    const params = new URLSearchParams(hash);
    const idToken = params.get('id_token');
    const error = params.get('error');
    const returnPath = params.get('state') || sessionStorage.getItem('google-auth-return') || '/login';

    // Clean up persisted return path
    sessionStorage.removeItem('google-auth-return');

    if (error) {
      const message =
        error === 'access_denied'
          ? 'Google sign-in was cancelled.'
          : `Google sign-in error: ${error}`;

      if (window.opener) {
        window.opener.postMessage({ type: 'google-auth-error', message }, window.location.origin);
        window.close();
        return;
      }
      setStatus('error');
      setErrorMessage(message);
      return;
    }

    if (!idToken) {
      const message = 'No token received from Google. Please try again.';
      if (window.opener) {
        window.opener.postMessage({ type: 'google-auth-error', message }, window.location.origin);
        window.close();
        return;
      }
      setStatus('error');
      setErrorMessage(message);
      return;
    }

    // --- Success ---
    if (window.opener) {
      // Popup mode: send token to parent and close
      window.opener.postMessage({ type: 'google-auth-success', idToken }, window.location.origin);
      window.close();
      return;
    }

    // Full redirect fallback: persist token and redirect back
    sessionStorage.setItem('google-auth-id-token', idToken);
    router.replace(returnPath);
  }, [router]);

  if (status === 'error') {
    return (
      <div className="min-h-screen flex items-center justify-center bg-brand">
        <div className="glass-card rounded-2xl p-8 max-w-md text-center">
          <div className="w-12 h-12 rounded-full bg-red-500/20 flex items-center justify-center mx-auto mb-4">
            <svg className="w-6 h-6 text-red-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </div>
          <h2 className="text-lg font-semibold text-white mb-2">Sign-in failed</h2>
          <p className="text-sm text-white/50 mb-6">{errorMessage}</p>
          <button
            onClick={() => window.close()}
            className="px-6 py-2 bg-brand-accent text-white font-medium text-sm rounded-lg hover:bg-blue-500 transition-colors"
          >
            Close
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-brand">
      <div className="text-center">
        <div className="animate-spin w-8 h-8 border-2 border-white/20 border-t-white rounded-full mx-auto mb-4" />
        <p className="text-white/60 text-sm">Completing Google sign-in...</p>
      </div>
    </div>
  );
}
