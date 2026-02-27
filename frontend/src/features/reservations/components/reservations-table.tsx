import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { ReservationDto } from "@/api/generated/models";
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
import { ReservationFormDialog } from "./reservation-form-dialog";
import { ReservationDeleteDialog } from "./reservation-delete-dialog";

interface ReservationsTableProps {
  reservations: ReservationDto[];
  isLoading: boolean;
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

export function ReservationsTable({
  reservations,
  isLoading,
  page,
  totalPages,
  onPageChange,
}: ReservationsTableProps) {
  const { t } = useTranslation();
  const [editReservation, setEditReservation] = useState<ReservationDto | null>(null);
  const [deleteReservation, setDeleteReservation] = useState<ReservationDto | null>(null);

  if (isLoading) {
    return <ReservationsTableSkeleton t={t} />;
  }

  return (
    <>
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="w-16">{t("reservations.table.id")}</TableHead>
              <TableHead>{t("reservations.table.factory")}</TableHead>
              <TableHead>{t("reservations.table.startTime")}</TableHead>
              <TableHead>{t("reservations.table.endTime")}</TableHead>
              <TableHead className="hidden sm:table-cell">{t("reservations.table.duration")}</TableHead>
              <TableHead className="hidden md:table-cell">{t("reservations.table.personnel")}</TableHead>
              <TableHead className="w-16" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {reservations.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7} className="text-center text-muted-foreground h-24">
                  {t("reservations.table.empty")}
                </TableCell>
              </TableRow>
            ) : (
              reservations.map((reservation) => (
                <TableRow key={reservation.id}>
                  <TableCell className="font-mono text-muted-foreground">{reservation.id}</TableCell>
                  <TableCell className="font-medium">{reservation.factoryDisplayName}</TableCell>
                  <TableCell className="text-muted-foreground">
                    {reservation.startTime
                      ? new Date(reservation.startTime).toLocaleString()
                      : "—"}
                  </TableCell>
                  <TableCell className="text-muted-foreground">
                    {reservation.endTime
                      ? new Date(reservation.endTime).toLocaleString()
                      : "—"}
                  </TableCell>
                  <TableCell className="hidden sm:table-cell text-muted-foreground">
                    {reservation.durationHours?.toFixed(1)}h
                  </TableCell>
                  <TableCell className="hidden md:table-cell">
                    <div className="flex flex-wrap gap-1">
                      {reservation.personnel?.map((rp) => (
                        <Badge key={rp.id} className="bg-green-100 text-green-700 hover:bg-green-100 text-xs">
                          {rp.personDisplayName}
                        </Badge>
                      ))}
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
                        <DropdownMenuItem onClick={() => setEditReservation(reservation)}>
                          <Pencil className="mr-2 h-4 w-4" />
                          {t("common.edit")}
                        </DropdownMenuItem>
                        <DropdownMenuItem variant="destructive" onClick={() => setDeleteReservation(reservation)}>
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

      <ReservationFormDialog
        reservation={editReservation}
        open={editReservation !== null}
        onOpenChange={(open) => !open && setEditReservation(null)}
      />

      <ReservationDeleteDialog
        reservation={deleteReservation}
        open={deleteReservation !== null}
        onOpenChange={(open) => !open && setDeleteReservation(null)}
      />
    </>
  );
}

function ReservationsTableSkeleton({ t }: { t: (key: string) => string }) {
  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead className="w-16">{t("reservations.table.id")}</TableHead>
            <TableHead>{t("reservations.table.factory")}</TableHead>
            <TableHead>{t("reservations.table.startTime")}</TableHead>
            <TableHead>{t("reservations.table.endTime")}</TableHead>
            <TableHead className="hidden sm:table-cell">{t("reservations.table.duration")}</TableHead>
            <TableHead className="hidden md:table-cell">{t("reservations.table.personnel")}</TableHead>
            <TableHead className="w-16" />
          </TableRow>
        </TableHeader>
        <TableBody>
          {Array.from({ length: 5 }).map((_, i) => (
            <TableRow key={i}>
              <TableCell><Skeleton className="h-4 w-8" /></TableCell>
              <TableCell><Skeleton className="h-4 w-32" /></TableCell>
              <TableCell><Skeleton className="h-4 w-36" /></TableCell>
              <TableCell><Skeleton className="h-4 w-36" /></TableCell>
              <TableCell className="hidden sm:table-cell"><Skeleton className="h-4 w-12" /></TableCell>
              <TableCell className="hidden md:table-cell"><Skeleton className="h-4 w-40" /></TableCell>
              <TableCell><Skeleton className="h-4 w-4" /></TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
