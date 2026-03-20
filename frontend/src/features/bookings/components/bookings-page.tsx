import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "@/store/hooks";
import { setRoomFilter, setFromDate, setToDate } from "../store/bookings-slice";
import { useGetApiBookings } from "@/api/generated/bookings/bookings";
import { useGetApiRoomsAll } from "@/api/generated/rooms/rooms";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Plus } from "lucide-react";
import { BookingsTable } from "./bookings-table";
import { BookingFormDialog } from "./booking-form-dialog";

const PAGE_SIZE = 10;

export function BookingsPage() {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const { roomFilter, fromDate, toDate } = useAppSelector((s) => s.bookings);
  const [page, setPage] = useState(1);
  const [createOpen, setCreateOpen] = useState(false);

  const { data: roomsResponse } = useGetApiRoomsAll();
  const rooms = roomsResponse?.data?.data ?? [];

  const { data: bookingsResponse, isLoading } = useGetApiBookings({
    page,
    pageSize: PAGE_SIZE,
    roomId: roomFilter ?? undefined,
    fromDate: fromDate ?? undefined,
    toDate: toDate ?? undefined,
  });
  const pagedResult = bookingsResponse?.data?.data;
  const bookings = pagedResult?.items ?? [];
  const totalPages = pagedResult?.totalPages ?? 1;

  function handleRoomFilterChange(value: string) {
    dispatch(setRoomFilter(value ? Number(value) : null));
    setPage(1);
  }

  function handleFromDateChange(value: string) {
    dispatch(setFromDate(value ? new Date(value).toISOString() : null));
    setPage(1);
  }

  function handleToDateChange(value: string) {
    dispatch(setToDate(value ? new Date(value).toISOString() : null));
    setPage(1);
  }

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">
            {t("bookings.title")}
          </h1>
          <p className="text-muted-foreground">{t("bookings.description")}</p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          {t("bookings.newBooking")}
        </Button>
      </div>

      <div className="flex flex-wrap gap-2">
        <select
          className="border-input bg-background h-10 rounded-md border px-3 text-sm"
          value={roomFilter ?? ""}
          onChange={(e) => handleRoomFilterChange(e.target.value)}
        >
          <option value="">{t("bookings.allRooms")}</option>
          {rooms.map((room) => (
            <option key={room.id} value={room.id}>
              {room.name}
            </option>
          ))}
        </select>

        <div className="flex items-center gap-2">
          <label className="text-sm">{t("bookings.from")}</label>
          <Input
            type="date"
            className="w-auto"
            value={fromDate ? fromDate.slice(0, 10) : ""}
            onChange={(e) => handleFromDateChange(e.target.value)}
          />
        </div>
        <div className="flex items-center gap-2">
          <label className="text-sm">{t("bookings.to")}</label>
          <Input
            type="date"
            className="w-auto"
            value={toDate ? toDate.slice(0, 10) : ""}
            onChange={(e) => handleToDateChange(e.target.value)}
          />
        </div>
      </div>

      <BookingsTable
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
