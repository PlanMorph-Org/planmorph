import { jwtDecode } from 'jwt-decode';

export type GoogleIdTokenProfile = {
  email?: string;
  given_name?: string;
  family_name?: string;
};

const GOOGLE_AUTH_ID_TOKEN_STORAGE_KEY = 'google-auth-id-token';

export function decodeGoogleIdTokenProfile(token: string): GoogleIdTokenProfile | undefined {
  try {
    return jwtDecode<GoogleIdTokenProfile>(token);
  } catch {
    return undefined;
  }
}

export function consumePendingGoogleIdToken(): string | null {
  if (typeof window === 'undefined') {
    return null;
  }

  const token = sessionStorage.getItem(GOOGLE_AUTH_ID_TOKEN_STORAGE_KEY);
  if (token) {
    sessionStorage.removeItem(GOOGLE_AUTH_ID_TOKEN_STORAGE_KEY);
  }

  return token;
}
