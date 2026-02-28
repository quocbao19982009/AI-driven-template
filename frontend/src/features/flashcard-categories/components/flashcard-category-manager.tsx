import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useQueryClient } from "@tanstack/react-query";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Pencil, Trash2, Plus, Check, X } from "lucide-react";
import {
  useGetApiFlashcardCategoriesAll,
  getGetApiFlashcardCategoriesAllQueryKey,
  usePostApiFlashcardCategoriesWithJson,
  usePutApiFlashcardCategoriesIdWithJson,
  useDeleteApiFlashcardCategoriesId,
} from "@/api/generated/flashcard-categories/flashcard-categories";
import { getGetApiFlashcardsQueryKey } from "@/api/generated/flashcards/flashcards";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { toast } from "sonner";

export function FlashcardCategoryManager() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const { data: response } = useGetApiFlashcardCategoriesAll();
  const categories = response?.data?.data ?? [];

  const [newName, setNewName] = useState("");
  const [editingId, setEditingId] = useState<number | null>(null);
  const [editingName, setEditingName] = useState("");
  const [deleteTarget, setDeleteTarget] = useState<{
    id: number;
    name: string;
  } | null>(null);

  const invalidateAll = () => {
    queryClient.invalidateQueries({
      queryKey: getGetApiFlashcardCategoriesAllQueryKey(),
    });
    queryClient.invalidateQueries({
      queryKey: getGetApiFlashcardsQueryKey(),
    });
  };

  const createMutation = usePostApiFlashcardCategoriesWithJson({
    mutation: {
      onSuccess: () => {
        invalidateAll();
        setNewName("");
        toast.success(t("flashcardCategories.toast.created"));
      },
      onError: (error) => {
        toast.error(
          error instanceof Error
            ? error.message
            : t("flashcardCategories.toast.createError"),
        );
      },
    },
  });

  const updateMutation = usePutApiFlashcardCategoriesIdWithJson({
    mutation: {
      onSuccess: () => {
        invalidateAll();
        setEditingId(null);
        toast.success(t("flashcardCategories.toast.updated"));
      },
      onError: (error) => {
        toast.error(
          error instanceof Error
            ? error.message
            : t("flashcardCategories.toast.updateError"),
        );
      },
    },
  });

  const deleteMutation = useDeleteApiFlashcardCategoriesId({
    mutation: {
      onSuccess: () => {
        invalidateAll();
        setDeleteTarget(null);
        toast.success(t("flashcardCategories.toast.deleted"));
      },
      onError: (error) => {
        toast.error(
          error instanceof Error
            ? error.message
            : t("flashcardCategories.toast.deleteError"),
        );
      },
    },
  });

  const handleCreate = () => {
    if (!newName.trim()) return;
    createMutation.mutate({ data: { name: newName.trim() } });
  };

  const handleUpdate = () => {
    if (editingId === null || !editingName.trim()) return;
    updateMutation.mutate({
      id: editingId,
      data: { name: editingName.trim() },
    });
  };

  const startEditing = (id: number, name: string) => {
    setEditingId(id);
    setEditingName(name);
  };

  const cancelEditing = () => {
    setEditingId(null);
    setEditingName("");
  };

  return (
    <div className="rounded-md border p-4 space-y-4">
      <h3 className="text-lg font-semibold">
        {t("flashcardCategories.title")}
      </h3>

      <div className="flex gap-2">
        <Input
          className="max-w-xs"
          placeholder={t("flashcardCategories.form.namePlaceholder")}
          value={newName}
          onChange={(e) => setNewName(e.target.value)}
          onKeyDown={(e) => e.key === "Enter" && handleCreate()}
        />
        <Button
          size="sm"
          onClick={handleCreate}
          disabled={createMutation.isPending || !newName.trim()}
        >
          <Plus className="mr-2 h-4 w-4" />
          {t("flashcardCategories.addCategory")}
        </Button>
      </div>

      <div className="space-y-1">
        {categories.map((cat) => (
          <div
            key={cat.id}
            className="flex items-center gap-2 py-1.5 px-2 rounded-md hover:bg-muted/50"
          >
            {editingId === cat.id ? (
              <>
                <Input
                  className="h-8 max-w-xs"
                  value={editingName}
                  onChange={(e) => setEditingName(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === "Enter") handleUpdate();
                    if (e.key === "Escape") cancelEditing();
                  }}
                  autoFocus
                />
                <Button
                  variant="ghost"
                  size="icon-xs"
                  onClick={handleUpdate}
                  disabled={updateMutation.isPending}
                >
                  <Check className="h-4 w-4" />
                </Button>
                <Button
                  variant="ghost"
                  size="icon-xs"
                  onClick={cancelEditing}
                >
                  <X className="h-4 w-4" />
                </Button>
              </>
            ) : (
              <>
                <span className="flex-1 text-sm">{cat.name}</span>
                <Button
                  variant="ghost"
                  size="icon-xs"
                  onClick={() => startEditing(cat.id, cat.name)}
                >
                  <Pencil className="h-3.5 w-3.5" />
                </Button>
                <Button
                  variant="ghost"
                  size="icon-xs"
                  onClick={() =>
                    setDeleteTarget({ id: cat.id, name: cat.name })
                  }
                >
                  <Trash2 className="h-3.5 w-3.5" />
                </Button>
              </>
            )}
          </div>
        ))}
        {categories.length === 0 && (
          <p className="text-sm text-muted-foreground py-2">
            {t("flashcardCategories.empty")}
          </p>
        )}
      </div>

      <Dialog
        open={deleteTarget !== null}
        onOpenChange={(open) => !open && setDeleteTarget(null)}
      >
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>
              {t("flashcardCategories.delete.title")}
            </DialogTitle>
            <DialogDescription>
              {t("flashcardCategories.delete.confirmPrefix")}{" "}
              <strong>{deleteTarget?.name}</strong>
              {"? "}
              {t("flashcardCategories.delete.confirmSuffix")}
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setDeleteTarget(null)}
              disabled={deleteMutation.isPending}
            >
              {t("common.cancel")}
            </Button>
            <Button
              variant="destructive"
              onClick={() =>
                deleteTarget && deleteMutation.mutate({ id: deleteTarget.id })
              }
              disabled={deleteMutation.isPending}
            >
              {deleteMutation.isPending
                ? t("common.deleting")
                : t("common.delete")}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
