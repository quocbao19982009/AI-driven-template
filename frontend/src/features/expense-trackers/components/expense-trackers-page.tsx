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
import { Plus, Search } from "lucide-react";
import { ExpenseTrackersTable } from "./expense-trackers-table";
import { ExpenseTrackerFormDialog } from "./expense-tracker-form-dialog";
import { useExpenseTrackersPagination } from "../hooks/use-expense-trackers-pagination";
import { useAppDispatch, useAppSelector } from "@/store/hooks";
import {
  setSearchQuery,
  setCategoryFilter,
} from "../store/expense-trackers-slice";
import { useAuth } from "@/auth/auth-context";

const CATEGORIES = [
  "all",
  "Food",
  "Transport",
  "Utilities",
  "Entertainment",
  "Other",
] as const;

export function ExpenseTrackersPage() {
  const { t } = useTranslation();
  const { accessToken } = useAuth();
  const dispatch = useAppDispatch();
  const searchQuery = useAppSelector((s) => s.expenseTrackers.searchQuery);
  const categoryFilter = useAppSelector(
    (s) => s.expenseTrackers.categoryFilter
  );
  const [createOpen, setCreateOpen] = useState(false);
  const { expenses, isLoading, page, totalPages, setPage } =
    useExpenseTrackersPagination();

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">
            {t("expenseTrackers.title")}
          </h1>
          <p className="text-muted-foreground">
            {t("expenseTrackers.description")}
          </p>
        </div>
        {accessToken && (
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="mr-2 h-4 w-4" />
            {t("expenseTrackers.newExpense")}
          </Button>
        )}
      </div>

      <div className="flex items-center gap-4">
        <div className="relative max-w-sm flex-1">
          <Search className="text-muted-foreground absolute top-2.5 left-3 h-4 w-4" />
          <Input
            placeholder={t("expenseTrackers.searchPlaceholder")}
            value={searchQuery}
            onChange={(e) => dispatch(setSearchQuery(e.target.value))}
            className="pl-9"
          />
        </div>
        <Select
          value={categoryFilter}
          onValueChange={(value) => dispatch(setCategoryFilter(value))}
        >
          <SelectTrigger className="w-[180px]">
            <SelectValue placeholder={t("expenseTrackers.filter.category")} />
          </SelectTrigger>
          <SelectContent>
            {CATEGORIES.map((cat) => (
              <SelectItem key={cat} value={cat}>
                {cat === "all"
                  ? t("expenseTrackers.filter.all")
                  : t(`expenseTrackers.filter.${cat.toLowerCase()}`)}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <ExpenseTrackersTable
        expenses={expenses}
        isLoading={isLoading}
        page={page}
        totalPages={totalPages}
        onPageChange={setPage}
      />

      <ExpenseTrackerFormDialog
        open={createOpen}
        onOpenChange={setCreateOpen}
      />
    </div>
  );
}

