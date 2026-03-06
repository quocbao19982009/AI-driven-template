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
  useDeleteApiRoomsId,
  getGetApiRoomsQueryKey,
  getGetApiRoomsAllQueryKey,
} from "@/api/generated/rooms/rooms";
import type { RoomDto } from "@/api/generated/models";

interface RoomDeleteDialogProps {
  room?: RoomDto | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function RoomDeleteDialog({ room, open, onOpenChange }: RoomDeleteDialogProps) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const deleteMutation = useDeleteApiRoomsId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiRoomsQueryKey() });
        queryClient.invalidateQueries({ queryKey: getGetApiRoomsAllQueryKey() });
        onOpenChange(false);
        toast.success(t("rooms.toast.deleted"));
      },
      onError: (err) =>
        toast.error(err instanceof Error ? err.message : t("rooms.toast.deleteError")),
    },
  });

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{t("rooms.delete.title")}</DialogTitle>
          <DialogDescription
            dangerouslySetInnerHTML={{
              __html: t("rooms.delete.description", { name: room?.name ?? "" }),
            }}
          />
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
            onClick={() => room?.id && deleteMutation.mutate({ id: room.id })}
            disabled={deleteMutation.isPending}
          >
            {deleteMutation.isPending ? t("common.deleting") : t("common.delete")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
