import { Link, Outlet } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";
import { LanguageSwitcher } from "@/components/ui/language-switcher";

export function AppLayout() {
  const { t } = useTranslation();

  return (
    <>
      <nav className="p-2 flex items-center gap-2">
        <Link to="/" className="[&.active]:font-bold">
          {t("nav.home")}
        </Link>{" "}
        <Link to="/about" className="[&.active]:font-bold">
          {t("nav.about")}
        </Link>{" "}
        <Link to="/features" className="[&.active]:font-bold">
          {t("nav.features")}
        </Link>{" "}
        <Link to="/todos" className="[&.active]:font-bold">
          {t("nav.todos")}
        </Link>{" "}
        <Link to="/factories" className="[&.active]:font-bold">
          {t("nav.factories")}
        </Link>{" "}
        <Link to="/personnel" className="[&.active]:font-bold">
          {t("nav.personnel")}
        </Link>{" "}
        <Link to="/reservations" className="[&.active]:font-bold">
          {t("nav.reservations")}
        </Link>{" "}
        <Link to="/scheduling" className="[&.active]:font-bold">
          {t("nav.scheduling")}
        </Link>
        <div className="ml-auto">
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
