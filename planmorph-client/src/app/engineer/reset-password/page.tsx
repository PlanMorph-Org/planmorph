import PasswordResetConfirmPage from '@/src/components/auth/PasswordResetConfirmPage';

export default function EngineerResetPasswordPage() {
  return (
    <PasswordResetConfirmPage
      portal="engineer"
      title="Engineer Reset Password"
      subtitle="Enter your token, verification code, and a new secure password."
      loginHref="/engineer/login"
      accentClass="text-slate-teal/80 hover:text-slate-teal"
      requireVerificationCode
      passwordHint="Professional passwords must be at least 12 characters with uppercase, lowercase, number, and special character."
    />
  );
}
