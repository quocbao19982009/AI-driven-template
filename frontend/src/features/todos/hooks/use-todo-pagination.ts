import { useState, useEffect } from "react";
import { useGetApiTodos } from "@/api/generated/todos/todos";
import { useAppSelector } from "@/store/hooks";
import { useDebounce } from "@/hooks";

const DEFAULT_PAGE_SIZE = 10;

export function useTodoPagination(pageSize = DEFAULT_PAGE_SIZE) {
  const [page, setPage] = useState(1);

  const searchQuery = useAppSelector((s) => s.todos.searchQuery);
  const statusFilter = useAppSelector((s) => s.todos.statusFilter);
  const priorityFilter = useAppSelector((s) => s.todos.priorityFilter);

  const debouncedSearch = useDebounce(searchQuery, 400);

  const isCompleted: boolean | undefined =
    statusFilter === "active"
      ? false
      : statusFilter === "completed"
        ? true
        : undefined;

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setPage(1);
  }, [debouncedSearch, isCompleted, priorityFilter]);

  const { data: response, isLoading } = useGetApiTodos({
    page,
    pageSize,
    search: debouncedSearch || undefined,
    isCompleted,
    priority: priorityFilter ?? undefined,
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
