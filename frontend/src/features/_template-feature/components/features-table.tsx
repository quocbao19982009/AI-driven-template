import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { FeatureDto } from "@/api/generated/models";
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
import { FeatureFormDialog } from "./feature-form-dialog";
import { FeatureDeleteDialog } from "./feature-delete-dialog";

interface FeaturesTableProps {
  features: FeatureDto[];
  isLoading: boolean;
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

export function FeaturesTable({
  features,
  isLoading,
  page,
  totalPages,
  onPageChange,
}: FeaturesTableProps) {
  const { t } = useTranslation();
  const [editFeature, setEditFeature] = useState<FeatureDto | null>(null);
  const [deleteFeature, setDeleteFeature] = useState<FeatureDto | null>(null);

  if (isLoading) {
    return <FeaturesTableSkeleton />;
  }

  return (
    <>
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="w-16">{t("features.table.id")}</TableHead>
              <TableHead>{t("features.table.name")}</TableHead>
              <TableHead>{t("features.table.createdAt")}</TableHead>
              <TableHead className="w-16" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {features.length === 0 ? (
              <TableRow>
                <TableCell colSpan={4} className="text-center text-muted-foreground h-24">
                  {t("features.table.empty")}
                </TableCell>
              </TableRow>
            ) : (
              features.map((feature) => (
                <TableRow key={feature.id}>
                  <TableCell className="font-mono text-muted-foreground">
                    {feature.id}
                  </TableCell>
                  <TableCell className="font-medium">{feature.name}</TableCell>
                  <TableCell>
                    {feature.createdAt
                      ? new Date(feature.createdAt).toLocaleDateString()
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
                        <DropdownMenuItem onClick={() => setEditFeature(feature)}>
                          <Pencil className="mr-2 h-4 w-4" />
                          {t("common.edit")}
                        </DropdownMenuItem>
                        <DropdownMenuItem
                          variant="destructive"
                          onClick={() => setDeleteFeature(feature)}
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

      <FeatureFormDialog
        feature={editFeature}
        open={editFeature !== null}
        onOpenChange={(open) => !open && setEditFeature(null)}
      />

      <FeatureDeleteDialog
        feature={deleteFeature}
        open={deleteFeature !== null}
        onOpenChange={(open) => !open && setDeleteFeature(null)}
      />
    </>
  );
}

function FeaturesTableSkeleton() {
  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead className="w-16">ID</TableHead>
            <TableHead>Name</TableHead>
            <TableHead>Created At</TableHead>
            <TableHead className="w-16" />
          </TableRow>
        </TableHeader>
        <TableBody>
          {Array.from({ length: 5 }).map((_, i) => (
            <TableRow key={i}>
              <TableCell><Skeleton className="h-4 w-8" /></TableCell>
              <TableCell><Skeleton className="h-4 w-40" /></TableCell>
              <TableCell><Skeleton className="h-4 w-24" /></TableCell>
              <TableCell><Skeleton className="h-4 w-4" /></TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
