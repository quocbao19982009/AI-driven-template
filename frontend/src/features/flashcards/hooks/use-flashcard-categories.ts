import { useMemo } from "react";
import { useGetApiFlashcards } from "@/api/generated/flashcards/flashcards";

export function useFlashcardCategories() {
  const { data: response } = useGetApiFlashcards({
    page: 1,
    pageSize: 100,
  });

  const categories = useMemo(() => {
    const items = response?.data?.data?.items ?? [];
    const unique = [...new Set(items.map((item) => item.category))];
    return unique.filter(Boolean).sort();
  }, [response]);

  return categories;
}
