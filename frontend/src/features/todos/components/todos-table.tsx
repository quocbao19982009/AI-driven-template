import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useQueryClient } from "@tanstack/react-query";
import type { TodoDto } from "@/api/generated/models";
import {
  usePatchApiTodosIdToggle,
  getGetApiTodosQueryKey,
} from "@/api/generated/todos/todos";
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
import { MoreHorizontal, Pencil, Trash2, CheckCircle2 } from "lucide-react";
import { cn } from "@/lib/utils";
import { TodoFormDialog } from "./todo-form-dialog";
import { TodoDeleteDialog } from "./todo-delete-dialog";
import { toast } from "sonner";

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
  const [editTodo, setEditTodo] = useState<TodoDto | null>(null);
  const [deleteTodo, setDeleteTodo] = useState<TodoDto | null>(null);

  if (isLoading) {
    return <TodosTableSkeleton t={t} />;
  }

  return (
    <>
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="w-16">{t("todos.table.id")}</TableHead>
              <TableHead>{t("todos.table.title")}</TableHead>
              <TableHead className="hidden md:table-cell">
                {t("todos.table.description")}
              </TableHead>
              <TableHead>{t("todos.table.priority")}</TableHead>
              <TableHead className="hidden sm:table-cell">
                {t("todos.table.dueDate")}
              </TableHead>
              <TableHead>{t("todos.table.status")}</TableHead>
              <TableHead className="hidden lg:table-cell">
                {t("todos.table.createdAt")}
              </TableHead>
              <TableHead className="w-16" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {todos.length === 0 ? (
              <TableRow>
                <TableCell
                  colSpan={8}
                  className="text-center text-muted-foreground h-24"
                >
                  {t("todos.table.empty")}
                </TableCell>
              </TableRow>
            ) : (
              todos.map((todo) => (
                <TodoRow
                  key={todo.id}
                  todo={todo}
                  onEdit={() => setEditTodo(todo)}
                  onDelete={() => setDeleteTodo(todo)}
                  t={t}
                />
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

interface TodoRowProps {
  todo: TodoDto;
  onEdit: () => void;
  onDelete: () => void;
  t: (key: string, opts?: Record<string, unknown>) => string;
}

function TodoRow({ todo, onEdit, onDelete, t }: TodoRowProps) {
  const queryClient = useQueryClient();

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

  return (
    <TableRow className={cn(todo.isCompleted && "opacity-60")}>
      <TableCell className="font-mono text-muted-foreground">{todo.id}</TableCell>
      <TableCell className="font-medium max-w-[200px] truncate">
        {todo.title}
      </TableCell>
      <TableCell className="hidden md:table-cell max-w-[250px] truncate text-muted-foreground">
        {todo.description
          ? todo.description.length > 60
            ? `${todo.description.slice(0, 60)}…`
            : todo.description
          : "—"}
      </TableCell>
      <TableCell>
        <PriorityBadge priority={todo.priority} t={t} />
      </TableCell>
      <TableCell className="hidden sm:table-cell text-muted-foreground">
        {todo.dueDate
          ? new Date(todo.dueDate).toLocaleDateString()
          : "—"}
      </TableCell>
      <TableCell>
        <StatusBadge isCompleted={todo.isCompleted} t={t} />
      </TableCell>
      <TableCell className="hidden lg:table-cell text-muted-foreground">
        {todo.createdAt ? new Date(todo.createdAt).toLocaleDateString() : "—"}
      </TableCell>
      <TableCell>
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="icon-xs">
              <MoreHorizontal className="h-4 w-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuItem onClick={onEdit}>
              <Pencil className="mr-2 h-4 w-4" />
              {t("common.edit")}
            </DropdownMenuItem>
            <DropdownMenuItem
              onClick={() => toggleMutation.mutate({ id: todo.id })}
              disabled={toggleMutation.isPending}
            >
              <CheckCircle2 className="mr-2 h-4 w-4" />
              {todo.isCompleted
                ? t("todos.actions.markActive")
                : t("todos.actions.markDone")}
            </DropdownMenuItem>
            <DropdownMenuItem variant="destructive" onClick={onDelete}>
              <Trash2 className="mr-2 h-4 w-4" />
              {t("common.delete")}
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </TableCell>
    </TableRow>
  );
}

function PriorityBadge({
  priority,
  t,
}: {
  priority: number;
  t: (key: string) => string;
}) {
  const configs: Record<number, { label: string; className: string }> = {
    0: {
      label: t("todos.priority.low"),
      className: "bg-gray-100 text-gray-700 hover:bg-gray-100",
    },
    1: {
      label: t("todos.priority.medium"),
      className: "bg-yellow-100 text-yellow-700 hover:bg-yellow-100",
    },
    2: {
      label: t("todos.priority.high"),
      className: "bg-red-100 text-red-700 hover:bg-red-100",
    },
  };
  const config = configs[priority] ?? {
    label: "—",
    className: "",
  };
  return <Badge className={cn(config.className)}>{config.label}</Badge>;
}

function StatusBadge({
  isCompleted,
  t,
}: {
  isCompleted: boolean;
  t: (key: string) => string;
}) {
  return isCompleted ? (
    <Badge className="bg-green-100 text-green-700 hover:bg-green-100">
      {t("todos.filter.status.completed")}
    </Badge>
  ) : (
    <Badge className="bg-blue-100 text-blue-700 hover:bg-blue-100">
      {t("todos.filter.status.active")}
    </Badge>
  );
}

function TodosTableSkeleton({ t }: { t: (key: string) => string }) {
  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead className="w-16">{t("todos.table.id")}</TableHead>
            <TableHead>{t("todos.table.title")}</TableHead>
            <TableHead className="hidden md:table-cell">{t("todos.table.description")}</TableHead>
            <TableHead>{t("todos.table.priority")}</TableHead>
            <TableHead className="hidden sm:table-cell">{t("todos.table.dueDate")}</TableHead>
            <TableHead>{t("todos.table.status")}</TableHead>
            <TableHead className="hidden lg:table-cell">{t("todos.table.createdAt")}</TableHead>
            <TableHead className="w-16" />
          </TableRow>
        </TableHeader>
        <TableBody>
          {Array.from({ length: 5 }).map((_, i) => (
            <TableRow key={i}>
              <TableCell>
                <Skeleton className="h-4 w-8" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-4 w-40" />
              </TableCell>
              <TableCell className="hidden md:table-cell">
                <Skeleton className="h-4 w-56" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-4 w-16" />
              </TableCell>
              <TableCell className="hidden sm:table-cell">
                <Skeleton className="h-4 w-20" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-4 w-20" />
              </TableCell>
              <TableCell className="hidden lg:table-cell">
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
