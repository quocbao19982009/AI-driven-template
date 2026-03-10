import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import { Plus } from "lucide-react";
import { FeaturesTable } from "./features-table";
import { FeatureFormDialog } from "./feature-form-dialog";
import { useFeaturePagination } from "../hooks";

export function FeaturesPage() {
  const { t } = useTranslation();
  const [createOpen, setCreateOpen] = useState(false);
  const { features, isLoading, page, totalPages, setPage } =
    useFeaturePagination();

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">
            {t("features.title")}
          </h1>
          <p className="text-muted-foreground">{t("features.description")}</p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          {t("features.newFeature")}
        </Button>
      </div>

      <FeaturesTable
        features={features}
        isLoading={isLoading}
        page={page}
        totalPages={totalPages}
        onPageChange={setPage}
      />

      <FeatureFormDialog open={createOpen} onOpenChange={setCreateOpen} />
    </div>
  );
}
