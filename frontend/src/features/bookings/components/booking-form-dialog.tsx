import { useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  usePostApiBookingsWithJson,
  usePutApiBookingsIdWithJson,
  getGetApiBookingsQueryKey,
} from "@/api/generated/bookings/bookings";
import { useGetApiRoomsAll } from "@/api/generated/rooms/rooms";
import type {
  BookingDto,
  CreateBookingRequest,
  RoomDto,
  UpdateBookingRequest,
} from "@/api/generated/models";

interface BookingFormDialogProps {
  booking?: BookingDto | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  defaults?: { roomId?: number; startTime?: string; endTime?: string };
}

function toInputDatetime(iso: string | Date | undefined | null): string {
  if (!iso) return "";
  const d = typeof iso === "string" ? new Date(iso) : iso;
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

export function BookingFormDialog({
  booking,
  open,
  onOpenChange,
  defaults,
}: BookingFormDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <BookingFormContent
        key={open ? (booking?.id ?? defaults?.startTime ?? "new") : "closed"}
        booking={booking}
        open={open}
        onOpenChange={onOpenChange}
        defaults={defaults}
      />
    </Dialog>
  );
}

function BookingFormContent({
  booking,
  open,
  onOpenChange,
  defaults,
}: BookingFormDialogProps) {
  const { t } = useTranslation();
  const isEditing = !!booking;
  const queryClient = useQueryClient();

  const { data: roomsRes } = useGetApiRoomsAll();
  const rooms: RoomDto[] = roomsRes?.data?.data ?? [];

  const [roomId, setRoomId] = useState<string>(
    booking?.roomId
      ? String(booking.roomId)
      : defaults?.roomId
        ? String(defaults.roomId)
        : ""
  );
  const [startTime, setStartTime] = useState(
    booking?.startTime
      ? toInputDatetime(booking.startTime)
      : (defaults?.startTime ?? "")
  );
  const [endTime, setEndTime] = useState(
    booking?.endTime
      ? toInputDatetime(booking.endTime)
      : (defaults?.endTime ?? "")
  );
  const [bookedBy, setBookedBy] = useState(booking?.bookedBy ?? "");
  const [purpose, setPurpose] = useState(booking?.purpose ?? "");
  const [overlapError, setOverlapError] = useState<string | null>(null);

  const invalidate = () =>
    queryClient.invalidateQueries({ queryKey: getGetApiBookingsQueryKey() });

  const createMutation = usePostApiBookingsWithJson({
    mutation: {
      onSuccess: () => {
        invalidate();
        onOpenChange(false);
        toast.success(t("bookings.toast.created"));
      },
      onError: (err) => {
        const msg =
          err instanceof Error ? err.message : t("bookings.toast.createError");
        if (
          msg.toLowerCase().includes("overlap") ||
          msg.toLowerCase().includes("booked")
        ) {
          setOverlapError(msg);
        } else {
          toast.error(msg);
        }
      },
    },
  });

  const updateMutation = usePutApiBookingsIdWithJson({
    mutation: {
      onSuccess: () => {
        invalidate();
        onOpenChange(false);
        toast.success(t("bookings.toast.updated"));
      },
      onError: (err) => {
        const msg =
          err instanceof Error ? err.message : t("bookings.toast.updateError");
        if (
          msg.toLowerCase().includes("overlap") ||
          msg.toLowerCase().includes("booked")
        ) {
          setOverlapError(msg);
        } else {
          toast.error(msg);
        }
      },
    },
  });

  const isPending = createMutation.isPending || updateMutation.isPending;

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setOverlapError(null);
    const payload: CreateBookingRequest & UpdateBookingRequest = {
      roomId: Number(roomId),
      startTime: new Date(startTime).toISOString(),
      endTime: new Date(endTime).toISOString(),
      bookedBy,
      purpose: purpose || undefined,
    };
    if (isEditing && booking?.id) {
      updateMutation.mutate({ id: booking.id, data: payload });
    } else {
      createMutation.mutate({ data: payload });
    }
  }

  if (!open) return null;

  return (
    <DialogContent className="sm:max-w-lg">
      <form onSubmit={handleSubmit}>
        <DialogHeader>
          <DialogTitle>
            {isEditing
              ? t("bookings.form.editTitle")
              : t("bookings.form.createTitle")}
          </DialogTitle>
          <DialogDescription>
            {isEditing
              ? t("bookings.form.editDescription")
              : t("bookings.form.createDescription")}
          </DialogDescription>
        </DialogHeader>

        <div className="grid gap-4 py-4">
          <div className="grid gap-1.5">
            <Label>{t("bookings.form.roomLabel")}</Label>
            <Select value={roomId} onValueChange={setRoomId} required>
              <SelectTrigger>
                <SelectValue placeholder={t("bookings.form.roomPlaceholder")} />
              </SelectTrigger>
              <SelectContent>
                {rooms.map((r) => (
                  <SelectItem key={r.id} value={String(r.id)}>
                    {r.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div className="grid gap-1.5">
              <Label htmlFor="start-time">
                {t("bookings.form.startLabel")}
              </Label>
              <Input
                id="start-time"
                type="datetime-local"
                value={startTime}
                onChange={(e) => setStartTime(e.target.value)}
                required
              />
            </div>
            <div className="grid gap-1.5">
              <Label htmlFor="end-time">{t("bookings.form.endLabel")}</Label>
              <Input
                id="end-time"
                type="datetime-local"
                value={endTime}
                onChange={(e) => setEndTime(e.target.value)}
                required
              />
            </div>
          </div>

          <div className="grid gap-1.5">
            <Label htmlFor="booked-by">
              {t("bookings.form.bookedByLabel")}
            </Label>
            <Input
              id="booked-by"
              value={bookedBy}
              onChange={(e) => setBookedBy(e.target.value)}
              placeholder={t("bookings.form.bookedByPlaceholder")}
              required
            />
          </div>

          <div className="grid gap-1.5">
            <Label htmlFor="booking-purpose">
              {t("bookings.form.purposeLabel")}
            </Label>
            <Textarea
              id="booking-purpose"
              value={purpose}
              onChange={(e) => setPurpose(e.target.value)}
              placeholder={t("bookings.form.purposePlaceholder")}
              rows={2}
            />
          </div>

          {overlapError && (
            <p className="text-sm text-destructive">{overlapError}</p>
          )}
        </div>

        <DialogFooter>
          <Button
            type="button"
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={isPending}
          >
            {t("common.cancel")}
          </Button>
          <Button type="submit" disabled={isPending || !roomId}>
            {isPending
              ? t("common.saving")
              : isEditing
                ? t("common.save")
                : t("common.create")}
          </Button>
        </DialogFooter>
      </form>
    </DialogContent>
  );
}
