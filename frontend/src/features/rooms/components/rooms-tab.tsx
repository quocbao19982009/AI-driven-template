import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Plus, Settings2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { RoomTable } from "./room-table";
import { RoomFormDialog } from "./room-form-dialog";
import { LocationsPanel } from "./locations-panel";
import { useGetApiRooms } from "@/api/generated/rooms/rooms";
import { useGetApiLocations } from "@/api/generated/locations/locations";
import { useAppDispatch, useAppSelector } from "@/store/hooks";
import { setSearchQuery, setLocationIdFilter } from "../store";
import { useDebounce } from "@/hooks/use-debounce";

const PAGE_SIZE = 10;

export function RoomsTab() {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const { searchQuery, locationIdFilter, sortBy, sortDir } = useAppSelector((s) => s.rooms);
  const debouncedSearch = useDebounce(searchQuery, 300);

  const [page, setPage] = useState(1);
  const [createOpen, setCreateOpen] = useState(false);
  const [locationsOpen, setLocationsOpen] = useState(false);

  const { data: locationsRes } = useGetApiLocations();
  const locations = locationsRes?.data?.data ?? [];

  const { data: roomsRes, isLoading } = useGetApiRooms({
    page,
    pageSize: PAGE_SIZE,
    search: debouncedSearch || undefined,
    locationId: locationIdFilter ?? undefined,
    sortBy,
    sortDir,
  });
  const pagedResult = roomsRes?.data?.data;
  const rooms = pagedResult?.items ?? [];
  const totalPages = pagedResult?.totalPages ?? 1;

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center gap-2">
        <Input
          placeholder={t("rooms.search.placeholder")}
          value={searchQuery}
          onChange={(e) => {
            dispatch(setSearchQuery(e.target.value));
            setPage(1);
          }}
          className="w-56"
        />

        <Select
          value={locationIdFilter ? String(locationIdFilter) : "all"}
          onValueChange={(v) => {
            dispatch(setLocationIdFilter(v === "all" ? null : Number(v)));
            setPage(1);
          }}
        >
          <SelectTrigger className="w-44">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">{t("rooms.search.allLocations")}</SelectItem>
            {locations.map((loc) => (
              <SelectItem key={loc.id} value={String(loc.id)}>
                {loc.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <div className="ml-auto flex gap-2">
          <Button variant="outline" onClick={() => setLocationsOpen(true)}>
            <Settings2 className="mr-2 h-4 w-4" />
            {t("rooms.manageLocations")}
          </Button>
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="mr-2 h-4 w-4" />
            {t("rooms.newRoom")}
          </Button>
        </div>
      </div>

      <RoomTable
        rooms={rooms}
        isLoading={isLoading}
        page={page}
        totalPages={totalPages}
        onPageChange={setPage}
      />

      <RoomFormDialog open={createOpen} onOpenChange={setCreateOpen} />
      <LocationsPanel open={locationsOpen} onOpenChange={setLocationsOpen} />
    </div>
  );
}
