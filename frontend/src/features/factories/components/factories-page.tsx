import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Plus } from "lucide-react";
import { useAppDispatch, useAppSelector } from "@/store/hooks";
import { setSearchQuery } from "../store";
import { FactoriesTable } from "./factories-table";
import { FactoryFormDialog } from "./factory-form-dialog";
import { useFactoriesPagination } from "../hooks";

export function FactoriesPage() {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const [createOpen, setCreateOpen] = useState(false);

  const searchQuery = useAppSelector((s) => s.factories.searchQuery);
  const { factories, isLoading, page, totalPages, setPage } = useFactoriesPagination();

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">{t("factories.title")}</h1>
          <p className="text-muted-foreground">{t("factories.description")}</p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          {t("factories.newFactory")}
        </Button>
      </div>

      <div className="flex flex-wrap items-center gap-3">
        <Input
          className="max-w-sm"
          placeholder={t("factories.searchPlaceholder")}
          value={searchQuery}
          onChange={(e) => dispatch(setSearchQuery(e.target.value))}
        />
      </div>

      <FactoriesTable
        factories={factories}
        isLoading={isLoading}
        page={page}
        totalPages={totalPages}
        onPageChange={setPage}
      />

      <FactoryFormDialog open={createOpen} onOpenChange={setCreateOpen} />
    </div>
  );
}
