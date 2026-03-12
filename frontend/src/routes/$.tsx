import { createFileRoute, Link } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";

export const Route = createFileRoute("/$")({
  component: NotFoundPage,
});

function NotFoundPage() {
  const { t } = useTranslation();

  return (
    <div className="flex min-h-[60vh] flex-col items-center justify-center gap-4 text-center">
      <h1 className="text-4xl font-bold">404</h1>
      <p className="text-xl text-muted-foreground">{t("errors.notFound")}</p>
      <p className="text-muted-foreground">{t("errors.notFoundDescription")}</p>
      <Link
        to="/"
        className="rounded bg-primary px-4 py-2 text-primary-foreground hover:bg-primary/90"
      >
        {t("errors.goHome")}
      </Link>
    </div>
  );
}
