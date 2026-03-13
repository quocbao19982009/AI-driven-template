import { useState } from "react";
import { useGetApiExpenseTrackers } from "@/api/generated/expense-trackers/expense-trackers";
import { useAppSelector } from "@/store/hooks";
import { useDebounce } from "@/hooks/use-debounce";

const DEFAULT_PAGE_SIZE = 10;

export function useExpenseTrackersPagination(pageSize = DEFAULT_PAGE_SIZE) {
  const [page, setPage] = useState(1);
  const searchQuery = useAppSelector((s) => s.expenseTrackers.searchQuery);
  const categoryFilter = useAppSelector(
    (s) => s.expenseTrackers.categoryFilter
  );
  const debouncedSearch = useDebounce(searchQuery, 300);

  const { data: response, isLoading } = useGetApiExpenseTrackers({
    page,
    pageSize,
  });

  const pagedResult = response?.data?.data;
  let items = pagedResult?.items ?? [];

  // Client-side filtering for search and category
  if (debouncedSearch) {
    const lowerSearch = debouncedSearch.toLowerCase();
    items = items.filter(
      (e) =>
        e.description?.toLowerCase().includes(lowerSearch) ||
        e.category.toLowerCase().includes(lowerSearch)
    );
  }

  if (categoryFilter && categoryFilter !== "all") {
    items = items.filter((e) => e.category === categoryFilter);
  }

  return {
    expenses: items,
    totalPages: pagedResult?.totalPages ?? 1,
    page,
    setPage,
    isLoading,
  };
}

