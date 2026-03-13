import { useQueryClient } from "@tanstack/react-query";
import type { ExpenseTrackerDto } from "@/api/generated/models";
import {
  useDeleteApiExpenseTrackersId,
  getGetApiExpenseTrackersQueryKey,
} from "@/api/generated/expense-trackers/expense-trackers";
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

interface ExpenseTrackerDeleteDialogProps {
  expense?: ExpenseTrackerDto | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function ExpenseTrackerDeleteDialog({
  expense,
  open,
  onOpenChange,
}: ExpenseTrackerDeleteDialogProps) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const deleteMutation = useDeleteApiExpenseTrackersId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiExpenseTrackersQueryKey(),
        });
        onOpenChange(false);
        toast.success(t("expenseTrackers.toast.deleted"));
      },
      onError: (error) => {
        toast.error(
          error instanceof Error
            ? error.message
            : t("expenseTrackers.toast.deleteError")
        );
      },
    },
  });

  function handleDelete() {
    if (expense?.id) {
      deleteMutation.mutate({ id: expense.id });
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{t("expenseTrackers.delete.title")}</DialogTitle>
          <DialogDescription>
            {t("expenseTrackers.delete.description", {
              amount: expense?.amount ?? 0,
              category: expense?.category ?? "",
            })}
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

