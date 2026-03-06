import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { BookingTable } from "./booking-table";
import { BookingFormDialog } from "./booking-form-dialog";
import { useGetApiBookings } from "@/api/generated/bookings/bookings";
import { useGetApiRoomsAll } from "@/api/generated/rooms/rooms";
import { useAppDispatch, useAppSelector } from "@/store/hooks";
import { setRoomIdFilter, setFromDate, setToDate } from "../store";
import type { RoomDto } from "@/api/generated/models";

const PAGE_SIZE = 10;

export function BookingsPage() {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const { roomIdFilter, fromDate, toDate } = useAppSelector((s) => s.bookings);
  const [page, setPage] = useState(1);
  const [createOpen, setCreateOpen] = useState(false);

  const { data: roomsRes } = useGetApiRoomsAll();
  const rooms: RoomDto[] = roomsRes?.data?.data ?? [];

  const { data: bookingsRes, isLoading } = useGetApiBookings({
    page,
    pageSize: PAGE_SIZE,
    roomId: roomIdFilter ?? undefined,
    fromDate: fromDate ? new Date(fromDate).toISOString() : undefined,
    toDate: toDate ? new Date(toDate).toISOString() : undefined,
  });
  const pagedResult = bookingsRes?.data?.data;
  const bookings = pagedResult?.items ?? [];
  const totalPages = pagedResult?.totalPages ?? 1;

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">{t("bookings.title")}</h1>
          <p className="text-muted-foreground">{t("bookings.description")}</p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          {t("bookings.newBooking")}
        </Button>
      </div>

      <div className="flex flex-wrap items-end gap-3">
        <div className="grid gap-1">
          <Label className="text-xs text-muted-foreground">{t("bookings.filter.room")}</Label>
          <Select
            value={roomIdFilter ? String(roomIdFilter) : "all"}
            onValueChange={(v) => {
              dispatch(setRoomIdFilter(v === "all" ? null : Number(v)));
              setPage(1);
            }}
          >
            <SelectTrigger className="w-44">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">{t("bookings.filter.allRooms")}</SelectItem>
              {rooms.map((r) => (
                <SelectItem key={r.id} value={String(r.id)}>
                  {r.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="grid gap-1">
          <Label htmlFor="from-date" className="text-xs text-muted-foreground">
            {t("bookings.filter.from")}
          </Label>
          <Input
            id="from-date"
            type="date"
            value={fromDate ?? ""}
            onChange={(e) => {
              dispatch(setFromDate(e.target.value || null));
              setPage(1);
            }}
            className="w-36"
          />
        </div>

        <div className="grid gap-1">
          <Label htmlFor="to-date" className="text-xs text-muted-foreground">
            {t("bookings.filter.to")}
          </Label>
          <Input
            id="to-date"
            type="date"
            value={toDate ?? ""}
            onChange={(e) => {
              dispatch(setToDate(e.target.value || null));
              setPage(1);
            }}
            className="w-36"
          />
        </div>
      </div>

      <BookingTable
        bookings={bookings}
        isLoading={isLoading}
        page={page}
        totalPages={totalPages}
        onPageChange={setPage}
      />

      <BookingFormDialog open={createOpen} onOpenChange={setCreateOpen} />
    </div>
  );
}
