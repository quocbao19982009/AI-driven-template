import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";

const LANGUAGES = [
  { code: "en", label: "EN" },
  { code: "fi", label: "FI" },
] as const;

export function LanguageSwitcher() {
  const { i18n } = useTranslation();

  function handleChange(code: string) {
    i18n.changeLanguage(code);
    localStorage.setItem("language", code);
  }

  return (
    <div className="flex gap-1">
      {LANGUAGES.map((lang) => (
        <Button
          key={lang.code}
          variant={i18n.language === lang.code ? "default" : "ghost"}
          size="sm"
          onClick={() => handleChange(lang.code)}
        >
          {lang.label}
        </Button>
      ))}
    </div>
  );
}
