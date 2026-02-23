import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Plus } from "lucide-react";
import { useAppDispatch, useAppSelector } from "@/store/hooks";
import {
  setSearchQuery,
  setStatusFilter,
  setPriorityFilter,
} from "../store";
import { TodosTable } from "./todos-table";
import { TodoFormDialog } from "./todo-form-dialog";
import { useTodoPagination } from "../hooks";

export function TodosPage() {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const [createOpen, setCreateOpen] = useState(false);

  const searchQuery = useAppSelector((s) => s.todos.searchQuery);
  const statusFilter = useAppSelector((s) => s.todos.statusFilter);
  const priorityFilter = useAppSelector((s) => s.todos.priorityFilter);

  const { todos, isLoading, page, totalPages, setPage } = useTodoPagination();

  const statusOptions = ["all", "active", "completed"] as const;

  const prioritySelectOptions = [
    { value: "all", label: t("todos.filter.priority.all") },
    { value: "0", label: t("todos.filter.priority.low") },
    { value: "1", label: t("todos.filter.priority.medium") },
    { value: "2", label: t("todos.filter.priority.high") },
  ];

  return (
    <div className="p-6 space-y-6">
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

      <div className="flex flex-wrap items-center gap-3">
        <Input
          className="max-w-sm"
          placeholder={t("todos.searchPlaceholder")}
          value={searchQuery}
          onChange={(e) => dispatch(setSearchQuery(e.target.value))}
        />

        <div className="flex gap-1 border rounded-md p-1">
          {statusOptions.map((status) => (
            <Button
              key={status}
              size="sm"
              variant={statusFilter === status ? "default" : "ghost"}
              onClick={() => dispatch(setStatusFilter(status))}
            >
              {t(`todos.filter.status.${status}`)}
            </Button>
          ))}
        </div>

        <Select
          value={priorityFilter === null ? "all" : String(priorityFilter)}
          onValueChange={(v) =>
            dispatch(setPriorityFilter(v === "all" ? null : Number(v)))
          }
        >
          <SelectTrigger className="w-40">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {prioritySelectOptions.map((opt) => (
              <SelectItem key={opt.value} value={opt.value}>
                {opt.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
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
