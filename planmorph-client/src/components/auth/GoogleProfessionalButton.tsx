'use client';

import Script from 'next/script';
import { useCallback, useEffect, useRef, useState } from 'react';
import api from '@/src/lib/api';
import { decodeGoogleIdTokenProfile, type GoogleIdTokenProfile } from '@/src/lib/googleAuth';

type GoogleCredentialResponse = {
  credential?: string;
};

type GoogleIdConfiguration = {
  auto_select?: boolean;
  callback: (response: GoogleCredentialResponse) => void;
  cancel_on_tap_outside?: boolean;
  client_id: string;
  context?: 'signin' | 'signup' | 'use';
  use_fedcm_for_prompt?: boolean;
};

type GoogleButtonConfiguration = {
  logo_alignment?: 'left' | 'center';
  shape?: 'circle' | 'pill' | 'rectangular' | 'square';
  size?: 'large' | 'medium' | 'small';
  text?: 'continue_with' | 'signin' | 'signin_with' | 'signup_with';
  theme?: 'filled_black' | 'filled_blue' | 'outline';
  type?: 'icon' | 'standard';
  width?: number;
};

type GoogleAccountsIdApi = {
  initialize: (configuration: GoogleIdConfiguration) => void;
  renderButton: (parent: HTMLElement, options: GoogleButtonConfiguration) => void;
};

declare global {
  interface Window {
    google?: {
      accounts?: {
        id?: GoogleAccountsIdApi;
      };
    };
  }
}

type GoogleProfessionalButtonProps = {
  onSuccess: (idToken: string, profile?: GoogleIdTokenProfile) => void;
  onError?: (message: string) => void;
  disabled?: boolean;
};

export default function GoogleProfessionalButton({
  onSuccess,
  onError,
  disabled = false,
}: GoogleProfessionalButtonProps) {
  const [clientId, setClientId] = useState(process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID ?? '');
  const [isScriptReady, setIsScriptReady] = useState(
    () => typeof window !== 'undefined' && !!window.google?.accounts?.id
  );
  const [buttonWidth, setButtonWidth] = useState(0);
  const buttonContainerRef = useRef<HTMLDivElement | null>(null);
  const buttonWrapperRef = useRef<HTMLDivElement | null>(null);
  const onSuccessRef = useRef(onSuccess);
  const onErrorRef = useRef(onError);

  useEffect(() => {
    onSuccessRef.current = onSuccess;
    onErrorRef.current = onError;
  }, [onSuccess, onError]);

  // Fetch client ID from API if not set via env var
  useEffect(() => {
    if (clientId) return;
    const loadClientId = async () => {
      try {
        const response = await api.get('/auth/google-client-id');
        const value = response.data?.clientId;
        if (typeof value === 'string' && value.trim().length > 0) {
          setClientId(value.trim());
        } else {
          onError?.('Google client ID is not configured.');
        }
      } catch {
        onError?.('Google client ID is not configured.');
      }
    };
    void loadClientId();
  }, [clientId, onError]);

  useEffect(() => {
    const wrapper = buttonWrapperRef.current;
    if (!wrapper) {
      return;
    }

    const syncWidth = () => {
      setButtonWidth(Math.max(Math.floor(wrapper.getBoundingClientRect().width), 240));
    };

    syncWidth();

    if (typeof ResizeObserver === 'undefined') {
      window.addEventListener('resize', syncWidth);
      return () => window.removeEventListener('resize', syncWidth);
    }

    const resizeObserver = new ResizeObserver(syncWidth);
    resizeObserver.observe(wrapper);
    return () => resizeObserver.disconnect();
  }, []);

  const handleCredentialResponse = useCallback((response: GoogleCredentialResponse) => {
    const idToken = response.credential;
    if (!idToken) {
      onErrorRef.current?.('Google authentication failed.');
      return;
    }

    const profile = decodeGoogleIdTokenProfile(idToken);
    onSuccessRef.current(idToken, profile);
  }, []);

  useEffect(() => {
    const googleIdentity = window.google?.accounts?.id;
    const buttonContainer = buttonContainerRef.current;

    if (!isScriptReady || !clientId || !buttonContainer || !googleIdentity || buttonWidth === 0) {
      return;
    }

    try {
      googleIdentity.initialize({
        client_id: clientId,
        callback: handleCredentialResponse,
        auto_select: false,
        cancel_on_tap_outside: true,
        context: 'signin',
        use_fedcm_for_prompt: true,
      });

      buttonContainer.innerHTML = '';
      googleIdentity.renderButton(buttonContainer, {
        type: 'standard',
        theme: 'outline',
        size: 'large',
        text: 'continue_with',
        shape: 'rectangular',
        logo_alignment: 'left',
        width: buttonWidth,
      });
    } catch {
      onErrorRef.current?.('Google authentication is unavailable right now.');
    }
  }, [buttonWidth, clientId, handleCredentialResponse, isScriptReady]);

  const handleScriptError = useCallback(() => {
    onErrorRef.current?.('Google authentication is unavailable right now.');
  }, []);

  return (
    <>
      <Script
        src="https://accounts.google.com/gsi/client"
        strategy="afterInteractive"
        onLoad={() => setIsScriptReady(true)}
        onError={handleScriptError}
      />
      <div className="relative w-full">
        <div
          ref={buttonWrapperRef}
          className={`min-h-12 overflow-hidden rounded-lg ${disabled ? 'opacity-50' : ''}`}
        >
          <div
            ref={buttonContainerRef}
            className="flex min-h-12 items-center justify-center"
          >
            {(!clientId || !isScriptReady) && (
              <span className="text-sm text-white/40">
                Loading Google sign-in...
              </span>
            )}
          </div>
        </div>
        {disabled && (
          <div
            aria-hidden="true"
            className="absolute inset-0 cursor-not-allowed"
          />
        )}
      </div>
    </>
  );
}
