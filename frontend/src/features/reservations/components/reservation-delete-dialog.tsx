import { useQueryClient } from "@tanstack/react-query";
import type { ReservationDto } from "@/api/generated/models";
import {
  useDeleteApiReservationsId,
  getGetApiReservationsQueryKey,
} from "@/api/generated/reservations/reservations";
import {
  getGetApiSchedulingByPersonQueryKey,
  getGetApiSchedulingByFactoryQueryKey,
} from "@/api/generated/scheduling/scheduling";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

interface ReservationDeleteDialogProps {
  reservation?: ReservationDto | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function ReservationDeleteDialog({
  reservation,
  open,
  onOpenChange,
}: ReservationDeleteDialogProps) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const deleteMutation = useDeleteApiReservationsId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiReservationsQueryKey() });
        queryClient.invalidateQueries({ queryKey: getGetApiSchedulingByPersonQueryKey() });
        queryClient.invalidateQueries({ queryKey: getGetApiSchedulingByFactoryQueryKey() });
        onOpenChange(false);
        toast.success(t("reservations.toast.deleted"));
      },
      onError: (error) => {
        toast.error(error instanceof Error ? error.message : t("reservations.toast.deleteError"));
      },
    },
  });

  function handleDelete() {
    if (reservation?.id) {
      deleteMutation.mutate({ id: reservation.id });
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{t("reservations.delete.title")}</DialogTitle>
          <DialogDescription>
            {t("reservations.delete.confirmMessage", {
              factory: reservation?.factoryDisplayName,
              date: reservation?.startTime
                ? new Date(reservation.startTime).toLocaleDateString()
                : "—",
            })}
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button type="button" variant="outline" onClick={() => onOpenChange(false)} disabled={deleteMutation.isPending}>
            {t("common.cancel")}
          </Button>
          <Button variant="destructive" onClick={handleDelete} disabled={deleteMutation.isPending}>
            {deleteMutation.isPending ? t("common.deleting") : t("common.delete")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
