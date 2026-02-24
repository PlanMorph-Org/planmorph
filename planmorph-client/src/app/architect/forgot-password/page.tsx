import PasswordResetRequestPage from '@/src/components/auth/PasswordResetRequestPage';

export default function ArchitectForgotPasswordPage() {
  return (
    <PasswordResetRequestPage
      portal="architect"
      title="Architect Password Reset"
      subtitle="Submit your architect account email to receive secure reset instructions."
      loginHref="/architect/login"
      accentClass="text-golden/80 hover:text-golden"
      helperText="Professional account resets require additional verification and stricter controls."
    />
  );
}
