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
import {
  useDeleteApiBookingsId,
  getGetApiBookingsQueryKey,
} from "@/api/generated/bookings/bookings";
import type { BookingDto } from "@/api/generated/models";

interface BookingDeleteDialogProps {
  booking?: BookingDto | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function BookingDeleteDialog({ booking, open, onOpenChange }: BookingDeleteDialogProps) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const deleteMutation = useDeleteApiBookingsId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiBookingsQueryKey() });
        onOpenChange(false);
        toast.success(t("bookings.toast.deleted"));
      },
      onError: (err) =>
        toast.error(err instanceof Error ? err.message : t("bookings.toast.deleteError")),
    },
  });

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{t("bookings.delete.title")}</DialogTitle>
          <DialogDescription>
            {t("bookings.delete.description", { room: booking?.roomName ?? "" })}
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button
            type="button"
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={deleteMutation.isPending}
          >
            {t("common.cancel")}
          </Button>
          <Button
            variant="destructive"
            onClick={() => booking?.id && deleteMutation.mutate({ id: booking.id })}
            disabled={deleteMutation.isPending}
          >
            {deleteMutation.isPending ? t("common.deleting") : t("common.delete")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
