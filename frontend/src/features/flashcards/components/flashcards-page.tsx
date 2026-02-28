import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Plus } from "lucide-react";
import { useAppDispatch, useAppSelector } from "@/store/hooks";
import {
  setSearchQuery,
  setActiveCategoryFilter,
  setActiveTab,
} from "../store";
import { FlashcardsTable } from "./flashcards-table";
import { FlashcardFormDialog } from "./flashcard-form-dialog";
import { FlashcardStudyMode } from "./flashcard-study-mode";
import { useFlashcardsPagination, useFlashcardCategories } from "../hooks";

export function FlashcardsPage() {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const [createOpen, setCreateOpen] = useState(false);

  const searchQuery = useAppSelector((s) => s.flashcards.searchQuery);
  const activeTab = useAppSelector((s) => s.flashcards.activeTab);
  const categoryFilter = useAppSelector(
    (s) => s.flashcards.activeCategoryFilter,
  );
  const { flashcards, isLoading, page, totalPages, setPage } =
    useFlashcardsPagination();
  const categories = useFlashcardCategories();

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">
            {t("flashcards.title")}
          </h1>
          <p className="text-muted-foreground">
            {t("flashcards.description")}
          </p>
        </div>
      </div>

      <div className="flex gap-2">
        <Button
          variant={activeTab === "manage" ? "default" : "outline"}
          onClick={() => dispatch(setActiveTab("manage"))}
        >
          {t("flashcards.tabs.manage")}
        </Button>
        <Button
          variant={activeTab === "study" ? "default" : "outline"}
          onClick={() => dispatch(setActiveTab("study"))}
        >
          {t("flashcards.tabs.study")}
        </Button>
      </div>

      {activeTab === "manage" ? (
        <>
          <div className="flex items-center justify-between">
            <div className="flex flex-wrap items-center gap-3">
              <Input
                className="max-w-sm"
                placeholder={t("flashcards.searchPlaceholder")}
                value={searchQuery}
                onChange={(e) => dispatch(setSearchQuery(e.target.value))}
              />
              <select
                className="h-9 rounded-md border border-input bg-background px-3 text-sm"
                value={categoryFilter}
                onChange={(e) =>
                  dispatch(setActiveCategoryFilter(e.target.value))
                }
              >
                <option value="">{t("flashcards.filter.allCategories")}</option>
                {categories.map((cat) => (
                  <option key={cat} value={cat}>
                    {cat}
                  </option>
                ))}
              </select>
            </div>
            <Button onClick={() => setCreateOpen(true)}>
              <Plus className="mr-2 h-4 w-4" />
              {t("flashcards.newFlashcard")}
            </Button>
          </div>

          <FlashcardsTable
            flashcards={flashcards}
            isLoading={isLoading}
            page={page}
            totalPages={totalPages}
            onPageChange={setPage}
          />

          <FlashcardFormDialog
            open={createOpen}
            onOpenChange={setCreateOpen}
            categories={categories}
          />
        </>
      ) : (
        <FlashcardStudyMode categories={categories} />
      )}
    </div>
  );
}
