import { useQueryClient } from "@tanstack/react-query";
import type { PersonDto } from "@/api/generated/models";
import {
  useDeleteApiPersonnelId,
  getGetApiPersonnelQueryKey,
} from "@/api/generated/personnel/personnel";
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

interface PersonDeleteDialogProps {
  person?: PersonDto | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function PersonDeleteDialog({
  person,
  open,
  onOpenChange,
}: PersonDeleteDialogProps) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const deleteMutation = useDeleteApiPersonnelId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiPersonnelQueryKey() });
        onOpenChange(false);
        toast.success(t("personnel.toast.deleted"));
      },
      onError: (error) => {
        toast.error(error instanceof Error ? error.message : t("personnel.toast.deleteError"));
      },
    },
  });

  function handleDelete() {
    if (person?.id) {
      deleteMutation.mutate({ id: person.id });
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{t("personnel.delete.title")}</DialogTitle>
          <DialogDescription>
            {t("personnel.delete.confirmPrefix")} <strong>{person?.fullName}</strong>
            <span>? </span>
            {t("personnel.delete.confirmSuffix")}
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
