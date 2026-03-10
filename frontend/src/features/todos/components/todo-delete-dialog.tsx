import { useQueryClient } from "@tanstack/react-query";
import {
  useDeleteApiTodosId,
  getGetApiTodosQueryKey,
} from "@/api/generated/todos/todos";
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
import type { TodoDto } from "../hooks";

interface TodoDeleteDialogProps {
  todo?: TodoDto | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function TodoDeleteDialog({
  todo,
  open,
  onOpenChange,
}: TodoDeleteDialogProps) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const deleteMutation = useDeleteApiTodosId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiTodosQueryKey() });
        onOpenChange(false);
        toast.success(t("todos.toast.deleted"));
      },
      onError: (error) => {
        toast.error(
          error instanceof Error ? error.message : t("todos.toast.deleteError")
        );
      },
    },
  });

  function handleDelete() {
    if (todo?.id) {
      deleteMutation.mutate({ id: todo.id });
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{t("todos.delete.title")}</DialogTitle>
          <DialogDescription
            dangerouslySetInnerHTML={{
              __html: t("todos.delete.description", {
                title: todo?.title ?? "",
              }),
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
