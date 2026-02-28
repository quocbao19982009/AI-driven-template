import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { FlashcardDto } from "@/api/generated/models";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Skeleton } from "@/components/ui/skeleton";
import { MoreHorizontal, Pencil, Trash2 } from "lucide-react";
import { FlashcardFormDialog } from "./flashcard-form-dialog";
import { FlashcardDeleteDialog } from "./flashcard-delete-dialog";
import { useFlashcardCategories } from "../hooks";

interface FlashcardsTableProps {
  flashcards: FlashcardDto[];
  isLoading: boolean;
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

export function FlashcardsTable({
  flashcards,
  isLoading,
  page,
  totalPages,
  onPageChange,
}: FlashcardsTableProps) {
  const { t } = useTranslation();
  const [editFlashcard, setEditFlashcard] = useState<FlashcardDto | null>(null);
  const [deleteFlashcard, setDeleteFlashcard] = useState<FlashcardDto | null>(
    null,
  );
  const categories = useFlashcardCategories();

  if (isLoading) {
    return <FlashcardsTableSkeleton t={t} />;
  }

  return (
    <>
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="w-16">
                {t("flashcards.table.id")}
              </TableHead>
              <TableHead>{t("flashcards.table.finnishWord")}</TableHead>
              <TableHead>{t("flashcards.table.englishTranslation")}</TableHead>
              <TableHead>{t("flashcards.table.category")}</TableHead>
              <TableHead className="hidden lg:table-cell">
                {t("flashcards.table.lastReviewed")}
              </TableHead>
              <TableHead className="w-16" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {flashcards.length === 0 ? (
              <TableRow>
                <TableCell
                  colSpan={6}
                  className="text-center text-muted-foreground h-24"
                >
                  {t("flashcards.table.empty")}
                </TableCell>
              </TableRow>
            ) : (
              flashcards.map((flashcard) => (
                <TableRow key={flashcard.id}>
                  <TableCell className="font-mono text-muted-foreground">
                    {flashcard.id}
                  </TableCell>
                  <TableCell className="font-medium">
                    {flashcard.finnishWord}
                  </TableCell>
                  <TableCell>{flashcard.englishTranslation}</TableCell>
                  <TableCell className="text-muted-foreground">
                    {flashcard.categoryName ?? "—"}
                  </TableCell>
                  <TableCell className="hidden lg:table-cell text-muted-foreground">
                    {flashcard.lastReviewedAt
                      ? new Date(flashcard.lastReviewedAt).toLocaleDateString()
                      : "—"}
                  </TableCell>
                  <TableCell>
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button variant="ghost" size="icon-xs">
                          <MoreHorizontal className="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuItem
                          onClick={() => setEditFlashcard(flashcard)}
                        >
                          <Pencil className="mr-2 h-4 w-4" />
                          {t("common.edit")}
                        </DropdownMenuItem>
                        <DropdownMenuItem
                          variant="destructive"
                          onClick={() => setDeleteFlashcard(flashcard)}
                        >
                          <Trash2 className="mr-2 h-4 w-4" />
                          {t("common.delete")}
                        </DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      <div className="flex items-center justify-end gap-2 pt-4">
        <Button
          variant="outline"
          size="sm"
          onClick={() => onPageChange(page - 1)}
          disabled={page <= 1}
        >
          {t("common.previous")}
        </Button>
        <span className="text-sm text-muted-foreground">
          {t("common.page", { page, total: totalPages || 1 })}
        </span>
        <Button
          variant="outline"
          size="sm"
          onClick={() => onPageChange(page + 1)}
          disabled={page >= totalPages}
        >
          {t("common.next")}
        </Button>
      </div>

      <FlashcardFormDialog
        flashcard={editFlashcard}
        open={editFlashcard !== null}
        onOpenChange={(open) => !open && setEditFlashcard(null)}
        categories={categories}
      />

      <FlashcardDeleteDialog
        flashcard={deleteFlashcard}
        open={deleteFlashcard !== null}
        onOpenChange={(open) => !open && setDeleteFlashcard(null)}
      />
    </>
  );
}

function FlashcardsTableSkeleton({ t }: { t: (key: string) => string }) {
  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead className="w-16">{t("flashcards.table.id")}</TableHead>
            <TableHead>{t("flashcards.table.finnishWord")}</TableHead>
            <TableHead>{t("flashcards.table.englishTranslation")}</TableHead>
            <TableHead>{t("flashcards.table.category")}</TableHead>
            <TableHead className="hidden lg:table-cell">
              {t("flashcards.table.lastReviewed")}
            </TableHead>
            <TableHead className="w-16" />
          </TableRow>
        </TableHeader>
        <TableBody>
          {Array.from({ length: 5 }).map((_, i) => (
            <TableRow key={i}>
              <TableCell>
                <Skeleton className="h-4 w-8" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-4 w-32" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-4 w-32" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-4 w-24" />
              </TableCell>
              <TableCell className="hidden lg:table-cell">
                <Skeleton className="h-4 w-24" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-4 w-4" />
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
