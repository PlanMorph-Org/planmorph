import PasswordResetRequestPage from '@/src/components/auth/PasswordResetRequestPage';

export default function ForgotPasswordPage() {
  return (
    <PasswordResetRequestPage
      portal="client"
      title="Forgot Password"
      subtitle="Enter your account email and we’ll send a reset link."
      loginHref="/login"
      accentClass="text-brand-accent/80 hover:text-brand-accent"
    />
  );
}
