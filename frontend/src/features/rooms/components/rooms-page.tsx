import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "@/store/hooks";
import { setActiveTab } from "../store";
import { RoomsTab } from "./rooms-tab";
import { CalendarTab } from "./calendar-tab";

export function RoomsPage() {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const activeTab = useAppSelector((s) => s.rooms.activeTab);

  return (
    <div className="p-6 space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">
          {t("rooms.title")}
        </h1>
        <p className="text-muted-foreground">{t("rooms.description")}</p>
      </div>

      <div className="flex gap-1 border-b">
        {(["rooms", "calendar"] as const).map((tab) => (
          <button
            key={tab}
            onClick={() => dispatch(setActiveTab(tab))}
            className={[
              "px-4 py-2 text-sm font-medium -mb-px border-b-2 transition-colors",
              activeTab === tab
                ? "border-primary text-foreground"
                : "border-transparent text-muted-foreground hover:text-foreground",
            ].join(" ")}
          >
            {t(`rooms.tabs.${tab}`)}
          </button>
        ))}
      </div>

      {activeTab === "rooms" ? <RoomsTab /> : <CalendarTab />}
    </div>
  );
}
