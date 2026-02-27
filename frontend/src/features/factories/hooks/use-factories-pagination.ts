import { useState, useEffect } from "react";
import { useGetApiFactories } from "@/api/generated/factories/factories";
import { useAppSelector } from "@/store/hooks";
import { useDebounce } from "@/hooks";

const DEFAULT_PAGE_SIZE = 10;

export function useFactoriesPagination(pageSize = DEFAULT_PAGE_SIZE) {
  const [page, setPage] = useState(1);

  const searchQuery = useAppSelector((s) => s.factories.searchQuery);
  const debouncedSearch = useDebounce(searchQuery, 400);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setPage(1);
  }, [debouncedSearch]);

  const { data: response, isLoading } = useGetApiFactories({
    page,
    pageSize,
    search: debouncedSearch || undefined,
  });

  const pagedResult = response?.data?.data;

  return {
    factories: pagedResult?.items ?? [],
    totalPages: pagedResult?.totalPages ?? 1,
    page,
    setPage,
    isLoading,
  };
}
