import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { ExpenseTrackerDto } from "@/api/generated/models";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Skeleton } from "@/components/ui/skeleton";
import { MoreHorizontal, Pencil, Trash2 } from "lucide-react";
import { ExpenseTrackerFormDialog } from "./expense-tracker-form-dialog";
import { ExpenseTrackerDeleteDialog } from "./expense-tracker-delete-dialog";
import { useAuth } from "@/auth/auth-context";
import { useAppSelector } from "@/store/hooks";

interface ExpenseTrackersTableProps {
  expenses: ExpenseTrackerDto[];
  isLoading: boolean;
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

export function ExpenseTrackersTable({
  expenses,
  isLoading,
  page,
  totalPages,
  onPageChange,
}: ExpenseTrackersTableProps) {
  const { t } = useTranslation();
  const { accessToken } = useAuth();
  const user = useAppSelector((s) => s.auth.user);
  const [editExpense, setEditExpense] = useState<ExpenseTrackerDto | null>(
    null
  );
  const [deleteExpense, setDeleteExpense] = useState<ExpenseTrackerDto | null>(
    null
  );

  const canModify = (expense: ExpenseTrackerDto) => {
    if (!accessToken || !user) return false;
    return expense.userId === user.id || user.role === "Admin";
  };

  if (isLoading) {
    return <ExpenseTrackersTableSkeleton />;
  }

  return (
    <>
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>{t("expenseTrackers.table.amount")}</TableHead>
              <TableHead>{t("expenseTrackers.table.category")}</TableHead>
              <TableHead>{t("expenseTrackers.table.description")}</TableHead>
              <TableHead>{t("expenseTrackers.table.date")}</TableHead>
              <TableHead>{t("expenseTrackers.table.submittedBy")}</TableHead>
              {accessToken && <TableHead className="w-16" />}
            </TableRow>
          </TableHeader>
          <TableBody>
            {expenses.length === 0 ? (
              <TableRow>
                <TableCell
                  colSpan={accessToken ? 6 : 5}
                  className="text-muted-foreground h-24 text-center"
                >
                  {t("expenseTrackers.table.empty")}
                </TableCell>
              </TableRow>
            ) : (
              expenses.map((expense) => (
                <TableRow key={expense.id}>
                  <TableCell className="font-mono">
                    {new Intl.NumberFormat(undefined, {
                      style: "currency",
                      currency: "EUR",
                    }).format(expense.amount)}
                  </TableCell>
                  <TableCell>{expense.category}</TableCell>
                  <TableCell className="max-w-[200px] truncate">
                    {expense.description ?? "—"}
                  </TableCell>
                  <TableCell>
                    {new Date(expense.date).toLocaleDateString()}
                  </TableCell>
                  <TableCell>{expense.submittedBy}</TableCell>
                  {accessToken && (
                    <TableCell>
                      {canModify(expense) && (
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" size="icon-xs">
                              <MoreHorizontal className="h-4 w-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuItem
                              onClick={() => setEditExpense(expense)}
                            >
                              <Pencil className="mr-2 h-4 w-4" />
                              {t("common.edit")}
                            </DropdownMenuItem>
                            <DropdownMenuItem
                              variant="destructive"
                              onClick={() => setDeleteExpense(expense)}
                            >
                              <Trash2 className="mr-2 h-4 w-4" />
                              {t("common.delete")}
                            </DropdownMenuItem>
                          </DropdownMenuContent>
                        </DropdownMenu>
                      )}
                    </TableCell>
                  )}
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
        <span className="text-muted-foreground text-sm">
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

      <ExpenseTrackerFormDialog
        expense={editExpense}
        open={editExpense !== null}
        onOpenChange={(open) => !open && setEditExpense(null)}
      />

      <ExpenseTrackerDeleteDialog
        expense={deleteExpense}
        open={deleteExpense !== null}
        onOpenChange={(open) => !open && setDeleteExpense(null)}
      />
    </>
  );
}

function ExpenseTrackersTableSkeleton() {
  const { t } = useTranslation();
  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>{t("expenseTrackers.table.amount")}</TableHead>
            <TableHead>{t("expenseTrackers.table.category")}</TableHead>
            <TableHead>{t("expenseTrackers.table.description")}</TableHead>
            <TableHead>{t("expenseTrackers.table.date")}</TableHead>
            <TableHead>{t("expenseTrackers.table.submittedBy")}</TableHead>
            <TableHead className="w-16" />
          </TableRow>
        </TableHeader>
        <TableBody>
          {Array.from({ length: 5 }).map((_, i) => (
            <TableRow key={i}>
              <TableCell>
                <Skeleton className="h-4 w-16" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-4 w-20" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-4 w-32" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-4 w-24" />
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

