import { useState } from "react";
import { useTranslation } from "react-i18next";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { BookingFormDialog } from "./booking-form-dialog";
import { BookingDeleteDialog } from "./booking-delete-dialog";
import type { BookingDto } from "@/api/generated/models";

interface BookingDetailDialogProps {
  booking?: BookingDto | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function BookingDetailDialog({
  booking,
  open,
  onOpenChange,
}: BookingDetailDialogProps) {
  const { t } = useTranslation();
  const [editOpen, setEditOpen] = useState(false);
  const [deleteOpen, setDeleteOpen] = useState(false);

  if (!booking) return null;

  const fmt = (iso: string | undefined | null) =>
    iso ? new Date(iso).toLocaleString() : "—";

  return (
    <>
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>{t("bookings.detail.title")}</DialogTitle>
          </DialogHeader>

          <div className="grid gap-3 py-2 text-sm">
            <div className="grid grid-cols-3 gap-1">
              <span className="text-muted-foreground">
                {t("bookings.table.room")}
              </span>
              <span className="col-span-2 font-medium">{booking.roomName}</span>
            </div>
            <div className="grid grid-cols-3 gap-1">
              <span className="text-muted-foreground">
                {t("bookings.form.startLabel")}
              </span>
              <span className="col-span-2">{fmt(booking.startTime)}</span>
            </div>
            <div className="grid grid-cols-3 gap-1">
              <span className="text-muted-foreground">
                {t("bookings.form.endLabel")}
              </span>
              <span className="col-span-2">{fmt(booking.endTime)}</span>
            </div>
            <div className="grid grid-cols-3 gap-1">
              <span className="text-muted-foreground">
                {t("bookings.table.bookedBy")}
              </span>
              <span className="col-span-2">{booking.bookedBy}</span>
            </div>
            {booking.purpose && (
              <div className="grid grid-cols-3 gap-1">
                <span className="text-muted-foreground">
                  {t("bookings.table.purpose")}
                </span>
                <span className="col-span-2">{booking.purpose}</span>
              </div>
            )}
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => onOpenChange(false)}>
              {t("common.cancel")}
            </Button>
            <Button variant="outline" onClick={() => setEditOpen(true)}>
              {t("common.edit")}
            </Button>
            <Button variant="destructive" onClick={() => setDeleteOpen(true)}>
              {t("common.delete")}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <BookingFormDialog
        booking={booking}
        open={editOpen}
        onOpenChange={(o) => {
          setEditOpen(o);
          if (!o) onOpenChange(false);
        }}
      />
      <BookingDeleteDialog
        booking={booking}
        open={deleteOpen}
        onOpenChange={(o) => {
          setDeleteOpen(o);
          if (!o) onOpenChange(false);
        }}
      />
    </>
  );
}
