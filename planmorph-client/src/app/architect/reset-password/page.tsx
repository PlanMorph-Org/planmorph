import PasswordResetConfirmPage from '@/src/components/auth/PasswordResetConfirmPage';

export default function ArchitectResetPasswordPage() {
  return (
    <PasswordResetConfirmPage
      portal="architect"
      title="Architect Reset Password"
      subtitle="Enter your token, verification code, and a new secure password."
      loginHref="/architect/login"
      accentClass="text-golden/80 hover:text-golden"
      requireVerificationCode
      passwordHint="Professional passwords must be at least 12 characters with uppercase, lowercase, number, and special character."
    />
  );
}
