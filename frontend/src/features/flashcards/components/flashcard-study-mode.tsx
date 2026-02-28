import { useState, useCallback } from "react";
import { useTranslation } from "react-i18next";
import { useQueryClient } from "@tanstack/react-query";
import { Button } from "@/components/ui/button";
import {
  useGetApiFlashcards,
  usePatchApiFlashcardsIdReview,
  getGetApiFlashcardsQueryKey,
} from "@/api/generated/flashcards/flashcards";
import { useAppDispatch, useAppSelector } from "@/store/hooks";
import { setStudyCategory } from "../store";
import { ChevronLeft, ChevronRight, RotateCcw } from "lucide-react";
import { toast } from "sonner";

interface FlashcardStudyModeProps {
  categories: string[];
}

export function FlashcardStudyMode({ categories }: FlashcardStudyModeProps) {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const queryClient = useQueryClient();
  const studyCategory = useAppSelector((s) => s.flashcards.studyCategory);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [isFlipped, setIsFlipped] = useState(false);

  const { data: response, isLoading } = useGetApiFlashcards(
    {
      page: 1,
      pageSize: 100,
      category: studyCategory || undefined,
    },
    { query: { enabled: !!studyCategory } },
  );

  const cards = response?.data?.data?.items ?? [];

  const reviewMutation = usePatchApiFlashcardsIdReview({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiFlashcardsQueryKey(),
        });
        toast.success(t("flashcards.toast.reviewed"));
      },
      onError: (error) => {
        toast.error(
          error instanceof Error
            ? error.message
            : t("flashcards.toast.reviewError"),
        );
      },
    },
  });

  const goNext = useCallback(() => {
    setIsFlipped(false);
    setCurrentIndex((prev) => Math.min(prev + 1, cards.length - 1));
  }, [cards.length]);

  const goPrev = useCallback(() => {
    setIsFlipped(false);
    setCurrentIndex((prev) => Math.max(prev - 1, 0));
  }, []);

  const handleCategoryChange = (value: string) => {
    dispatch(setStudyCategory(value));
    setCurrentIndex(0);
    setIsFlipped(false);
  };

  const currentCard = cards[currentIndex];

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <select
          className="h-9 rounded-md border border-input bg-background px-3 text-sm"
          value={studyCategory}
          onChange={(e) => handleCategoryChange(e.target.value)}
        >
          <option value="">{t("flashcards.study.selectCategory")}</option>
          {categories.map((cat) => (
            <option key={cat} value={cat}>
              {cat}
            </option>
          ))}
        </select>
      </div>

      {!studyCategory && (
        <p className="text-muted-foreground text-center py-12">
          {t("flashcards.study.selectCategoryPrompt")}
        </p>
      )}

      {studyCategory && isLoading && (
        <p className="text-muted-foreground text-center py-12">
          {t("flashcards.study.loading")}
        </p>
      )}

      {studyCategory && !isLoading && cards.length === 0 && (
        <p className="text-muted-foreground text-center py-12">
          {t("flashcards.study.noCards")}
        </p>
      )}

      {studyCategory && !isLoading && cards.length > 0 && currentCard && (
        <div className="flex flex-col items-center space-y-6">
          <button
            type="button"
            onClick={() => setIsFlipped(!isFlipped)}
            className="w-full max-w-md h-64 rounded-xl border-2 border-border bg-card shadow-md hover:shadow-lg transition-shadow cursor-pointer flex flex-col items-center justify-center p-8"
          >
            <p className="text-xs uppercase tracking-wide text-muted-foreground mb-2">
              {isFlipped
                ? t("flashcards.study.english")
                : t("flashcards.study.finnish")}
            </p>
            <p className="text-3xl font-bold text-center">
              {isFlipped
                ? currentCard.englishTranslation
                : currentCard.finnishWord}
            </p>
            <p className="text-xs text-muted-foreground mt-4">
              {t("flashcards.study.clickToFlip")}
            </p>
          </button>

          <div className="flex items-center gap-4">
            <Button
              variant="outline"
              size="icon"
              onClick={goPrev}
              disabled={currentIndex === 0}
            >
              <ChevronLeft className="h-4 w-4" />
            </Button>

            <span className="text-sm text-muted-foreground min-w-[80px] text-center">
              {currentIndex + 1} / {cards.length}
            </span>

            <Button
              variant="outline"
              size="icon"
              onClick={goNext}
              disabled={currentIndex >= cards.length - 1}
            >
              <ChevronRight className="h-4 w-4" />
            </Button>
          </div>

          <div className="flex gap-2">
            <Button
              variant="outline"
              size="sm"
              onClick={() => {
                setCurrentIndex(0);
                setIsFlipped(false);
              }}
            >
              <RotateCcw className="mr-2 h-4 w-4" />
              {t("flashcards.study.restart")}
            </Button>
            <Button
              variant="secondary"
              size="sm"
              onClick={() =>
                reviewMutation.mutate({ id: currentCard.id })
              }
              disabled={reviewMutation.isPending}
            >
              {t("flashcards.study.markReviewed")}
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}
