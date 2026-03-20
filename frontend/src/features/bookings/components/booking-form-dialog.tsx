import { useEffect } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import type { BookingDto } from "@/api/generated/models";
import {
  usePostApiBookings,
  usePutApiBookingsId,
  getGetApiBookingsQueryKey,
} from "@/api/generated/bookings/bookings";
import { PostApiBookingsBody } from "@/api/generated/bookings/bookings.zod";
import { useGetApiRoomsAll } from "@/api/generated/rooms/rooms";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";

interface BookingFormDialogProps {
  booking?: BookingDto | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  prefillRoomId?: number;
  prefillStartTime?: Date;
  prefillEndTime?: Date;
}

const toLocalDatetimeString = (d: Date) => {
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
};

export function BookingFormDialog({
  booking,
  open,
  onOpenChange,
  prefillRoomId,
  prefillStartTime,
  prefillEndTime,
}: BookingFormDialogProps) {
  const { t } = useTranslation();

  const bookingFormSchema = PostApiBookingsBody.extend({
    roomId: z
      .number({ error: t("bookings.validation.roomRequired") })
      .min(1, t("bookings.validation.roomRequired")),
    startTime: z.string().min(1, t("bookings.validation.startTimeRequired")),
    endTime: z.string().min(1, t("bookings.validation.endTimeRequired")),
    bookedBy: z
      .string()
      .min(1, t("bookings.validation.bookedByRequired"))
      .max(200),
    purpose: z.string().max(500).optional(),
  }).refine((data) => new Date(data.startTime) < new Date(data.endTime), {
    message: t("bookings.validation.startBeforeEnd"),
    path: ["endTime"],
  });

  type BookingFormValues = z.infer<typeof bookingFormSchema>;

  const isEditing = !!booking;
  const queryClient = useQueryClient();

  const { data: roomsResponse } = useGetApiRoomsAll();
  const rooms = roomsResponse?.data?.data ?? [];

  const now = new Date();
  const form = useForm<BookingFormValues>({
    resolver: zodResolver(bookingFormSchema),
    defaultValues: {
      roomId: prefillRoomId ?? undefined,
      startTime: prefillStartTime
        ? toLocalDatetimeString(prefillStartTime)
        : toLocalDatetimeString(now),
      endTime: prefillEndTime
        ? toLocalDatetimeString(prefillEndTime)
        : toLocalDatetimeString(new Date(now.getTime() + 60 * 60 * 1000)),
      bookedBy: "",
      purpose: "",
    },
  });

  useEffect(() => {
    if (open) {
      if (booking) {
        form.reset({
          roomId: booking.roomId,
          startTime: toLocalDatetimeString(new Date(booking.startTime)),
          endTime: toLocalDatetimeString(new Date(booking.endTime)),
          bookedBy: booking.bookedBy,
          purpose: booking.purpose ?? "",
        });
      } else {
        const defaultStart = prefillStartTime ?? now;
        const defaultEnd =
          prefillEndTime ?? new Date(now.getTime() + 60 * 60 * 1000);
        form.reset({
          roomId: prefillRoomId ?? undefined,
          startTime: toLocalDatetimeString(defaultStart),
          endTime: toLocalDatetimeString(defaultEnd),
          bookedBy: "",
          purpose: "",
        });
      }
    }
  }, [open, booking, prefillRoomId, prefillStartTime, prefillEndTime]); // eslint-disable-line react-hooks/exhaustive-deps

  const createMutation = usePostApiBookings({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiBookingsQueryKey(),
        });
        onOpenChange(false);
        toast.success(t("bookings.toast.created"));
      },
      onError: (error) => {
        toast.error(
          error instanceof Error
            ? error.message
            : t("bookings.toast.createError")
        );
      },
    },
  });

  const updateMutation = usePutApiBookingsId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiBookingsQueryKey(),
        });
        onOpenChange(false);
        toast.success(t("bookings.toast.updated"));
      },
      onError: (error) => {
        toast.error(
          error instanceof Error
            ? error.message
            : t("bookings.toast.updateError")
        );
      },
    },
  });

  const isPending = createMutation.isPending || updateMutation.isPending;

  function onSubmit(values: BookingFormValues) {
    const payload = {
      roomId: values.roomId,
      startTime: new Date(values.startTime).toISOString(),
      endTime: new Date(values.endTime).toISOString(),
      bookedBy: values.bookedBy,
      purpose: values.purpose || undefined,
    };
    if (isEditing && booking?.id) {
      updateMutation.mutate({ id: booking.id, data: payload });
    } else {
      createMutation.mutate({ data: payload });
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)}>
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
              <FormField
                control={form.control}
                name="roomId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t("bookings.form.roomLabel")}</FormLabel>
                    <FormControl>
                      <select
                        className="border-input bg-background ring-offset-background focus-visible:ring-ring flex h-10 w-full rounded-md border px-3 py-2 text-sm focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:outline-none"
                        value={field.value ?? ""}
                        onChange={(e) =>
                          field.onChange(Number(e.target.value) || undefined)
                        }
                      >
                        <option value="">
                          {t("bookings.form.roomPlaceholder")}
                        </option>
                        {rooms.map((room) => (
                          <option key={room.id} value={room.id}>
                            {room.name}
                          </option>
                        ))}
                      </select>
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="startTime"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t("bookings.form.startTimeLabel")}</FormLabel>
                    <FormControl>
                      <Input type="datetime-local" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="endTime"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t("bookings.form.endTimeLabel")}</FormLabel>
                    <FormControl>
                      <Input type="datetime-local" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="bookedBy"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t("bookings.form.bookedByLabel")}</FormLabel>
                    <FormControl>
                      <Input
                        placeholder={t("bookings.form.bookedByPlaceholder")}
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="purpose"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t("bookings.form.purposeLabel")}</FormLabel>
                    <FormControl>
                      <Textarea
                        placeholder={t("bookings.form.purposePlaceholder")}
                        rows={3}
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
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
              <Button type="submit" disabled={isPending}>
                {isPending
                  ? t("common.saving")
                  : isEditing
                    ? t("common.save")
                    : t("common.create")}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
