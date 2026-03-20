import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "@/store/hooks";
import { setSelectedRoomId } from "../store/rooms-slice";
import { useGetApiRoomsAll } from "@/api/generated/rooms/rooms";
import { useGetApiBookings } from "@/api/generated/bookings/bookings";
import type { BookingDto } from "@/api/generated/models";
import { Button } from "@/components/ui/button";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { BookingFormDialog } from "@/features/bookings/components/booking-form-dialog";
import { BookingDetailDialog } from "@/features/bookings/components/booking-detail-dialog";

function startOfWeek(date: Date): Date {
  const d = new Date(date);
  const day = d.getDay();
  const diff = d.getDate() - day + (day === 0 ? -6 : 1); // Monday first
  d.setDate(diff);
  d.setHours(0, 0, 0, 0);
  return d;
}

function addDays(date: Date, days: number): Date {
  const d = new Date(date);
  d.setDate(d.getDate() + days);
  return d;
}

const HOURS = Array.from({ length: 14 }, (_, i) => i + 7); // 7:00 to 20:00

export function RoomsCalendar() {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const selectedRoomId = useAppSelector((s) => s.rooms.selectedRoomId);
  const [weekStart, setWeekStart] = useState(() => startOfWeek(new Date()));
  const [createSlot, setCreateSlot] = useState<{
    start: Date;
    end: Date;
  } | null>(null);
  const [selectedBooking, setSelectedBooking] = useState<BookingDto | null>(
    null
  );

  const { data: roomsResponse } = useGetApiRoomsAll();
  const rooms = roomsResponse?.data?.data ?? [];

  const weekEnd = addDays(weekStart, 7);
  const { data: bookingsResponse } = useGetApiBookings(
    selectedRoomId
      ? {
          roomId: selectedRoomId,
          fromDate: weekStart.toISOString(),
          toDate: weekEnd.toISOString(),
          pageSize: 100,
        }
      : null,
    { query: { enabled: selectedRoomId !== null } }
  );
  const bookings: BookingDto[] = bookingsResponse?.data?.data?.items ?? [];

  const weekDays = Array.from({ length: 7 }, (_, i) => addDays(weekStart, i));

  function prevWeek() {
    setWeekStart((d) => addDays(d, -7));
  }

  function nextWeek() {
    setWeekStart((d) => addDays(d, 7));
  }

  function handleSlotClick(day: Date, hour: number) {
    const start = new Date(day);
    start.setHours(hour, 0, 0, 0);
    const end = new Date(day);
    end.setHours(hour + 1, 0, 0, 0);
    setCreateSlot({ start, end });
  }

  function getBookingsForDayHour(day: Date, hour: number): BookingDto[] {
    return bookings.filter((b) => {
      const start = new Date(b.startTime);
      const end = new Date(b.endTime);
      const slotStart = new Date(day);
      slotStart.setHours(hour, 0, 0, 0);
      const slotEnd = new Date(day);
      slotEnd.setHours(hour + 1, 0, 0, 0);
      return (
        start.toDateString() === day.toDateString() &&
        start < slotEnd &&
        end > slotStart
      );
    });
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-4">
        <div className="flex items-center gap-2">
          <label className="text-sm font-medium">
            {t("rooms.calendar.roomLabel")}
          </label>
          <select
            className="border-input bg-background h-9 rounded-md border px-3 text-sm"
            value={selectedRoomId ?? ""}
            onChange={(e) =>
              dispatch(setSelectedRoomId(Number(e.target.value) || null))
            }
          >
            <option value="">{t("rooms.calendar.selectRoom")}</option>
            {rooms.map((room) => (
              <option key={room.id} value={room.id}>
                {room.name}
              </option>
            ))}
          </select>
        </div>

        <div className="ml-auto flex items-center gap-2">
          <Button variant="outline" size="sm" onClick={prevWeek}>
            <ChevronLeft className="h-4 w-4" />
          </Button>
          <span className="text-sm font-medium">
            {weekStart.toLocaleDateString()} –{" "}
            {addDays(weekStart, 6).toLocaleDateString()}
          </span>
          <Button variant="outline" size="sm" onClick={nextWeek}>
            <ChevronRight className="h-4 w-4" />
          </Button>
        </div>
      </div>

      {!selectedRoomId ? (
        <div className="text-muted-foreground flex h-48 items-center justify-center rounded-md border">
          {t("rooms.calendar.selectRoomPrompt")}
        </div>
      ) : (
        <div className="overflow-auto rounded-md border">
          <div
            className="grid"
            style={{ gridTemplateColumns: `60px repeat(7, 1fr)` }}
          >
            {/* Header row */}
            <div className="border-b p-2" />
            {weekDays.map((day) => (
              <div
                key={day.toISOString()}
                className="border-b border-l p-2 text-center text-xs font-medium"
              >
                <div>
                  {day.toLocaleDateString(undefined, { weekday: "short" })}
                </div>
                <div className="text-muted-foreground">
                  {day.toLocaleDateString(undefined, {
                    month: "numeric",
                    day: "numeric",
                  })}
                </div>
              </div>
            ))}

            {/* Time rows */}
            {HOURS.map((hour) => (
              <>
                <div
                  key={`hour-${hour}`}
                  className="text-muted-foreground border-b px-2 py-1 text-right text-xs"
                >
                  {String(hour).padStart(2, "0")}:00
                </div>
                {weekDays.map((day) => {
                  const slotBookings = getBookingsForDayHour(day, hour);
                  return (
                    <div
                      key={`${day.toISOString()}-${hour}`}
                      className="hover:bg-accent/30 relative h-12 cursor-pointer border-b border-l"
                      onClick={() => {
                        if (slotBookings.length === 0)
                          handleSlotClick(day, hour);
                      }}
                    >
                      {slotBookings.map((b) => (
                        <div
                          key={b.id}
                          className="bg-primary/90 text-primary-foreground absolute inset-x-0.5 inset-y-0.5 overflow-hidden rounded px-1 py-0.5 text-xs"
                          onClick={(e) => {
                            e.stopPropagation();
                            setSelectedBooking(b);
                          }}
                        >
                          <div className="truncate font-medium">
                            {b.bookedBy}
                          </div>
                          <div className="truncate opacity-80">{b.purpose}</div>
                        </div>
                      ))}
                    </div>
                  );
                })}
              </>
            ))}
          </div>
        </div>
      )}

      {createSlot && selectedRoomId && (
        <BookingFormDialog
          open
          onOpenChange={(open) => !open && setCreateSlot(null)}
          prefillRoomId={selectedRoomId}
          prefillStartTime={createSlot.start}
          prefillEndTime={createSlot.end}
        />
      )}

      {selectedBooking && (
        <BookingDetailDialog
          booking={selectedBooking}
          open
          onOpenChange={(open) => !open && setSelectedBooking(null)}
        />
      )}
    </div>
  );
}

