import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Plus } from "lucide-react";
import { useAppDispatch, useAppSelector } from "@/store/hooks";
import { setSearchQuery } from "../store";
import { PersonnelTable } from "./personnel-table";
import { PersonFormDialog } from "./person-form-dialog";
import { usePersonnelPagination } from "../hooks";

export function PersonnelPage() {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const [createOpen, setCreateOpen] = useState(false);

  const searchQuery = useAppSelector((s) => s.personnel.searchQuery);
  const { personnel, isLoading, page, totalPages, setPage } = usePersonnelPagination();

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">{t("personnel.title")}</h1>
          <p className="text-muted-foreground">{t("personnel.description")}</p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          {t("personnel.newPerson")}
        </Button>
      </div>

      <div className="flex flex-wrap items-center gap-3">
        <Input
          className="max-w-sm"
          placeholder={t("personnel.searchPlaceholder")}
          value={searchQuery}
          onChange={(e) => dispatch(setSearchQuery(e.target.value))}
        />
      </div>

      <PersonnelTable
        personnel={personnel}
        isLoading={isLoading}
        page={page}
        totalPages={totalPages}
        onPageChange={setPage}
      />

      <PersonFormDialog open={createOpen} onOpenChange={setCreateOpen} />
    </div>
  );
}
