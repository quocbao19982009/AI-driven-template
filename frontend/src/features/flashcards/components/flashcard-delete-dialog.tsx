import { useQueryClient } from "@tanstack/react-query";
import type { FlashcardDto } from "@/api/generated/models";
import {
  useDeleteApiFlashcardsId,
  getGetApiFlashcardsQueryKey,
} from "@/api/generated/flashcards/flashcards";
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

interface FlashcardDeleteDialogProps {
  flashcard?: FlashcardDto | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function FlashcardDeleteDialog({
  flashcard,
  open,
  onOpenChange,
}: FlashcardDeleteDialogProps) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const deleteMutation = useDeleteApiFlashcardsId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiFlashcardsQueryKey(),
        });
        onOpenChange(false);
        toast.success(t("flashcards.toast.deleted"));
      },
      onError: (error) => {
        toast.error(
          error instanceof Error
            ? error.message
            : t("flashcards.toast.deleteError"),
        );
      },
    },
  });

  function handleDelete() {
    if (flashcard?.id) {
      deleteMutation.mutate({ id: flashcard.id });
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{t("flashcards.delete.title")}</DialogTitle>
          <DialogDescription>
            {t("flashcards.delete.confirmPrefix")}{" "}
            <strong>{flashcard?.finnishWord}</strong>
            <span>? </span>
            {t("flashcards.delete.confirmSuffix")}
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
