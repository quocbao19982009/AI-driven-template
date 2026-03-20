import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "@/store/hooks";
import { setSearchQuery, setLocationFilter, setActiveTab } from "../store/rooms-slice";
import { useGetApiRooms } from "@/api/generated/rooms/rooms";
import { useGetApiLocations } from "@/api/generated/locations/locations";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Plus, MapPin } from "lucide-react";
import { RoomsTable } from "./rooms-table";
import { RoomsCalendar } from "./rooms-calendar";
import { RoomFormDialog } from "./room-form-dialog";
import { LocationsManager } from "./locations-manager";
import { useDebounce } from "@/hooks/use-debounce";
import { cn } from "@/lib/utils";

const PAGE_SIZE = 10;

export function RoomsPage() {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const { searchQuery, locationFilter, sortBy, sortDir, activeTab } = useAppSelector(
    (s) => s.rooms
  );
  const [page, setPage] = useState(1);
  const [createOpen, setCreateOpen] = useState(false);
  const [locationsOpen, setLocationsOpen] = useState(false);

  const debouncedSearch = useDebounce(searchQuery, 300);

  const { data: locationsResponse } = useGetApiLocations();
  const locations = locationsResponse?.data?.data ?? [];

  const { data: roomsResponse, isLoading } = useGetApiRooms({
    page,
    pageSize: PAGE_SIZE,
    search: debouncedSearch || undefined,
    locationId: locationFilter ?? undefined,
    sortBy,
    sortDir,
  });
  const pagedResult = roomsResponse?.data?.data;
  const rooms = pagedResult?.items ?? [];
  const totalPages = pagedResult?.totalPages ?? 1;

  function handleSearchChange(value: string) {
    dispatch(setSearchQuery(value));
    setPage(1);
  }

  function handleLocationFilterChange(value: string) {
    dispatch(setLocationFilter(value ? Number(value) : null));
    setPage(1);
  }

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">{t("rooms.title")}</h1>
          <p className="text-muted-foreground">{t("rooms.description")}</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => setLocationsOpen(true)}>
            <MapPin className="mr-2 h-4 w-4" />
            {t("rooms.manageLocations")}
          </Button>
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="mr-2 h-4 w-4" />
            {t("rooms.newRoom")}
          </Button>
        </div>
      </div>

      {/* Tab switcher */}
      <div className="border-b">
        <nav className="flex gap-4">
          {(["rooms", "calendar"] as const).map((tab) => (
            <button
              key={tab}
              onClick={() => dispatch(setActiveTab(tab))}
              className={cn(
                "border-b-2 px-1 pb-2 text-sm font-medium transition-colors",
                activeTab === tab
                  ? "border-primary text-foreground"
                  : "border-transparent text-muted-foreground hover:text-foreground"
              )}
            >
              {t(`rooms.tabs.${tab}`)}
            </button>
          ))}
        </nav>
      </div>

      {activeTab === "rooms" && (
        <>
          <div className="flex gap-2">
            <Input
              placeholder={t("rooms.searchPlaceholder")}
              value={searchQuery}
              onChange={(e) => handleSearchChange(e.target.value)}
              className="max-w-xs"
            />
            <select
              className="border-input bg-background h-10 rounded-md border px-3 text-sm"
              value={locationFilter ?? ""}
              onChange={(e) => handleLocationFilterChange(e.target.value)}
            >
              <option value="">{t("rooms.allLocations")}</option>
              {locations.map((loc) => (
                <option key={loc.id} value={loc.id}>
                  {loc.name}
                </option>
              ))}
            </select>
          </div>

          <RoomsTable
            rooms={rooms}
            isLoading={isLoading}
            page={page}
            totalPages={totalPages}
            onPageChange={setPage}
          />
        </>
      )}

      {activeTab === "calendar" && <RoomsCalendar />}

      <RoomFormDialog open={createOpen} onOpenChange={setCreateOpen} />
      <LocationsManager open={locationsOpen} onOpenChange={setLocationsOpen} />
    </div>
  );
}
