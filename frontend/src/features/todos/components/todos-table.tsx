import { useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Skeleton } from "@/components/ui/skeleton";
import { MoreHorizontal, Pencil, Trash2, ToggleLeft } from "lucide-react";
import { toast } from "sonner";
import {
  usePatchApiTodosIdToggle,
  getGetApiTodosQueryKey,
} from "@/api/generated/todos/todos";
import { TodoFormDialog } from "./todo-form-dialog";
import { TodoDeleteDialog } from "./todo-delete-dialog";
import type { TodoDto } from "../hooks";

interface TodosTableProps {
  todos: TodoDto[];
  isLoading: boolean;
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

export function TodosTable({
  todos,
  isLoading,
  page,
  totalPages,
  onPageChange,
}: TodosTableProps) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [editTodo, setEditTodo] = useState<TodoDto | null>(null);
  const [deleteTodo, setDeleteTodo] = useState<TodoDto | null>(null);

  const toggleMutation = usePatchApiTodosIdToggle({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiTodosQueryKey() });
        toast.success(t("todos.toast.toggled"));
      },
      onError: (error) => {
        toast.error(
          error instanceof Error ? error.message : t("todos.toast.toggleError"),
        );
      },
    },
  });

  if (isLoading) {
    return <TodosTableSkeleton />;
  }

  return (
    <>
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>{t("todos.table.title")}</TableHead>
              <TableHead>{t("todos.table.description")}</TableHead>
              <TableHead>{t("todos.table.dueDate")}</TableHead>
              <TableHead>{t("todos.table.status")}</TableHead>
              <TableHead>{t("todos.table.createdAt")}</TableHead>
              <TableHead className="w-16" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {todos.length === 0 ? (
              <TableRow>
                <TableCell
                  colSpan={6}
                  className="text-center text-muted-foreground h-24"
                >
                  {t("todos.table.empty")}
                </TableCell>
              </TableRow>
            ) : (
              todos.map((todo) => (
                <TableRow key={todo.id}>
                  <TableCell className="font-medium">{todo.title}</TableCell>
                  <TableCell className="max-w-xs truncate text-muted-foreground">
                    {todo.description ?? "—"}
                  </TableCell>
                  <TableCell>
                    {todo.dueDate
                      ? new Date(todo.dueDate).toLocaleDateString()
                      : "—"}
                  </TableCell>
                  <TableCell>
                    {todo.isCompleted ? (
                      <Badge variant="default">{t("todos.status.completed")}</Badge>
                    ) : (
                      <Badge variant="secondary">{t("todos.status.pending")}</Badge>
                    )}
                  </TableCell>
                  <TableCell>
                    {todo.createdAt
                      ? new Date(todo.createdAt).toLocaleDateString()
                      : "—"}
                  </TableCell>
                  <TableCell>
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button variant="ghost" size="icon-xs">
                          <MoreHorizontal className="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuItem onClick={() => setEditTodo(todo)}>
                          <Pencil className="mr-2 h-4 w-4" />
                          {t("common.edit")}
                        </DropdownMenuItem>
                        <DropdownMenuItem
                          onClick={() =>
                            toggleMutation.mutate({ id: todo.id })
                          }
                        >
                          <ToggleLeft className="mr-2 h-4 w-4" />
                          {t("todos.actions.toggleComplete")}
                        </DropdownMenuItem>
                        <DropdownMenuItem
                          variant="destructive"
                          onClick={() => setDeleteTodo(todo)}
                        >
                          <Trash2 className="mr-2 h-4 w-4" />
                          {t("common.delete")}
                        </DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      <div className="flex items-center justify-end gap-2 pt-4">
        <Button
          variant="outline"
          size="sm"
          onClick={() => onPageChange(page - 1)}
          disabled={page <= 1}
        >
          {t("common.previous")}
        </Button>
        <span className="text-sm text-muted-foreground">
          {t("common.page", { page, total: totalPages || 1 })}
        </span>
        <Button
          variant="outline"
          size="sm"
          onClick={() => onPageChange(page + 1)}
          disabled={page >= totalPages}
        >
          {t("common.next")}
        </Button>
      </div>

      <TodoFormDialog
        todo={editTodo}
        open={editTodo !== null}
        onOpenChange={(open) => !open && setEditTodo(null)}
      />

      <TodoDeleteDialog
        todo={deleteTodo}
        open={deleteTodo !== null}
        onOpenChange={(open) => !open && setDeleteTodo(null)}
      />
    </>
  );
}

function TodosTableSkeleton() {
  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Title</TableHead>
            <TableHead>Description</TableHead>
            <TableHead>Due Date</TableHead>
            <TableHead>Status</TableHead>
            <TableHead>Created At</TableHead>
            <TableHead className="w-16" />
          </TableRow>
        </TableHeader>
        <TableBody>
          {Array.from({ length: 5 }).map((_, i) => (
            <TableRow key={i}>
              <TableCell>
                <Skeleton className="h-4 w-40" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-4 w-48" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-4 w-24" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-4 w-20" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-4 w-24" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-4 w-4" />
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
