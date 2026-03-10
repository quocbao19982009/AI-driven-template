import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Plus } from "lucide-react";
import { useAppDispatch, useAppSelector } from "@/store/hooks";
import { setSearchQuery } from "../store";
import { useDebounce } from "@/hooks/use-debounce";
import { TodosTable } from "./todos-table";
import { TodoFormDialog } from "./todo-form-dialog";
import { useTodosPagination } from "../hooks";

export function TodosPage() {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const searchQuery = useAppSelector((state) => state.todos.searchQuery);
  const [createOpen, setCreateOpen] = useState(false);

  const _debouncedSearch = useDebounce(searchQuery, 300);

  const { todos, isLoading, page, totalPages, setPage } = useTodosPagination();

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">
            {t("todos.title")}
          </h1>
          <p className="text-muted-foreground">{t("todos.description")}</p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          {t("todos.newTodo")}
        </Button>
      </div>

      <div>
        <Input
          placeholder={t("todos.search.placeholder")}
          value={searchQuery}
          onChange={(e) => dispatch(setSearchQuery(e.target.value))}
          className="max-w-sm"
        />
      </div>

      <TodosTable
        todos={todos}
        isLoading={isLoading}
        page={page}
        totalPages={totalPages}
        onPageChange={setPage}
      />

      <TodoFormDialog open={createOpen} onOpenChange={setCreateOpen} />
    </div>
  );
}
