import { useState } from "react";
import { useGetApiTodos } from "@/api/generated/todos/todos";

const DEFAULT_PAGE_SIZE = 10;

export function useTodosPagination(pageSize = DEFAULT_PAGE_SIZE) {
  const [page, setPage] = useState(1);

  const { data: response, isLoading } = useGetApiTodos({
    page,
    pageSize,
  });

  const pagedResult = response?.data?.data;

  return {
    todos: pagedResult?.items ?? [],
    totalPages: pagedResult?.totalPages ?? 1,
    page,
    setPage,
    isLoading,
  };
}
