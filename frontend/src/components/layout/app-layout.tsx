import { Link, Outlet } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";
import { LanguageSwitcher } from "@/components/ui/language-switcher";
import { useAuth } from "@/auth/auth-context";
import { usePostApiAuthLogout } from "@/api/generated/auth/auth";
import { Button } from "@/components/ui/button";

export function AppLayout() {
  const { t } = useTranslation();
  const { accessToken, logout } = useAuth();

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
            <Button
              variant="ghost"
              size="sm"
              onClick={() => logoutMutation.mutate()}
              disabled={logoutMutation.isPending}
            >
              {t("nav.logout")}
            </Button>
          )}
          <LanguageSwitcher />
        </div>
      </nav>
      <hr />
      <main>
        <Outlet />
      </main>
    </>
  );
}
