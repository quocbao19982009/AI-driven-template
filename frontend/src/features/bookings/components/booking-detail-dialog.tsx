import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { BookingDto } from "@/api/generated/models";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { BookingFormDialog } from "./booking-form-dialog";
import { BookingDeleteDialog } from "./booking-delete-dialog";

interface BookingDetailDialogProps {
  booking: BookingDto;
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

  return (
    <>
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>{t("bookings.detail.title")}</DialogTitle>
            <DialogDescription>{booking.roomName}</DialogDescription>
          </DialogHeader>
          <div className="space-y-2 text-sm">
            <div>
              <span className="text-muted-foreground">
                {t("bookings.table.start")}:{" "}
              </span>
              {new Date(booking.startTime).toLocaleString()}
            </div>
            <div>
              <span className="text-muted-foreground">
                {t("bookings.table.end")}:{" "}
              </span>
              {new Date(booking.endTime).toLocaleString()}
            </div>
            <div>
              <span className="text-muted-foreground">
                {t("bookings.table.bookedBy")}:{" "}
              </span>
              {booking.bookedBy}
            </div>
            {booking.purpose && (
              <div>
                <span className="text-muted-foreground">
                  {t("bookings.table.purpose")}:{" "}
                </span>
                {booking.purpose}
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
        onOpenChange={(open) => {
          setEditOpen(open);
          if (!open) onOpenChange(false);
        }}
      />

      <BookingDeleteDialog
        booking={booking}
        open={deleteOpen}
        onOpenChange={(open) => {
          setDeleteOpen(open);
          if (!open) onOpenChange(false);
        }}
      />
    </>
  );
}
