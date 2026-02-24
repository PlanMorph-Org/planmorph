import PasswordResetRequestPage from '@/src/components/auth/PasswordResetRequestPage';

export default function EngineerForgotPasswordPage() {
  return (
    <PasswordResetRequestPage
      portal="engineer"
      title="Engineer Password Reset"
      subtitle="Submit your engineer account email to receive secure reset instructions."
      loginHref="/engineer/login"
      accentClass="text-slate-teal/80 hover:text-slate-teal"
      helperText="Professional account resets require additional verification and stricter controls."
    />
  );
}
