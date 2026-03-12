import { useEffect } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate, useSearch } from "@tanstack/react-router";
import { LoginForm } from "./login-form";
import { useAuth } from "@/auth/auth-context";

export function LoginPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { accessToken, isLoading } = useAuth();

  // If the user already has a valid token, redirect away from login
  useEffect(() => {
    if (!isLoading && accessToken) {
      void navigate({ to: "/" });
    }
  }, [isLoading, accessToken, navigate]);

  // Read the returnUrl from the query string (TanStack Router)
  const search = useSearch({ strict: false }) as { returnUrl?: string };
  const returnUrl =
    search.returnUrl && search.returnUrl.startsWith("/")
      ? search.returnUrl
      : "/";

  function handleSuccess() {
    void navigate({ to: returnUrl });
  }

  if (isLoading) {
    // Blank screen while the silent-refresh check runs
    return null;
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-background px-4">
      <div className="w-full max-w-sm space-y-6 rounded-lg border bg-card p-8 shadow-sm">
        <div className="space-y-1 text-center">
          <h1 className="text-2xl font-bold tracking-tight">
            {t("auth.page.title")}
          </h1>
          <p className="text-sm text-muted-foreground">
            {t("auth.page.description")}
          </p>
        </div>

        <LoginForm onSuccess={handleSuccess} />
      </div>
    </div>
  );
}
