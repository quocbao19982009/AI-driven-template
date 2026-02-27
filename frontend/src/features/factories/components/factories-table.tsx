import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { FactoryDto } from "@/api/generated/models";
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
import { FactoryFormDialog } from "./factory-form-dialog";
import { FactoryDeleteDialog } from "./factory-delete-dialog";

interface FactoriesTableProps {
  factories: FactoryDto[];
  isLoading: boolean;
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

export function FactoriesTable({
  factories,
  isLoading,
  page,
  totalPages,
  onPageChange,
}: FactoriesTableProps) {
  const { t } = useTranslation();
  const [editFactory, setEditFactory] = useState<FactoryDto | null>(null);
  const [deleteFactory, setDeleteFactory] = useState<FactoryDto | null>(null);

  if (isLoading) {
    return <FactoriesTableSkeleton t={t} />;
  }

  return (
    <>
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="w-16">{t("factories.table.id")}</TableHead>
              <TableHead>{t("factories.table.name")}</TableHead>
              <TableHead>{t("factories.table.timeZone")}</TableHead>
              <TableHead className="hidden lg:table-cell">{t("factories.table.createdAt")}</TableHead>
              <TableHead className="w-16" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {factories.length === 0 ? (
              <TableRow>
                <TableCell colSpan={5} className="text-center text-muted-foreground h-24">
                  {t("factories.table.empty")}
                </TableCell>
              </TableRow>
            ) : (
              factories.map((factory) => (
                <TableRow key={factory.id}>
                  <TableCell className="font-mono text-muted-foreground">{factory.id}</TableCell>
                  <TableCell className="font-medium">{factory.name}</TableCell>
                  <TableCell className="text-muted-foreground">{factory.timeZone}</TableCell>
                  <TableCell className="hidden lg:table-cell text-muted-foreground">
                    {factory.createdAt ? new Date(factory.createdAt).toLocaleDateString() : "—"}
                  </TableCell>
                  <TableCell>
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button variant="ghost" size="icon-xs">
                          <MoreHorizontal className="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuItem onClick={() => setEditFactory(factory)}>
                          <Pencil className="mr-2 h-4 w-4" />
                          {t("common.edit")}
                        </DropdownMenuItem>
                        <DropdownMenuItem variant="destructive" onClick={() => setDeleteFactory(factory)}>
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
        <Button variant="outline" size="sm" onClick={() => onPageChange(page - 1)} disabled={page <= 1}>
          {t("common.previous")}
        </Button>
        <span className="text-sm text-muted-foreground">
          {t("common.page", { page, total: totalPages || 1 })}
        </span>
        <Button variant="outline" size="sm" onClick={() => onPageChange(page + 1)} disabled={page >= totalPages}>
          {t("common.next")}
        </Button>
      </div>

      <FactoryFormDialog
        factory={editFactory}
        open={editFactory !== null}
        onOpenChange={(open) => !open && setEditFactory(null)}
      />

      <FactoryDeleteDialog
        factory={deleteFactory}
        open={deleteFactory !== null}
        onOpenChange={(open) => !open && setDeleteFactory(null)}
      />
    </>
  );
}

function FactoriesTableSkeleton({ t }: { t: (key: string) => string }) {
  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead className="w-16">{t("factories.table.id")}</TableHead>
            <TableHead>{t("factories.table.name")}</TableHead>
            <TableHead>{t("factories.table.timeZone")}</TableHead>
            <TableHead className="hidden lg:table-cell">{t("factories.table.createdAt")}</TableHead>
            <TableHead className="w-16" />
          </TableRow>
        </TableHeader>
        <TableBody>
          {Array.from({ length: 5 }).map((_, i) => (
            <TableRow key={i}>
              <TableCell><Skeleton className="h-4 w-8" /></TableCell>
              <TableCell><Skeleton className="h-4 w-40" /></TableCell>
              <TableCell><Skeleton className="h-4 w-32" /></TableCell>
              <TableCell className="hidden lg:table-cell"><Skeleton className="h-4 w-24" /></TableCell>
              <TableCell><Skeleton className="h-4 w-4" /></TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
