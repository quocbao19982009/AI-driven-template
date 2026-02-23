import { useState } from "react";
import { useGetApiFeatures } from "@/api/generated/feature/feature";

const DEFAULT_PAGE_SIZE = 10;

export function useFeaturePagination(pageSize = DEFAULT_PAGE_SIZE) {
  const [page, setPage] = useState(1);

  const { data: response, isLoading } = useGetApiFeatures({
    page,
    pageSize,
  });

  const pagedResult = response?.data?.data;

  return {
    features: pagedResult?.items ?? [],
    totalPages: pagedResult?.totalPages ?? 1,
    page,
    setPage,
    isLoading,
  };
}
