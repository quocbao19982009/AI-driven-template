import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { PersonDto } from "@/api/generated/models";
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
import { MoreHorizontal, Pencil, Trash2 } from "lucide-react";
import { PersonFormDialog } from "./person-form-dialog";
import { PersonDeleteDialog } from "./person-delete-dialog";

interface PersonnelTableProps {
  personnel: PersonDto[];
  isLoading: boolean;
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

export function PersonnelTable({
  personnel,
  isLoading,
  page,
  totalPages,
  onPageChange,
}: PersonnelTableProps) {
  const { t } = useTranslation();
  const [editPerson, setEditPerson] = useState<PersonDto | null>(null);
  const [deletePerson, setDeletePerson] = useState<PersonDto | null>(null);

  if (isLoading) {
    return <PersonnelTableSkeleton t={t} />;
  }

  return (
    <>
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="w-16">{t("personnel.table.id")}</TableHead>
              <TableHead>{t("personnel.table.personalId")}</TableHead>
              <TableHead>{t("personnel.table.fullName")}</TableHead>
              <TableHead className="hidden md:table-cell">{t("personnel.table.email")}</TableHead>
              <TableHead className="hidden lg:table-cell">{t("personnel.table.allowedFactories")}</TableHead>
              <TableHead className="w-16" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {personnel.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} className="text-center text-muted-foreground h-24">
                  {t("personnel.table.empty")}
                </TableCell>
              </TableRow>
            ) : (
              personnel.map((person) => (
                <TableRow key={person.id}>
                  <TableCell className="font-mono text-muted-foreground">{person.id}</TableCell>
                  <TableCell className="font-mono text-sm">{person.personalId}</TableCell>
                  <TableCell className="font-medium">{person.fullName}</TableCell>
                  <TableCell className="hidden md:table-cell text-muted-foreground">{person.email}</TableCell>
                  <TableCell className="hidden lg:table-cell">
                    <div className="flex flex-wrap gap-1">
                      {person.allowedFactories?.map((f) => (
                        <Badge key={f.id} className="bg-blue-100 text-blue-700 hover:bg-blue-100 text-xs">
                          {f.name}
                        </Badge>
                      ))}
                      {(!person.allowedFactories || person.allowedFactories.length === 0) && (
                        <span className="text-muted-foreground text-sm">—</span>
                      )}
                    </div>
                  </TableCell>
                  <TableCell>
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button variant="ghost" size="icon-xs">
                          <MoreHorizontal className="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuItem onClick={() => setEditPerson(person)}>
                          <Pencil className="mr-2 h-4 w-4" />
                          {t("common.edit")}
                        </DropdownMenuItem>
                        <DropdownMenuItem variant="destructive" onClick={() => setDeletePerson(person)}>
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

      <PersonFormDialog
        person={editPerson}
        open={editPerson !== null}
        onOpenChange={(open) => !open && setEditPerson(null)}
      />

      <PersonDeleteDialog
        person={deletePerson}
        open={deletePerson !== null}
        onOpenChange={(open) => !open && setDeletePerson(null)}
      />
    </>
  );
}

function PersonnelTableSkeleton({ t }: { t: (key: string) => string }) {
  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead className="w-16">{t("personnel.table.id")}</TableHead>
            <TableHead>{t("personnel.table.personalId")}</TableHead>
            <TableHead>{t("personnel.table.fullName")}</TableHead>
            <TableHead className="hidden md:table-cell">{t("personnel.table.email")}</TableHead>
            <TableHead className="hidden lg:table-cell">{t("personnel.table.allowedFactories")}</TableHead>
            <TableHead className="w-16" />
          </TableRow>
        </TableHeader>
        <TableBody>
          {Array.from({ length: 5 }).map((_, i) => (
            <TableRow key={i}>
              <TableCell><Skeleton className="h-4 w-8" /></TableCell>
              <TableCell><Skeleton className="h-4 w-24" /></TableCell>
              <TableCell><Skeleton className="h-4 w-40" /></TableCell>
              <TableCell className="hidden md:table-cell"><Skeleton className="h-4 w-48" /></TableCell>
              <TableCell className="hidden lg:table-cell"><Skeleton className="h-4 w-32" /></TableCell>
              <TableCell><Skeleton className="h-4 w-4" /></TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
