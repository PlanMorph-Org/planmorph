import { Suspense } from 'react';
import PasswordResetConfirmPage from '@/src/components/auth/PasswordResetConfirmPage';

export default function ResetPasswordPage() {
  return (
    <Suspense fallback={<div className="min-h-screen bg-brand" />}>
      <PasswordResetConfirmPage
        portal="client"
        title="Reset Password"
        subtitle="Use the reset token from your email to set a new password."
        loginHref="/login"
        accentClass="text-brand-accent/80 hover:text-brand-accent"
      />
    </Suspense>
  );
}
