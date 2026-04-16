import { useQueryClient } from "@tanstack/react-query";
import type { FeatureDto } from "@/api/generated/models";
import {
  useDeleteApiFeaturesId,
  getGetApiFeaturesQueryKey,
} from "@/api/generated/feature/feature";
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

interface FeatureDeleteDialogProps {
  feature?: FeatureDto | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function FeatureDeleteDialog({
  feature,
  open,
  onOpenChange,
}: FeatureDeleteDialogProps) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const deleteMutation = useDeleteApiFeaturesId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiFeaturesQueryKey(),
        });
        // If this mutation also affects another feature's cached data, add:
        // queryClient.invalidateQueries({ queryKey: getGetApi<OtherFeature>QueryKey() });
        onOpenChange(false);
        toast.success(t("features.toast.deleted"));
      },
      onError: (error) => {
        toast.error(
          error instanceof Error
            ? error.message
            : t("features.toast.deleteError")
        );
      },
    },
  });

  function handleDelete() {
    if (feature?.id) {
      deleteMutation.mutate({ id: feature.id });
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{t("features.delete.title")}</DialogTitle>
          <DialogDescription>
            {t("features.delete.description", { name: feature?.name ?? "" })}
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
            {deleteMutation.isPending
              ? t("common.deleting")
              : t("common.delete")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
