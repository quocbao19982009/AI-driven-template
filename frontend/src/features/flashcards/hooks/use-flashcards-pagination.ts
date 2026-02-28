import { useState, useEffect } from "react";
import { useGetApiFlashcards } from "@/api/generated/flashcards/flashcards";
import { useAppSelector } from "@/store/hooks";
import { useDebounce } from "@/hooks";

const DEFAULT_PAGE_SIZE = 10;

export function useFlashcardsPagination(pageSize = DEFAULT_PAGE_SIZE) {
  const [page, setPage] = useState(1);

  const searchQuery = useAppSelector((s) => s.flashcards.searchQuery);
  const categoryFilter = useAppSelector(
    (s) => s.flashcards.activeCategoryFilter,
  );
  const debouncedSearch = useDebounce(searchQuery, 400);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setPage(1);
  }, [debouncedSearch, categoryFilter]);

  const { data: response, isLoading } = useGetApiFlashcards({
    page,
    pageSize,
    search: debouncedSearch || undefined,
    category: categoryFilter || undefined,
  });

  const pagedResult = response?.data?.data;

  return {
    flashcards: pagedResult?.items ?? [],
    totalPages: pagedResult?.totalPages ?? 1,
    page,
    setPage,
    isLoading,
  };
}
