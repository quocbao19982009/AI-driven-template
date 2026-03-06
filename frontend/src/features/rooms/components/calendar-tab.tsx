import { useState } from "react";
import { useTranslation } from "react-i18next";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useGetApiRoomsAll } from "@/api/generated/rooms/rooms";
import { useGetApiBookings } from "@/api/generated/bookings/bookings";
import type { BookingDto, RoomDto } from "@/api/generated/models";
import { useAppDispatch, useAppSelector } from "@/store/hooks";
import { setSelectedRoomIdForCalendar } from "../store";
import { BookingFormDialog } from "@/features/bookings/components/booking-form-dialog";
import { BookingDetailDialog } from "@/features/bookings/components/booking-detail-dialog";

const HOURS = Array.from({ length: 24 }, (_, i) => i);
const DAYS = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];

function startOfWeek(date: Date): Date {
  const d = new Date(date);
  d.setDate(d.getDate() - d.getDay());
  d.setHours(0, 0, 0, 0);
  return d;
}

function addDays(date: Date, days: number): Date {
  const d = new Date(date);
  d.setDate(d.getDate() + days);
  return d;
}

function sameDay(a: Date, b: Date) {
  return (
    a.getFullYear() === b.getFullYear() &&
    a.getMonth() === b.getMonth() &&
    a.getDate() === b.getDate()
  );
}

function toLocalISOString(date: Date) {
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

export function CalendarTab() {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const selectedRoomId = useAppSelector((s) => s.rooms.selectedRoomIdForCalendar);
  const [weekStart, setWeekStart] = useState(() => startOfWeek(new Date()));

  const [createOpen, setCreateOpen] = useState(false);
  const [createDefaults, setCreateDefaults] = useState<{
    roomId?: number;
    startTime?: string;
    endTime?: string;
  }>({});

  const [detailBooking, setDetailBooking] = useState<BookingDto | null>(null);

  const { data: roomsRes } = useGetApiRoomsAll();
  const rooms: RoomDto[] = roomsRes?.data?.data ?? [];

  const weekEnd = addDays(weekStart, 7);
  const { data: bookingsRes } = useGetApiBookings(
    {
      roomId: selectedRoomId ?? undefined,
      fromDate: weekStart.toISOString(),
      toDate: weekEnd.toISOString(),
      pageSize: 200,
    },
    { query: { enabled: !!selectedRoomId } },
  );
  const bookings: BookingDto[] = bookingsRes?.data?.data?.items ?? [];

  const days = Array.from({ length: 7 }, (_, i) => addDays(weekStart, i));

  function handleSlotClick(day: Date, hour: number) {
    if (!selectedRoomId) return;
    const start = new Date(day);
    start.setHours(hour, 0, 0, 0);
    const end = new Date(day);
    end.setHours(hour + 1, 0, 0, 0);
    setCreateDefaults({
      roomId: selectedRoomId,
      startTime: toLocalISOString(start),
      endTime: toLocalISOString(end),
    });
    setCreateOpen(true);
  }

  function getBookingsForSlot(day: Date, hour: number): BookingDto[] {
    return bookings.filter((b) => {
      const start = new Date(b.startTime!);
      const end = new Date(b.endTime!);
      return sameDay(start, day) && start.getHours() <= hour && end.getHours() > hour;
    });
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3 flex-wrap">
        <Select
          value={selectedRoomId ? String(selectedRoomId) : ""}
          onValueChange={(v) => dispatch(setSelectedRoomIdForCalendar(v ? Number(v) : null))}
        >
          <SelectTrigger className="w-56">
            <SelectValue placeholder={t("rooms.calendar.selectRoom")} />
          </SelectTrigger>
          <SelectContent>
            {rooms.map((r) => (
              <SelectItem key={r.id} value={String(r.id)}>
                {r.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <div className="flex items-center gap-1 ml-auto">
          <Button
            variant="outline"
            size="icon"
            onClick={() => setWeekStart((w) => addDays(w, -7))}
          >
            <ChevronLeft className="h-4 w-4" />
          </Button>
          <span className="text-sm font-medium px-2">
            {weekStart.toLocaleDateString()} – {addDays(weekStart, 6).toLocaleDateString()}
          </span>
          <Button
            variant="outline"
            size="icon"
            onClick={() => setWeekStart((w) => addDays(w, 7))}
          >
            <ChevronRight className="h-4 w-4" />
          </Button>
          <Button
            variant="outline"
            size="sm"
            className="ml-1"
            onClick={() => setWeekStart(startOfWeek(new Date()))}
          >
            {t("rooms.calendar.today")}
          </Button>
        </div>
      </div>

      {!selectedRoomId ? (
        <div className="flex h-48 items-center justify-center text-muted-foreground">
          {t("rooms.calendar.selectRoomPrompt")}
        </div>
      ) : (
        <div className="overflow-auto border rounded-md">
          <table className="w-full text-xs border-collapse">
            <thead>
              <tr>
                <th className="w-14 border-b border-r p-1 bg-muted text-muted-foreground font-normal" />
                {days.map((day, i) => (
                  <th
                    key={i}
                    className="border-b border-r p-1 bg-muted text-center font-medium min-w-[100px]"
                  >
                    <div>{DAYS[day.getDay()]}</div>
                    <div className="text-muted-foreground">{day.getDate()}</div>
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {HOURS.map((hour) => (
                <tr key={hour}>
                  <td className="border-b border-r p-1 text-right text-muted-foreground align-top w-14">
                    {String(hour).padStart(2, "0")}:00
                  </td>
                  {days.map((day, di) => {
                    const slotBookings = getBookingsForSlot(day, hour);
                    return (
                      <td
                        key={di}
                        className="border-b border-r p-0.5 align-top cursor-pointer hover:bg-muted/50 h-10"
                        onClick={() => slotBookings.length === 0 && handleSlotClick(day, hour)}
                      >
                        {slotBookings.map((b) => (
                          <button
                            key={b.id}
                            className="w-full rounded text-left px-1 py-0.5 bg-primary/20 text-primary hover:bg-primary/30 truncate block"
                            onClick={(e) => {
                              e.stopPropagation();
                              setDetailBooking(b);
                            }}
                          >
                            {b.bookedBy}
                          </button>
                        ))}
                      </td>
                    );
                  })}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <BookingFormDialog
        open={createOpen}
        onOpenChange={setCreateOpen}
        defaults={createDefaults}
      />

      <BookingDetailDialog
        booking={detailBooking}
        open={detailBooking !== null}
        onOpenChange={(open) => !open && setDetailBooking(null)}
      />
    </div>
  );
}
