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
        <Link to="/rooms" className="[&.active]:font-bold">
          {t("nav.rooms")}
        </Link>{" "}
        <Link to="/bookings" className="[&.active]:font-bold">
          {t("nav.bookings")}
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
