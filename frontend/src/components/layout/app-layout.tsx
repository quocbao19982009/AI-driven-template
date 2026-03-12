import React from "react";
import { Link, Outlet } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";
import { LanguageSwitcher } from "@/components/ui/language-switcher";
import { useAuth } from "@/auth/auth-context";
import { useAppSelector } from "@/store/hooks";
import { usePostApiAuthLogout } from "@/api/generated/auth/auth";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Spinner } from "@/components/ui/spinner";
import { ErrorBoundary } from "@/components/error-boundary";

export function AppLayout() {
  const { t } = useTranslation();
  const { accessToken, logout } = useAuth();
  const user = useAppSelector((s) => s.auth.user);

  const logoutMutation = usePostApiAuthLogout({
    mutation: {
      onSettled: () => {
        logout();
      },
    },
  });

  return (
    <>
      <nav className="flex items-center gap-2 p-2">
        <Link to="/" className="[&.active]:font-bold">
          {t("nav.home")}
        </Link>{" "}
        <Link to="/about" className="[&.active]:font-bold">
          {t("nav.about")}
        </Link>{" "}
        <Link to="/features" className="[&.active]:font-bold">
          {t("nav.features")}
        </Link>
        <div className="ml-auto flex items-center gap-2">
          {!accessToken && (
            <Link to="/login" className="[&.active]:font-bold">
              {t("nav.login")}
            </Link>
          )}
          {accessToken && (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" size="sm" className="max-w-40 truncate">
                  {user?.email ?? t("nav.account")}
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuItem
                  onClick={() => logoutMutation.mutate()}
                  disabled={logoutMutation.isPending}
                >
                  {t("nav.logout")}
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          )}
          <LanguageSwitcher />
        </div>
      </nav>
      <hr />
      <main>
        <ErrorBoundary>
          <React.Suspense fallback={<Spinner />}>
            <Outlet />
          </React.Suspense>
        </ErrorBoundary>
      </main>
    </>
  );
}
