import { useQueryClient } from "@tanstack/react-query";
import type { FactoryDto } from "@/api/generated/models";
import {
  useDeleteApiFactoriesId,
  getGetApiFactoriesQueryKey,
  getGetApiFactoriesAllQueryKey,
} from "@/api/generated/factories/factories";
import {
  getGetApiPersonnelQueryKey,
  getGetApiPersonnelAllQueryKey,
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

interface FactoryDeleteDialogProps {
  factory?: FactoryDto | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function FactoryDeleteDialog({
  factory,
  open,
  onOpenChange,
}: FactoryDeleteDialogProps) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const deleteMutation = useDeleteApiFactoriesId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiFactoriesQueryKey() });
        queryClient.invalidateQueries({ queryKey: getGetApiFactoriesAllQueryKey() });
        queryClient.invalidateQueries({ queryKey: getGetApiPersonnelQueryKey() });
        queryClient.invalidateQueries({ queryKey: getGetApiPersonnelAllQueryKey() });
        onOpenChange(false);
        toast.success(t("factories.toast.deleted"));
      },
      onError: (error) => {
        toast.error(
          error instanceof Error ? error.message : t("factories.toast.deleteError"),
        );
      },
    },
  });

  function handleDelete() {
    if (factory?.id) {
      deleteMutation.mutate({ id: factory.id });
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{t("factories.delete.title")}</DialogTitle>
          <DialogDescription>
            {t("factories.delete.confirmPrefix")} <strong>{factory?.name}</strong>
            <span>? </span>
            {t("factories.delete.confirmSuffix")}
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
            onClick={handleDelete}
            disabled={deleteMutation.isPending}
          >
            {deleteMutation.isPending ? t("common.deleting") : t("common.delete")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
