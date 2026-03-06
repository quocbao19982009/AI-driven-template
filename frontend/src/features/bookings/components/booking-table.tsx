import { useState } from "react";
import { useTranslation } from "react-i18next";
import { MoreHorizontal, Pencil, Trash2 } from "lucide-react";
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
import { BookingFormDialog } from "./booking-form-dialog";
import { BookingDeleteDialog } from "./booking-delete-dialog";
import type { BookingDto } from "@/api/generated/models";

interface BookingTableProps {
  bookings: BookingDto[];
  isLoading: boolean;
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

function fmt(iso: string | undefined | null) {
  return iso ? new Date(iso).toLocaleString() : "—";
}

export function BookingTable({ bookings, isLoading, page, totalPages, onPageChange }: BookingTableProps) {
  const { t } = useTranslation();
  const [editBooking, setEditBooking] = useState<BookingDto | null>(null);
  const [deleteBooking, setDeleteBooking] = useState<BookingDto | null>(null);

  if (isLoading) return <BookingTableSkeleton />;

  return (
    <>
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>{t("bookings.table.room")}</TableHead>
              <TableHead>{t("bookings.table.start")}</TableHead>
              <TableHead>{t("bookings.table.end")}</TableHead>
              <TableHead>{t("bookings.table.bookedBy")}</TableHead>
              <TableHead>{t("bookings.table.purpose")}</TableHead>
              <TableHead className="w-16" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {bookings.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} className="text-center text-muted-foreground h-24">
                  {t("bookings.table.empty")}
                </TableCell>
              </TableRow>
            ) : (
              bookings.map((b) => (
                <TableRow key={b.id}>
                  <TableCell className="font-medium">{b.roomName}</TableCell>
                  <TableCell>{fmt(b.startTime)}</TableCell>
                  <TableCell>{fmt(b.endTime)}</TableCell>
                  <TableCell>{b.bookedBy}</TableCell>
                  <TableCell className="max-w-40 truncate text-muted-foreground">
                    {b.purpose ?? "—"}
                  </TableCell>
                  <TableCell>
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button variant="ghost" size="icon-xs">
                          <MoreHorizontal className="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuItem onClick={() => setEditBooking(b)}>
                          <Pencil className="mr-2 h-4 w-4" />
                          {t("common.edit")}
                        </DropdownMenuItem>
                        <DropdownMenuItem
                          variant="destructive"
                          onClick={() => setDeleteBooking(b)}
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

      <BookingFormDialog
        booking={editBooking}
        open={editBooking !== null}
        onOpenChange={(open) => !open && setEditBooking(null)}
      />
      <BookingDeleteDialog
        booking={deleteBooking}
        open={deleteBooking !== null}
        onOpenChange={(open) => !open && setDeleteBooking(null)}
      />
    </>
  );
}

function BookingTableSkeleton() {
  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Room</TableHead>
            <TableHead>Start</TableHead>
            <TableHead>End</TableHead>
            <TableHead>Booked By</TableHead>
            <TableHead>Purpose</TableHead>
            <TableHead className="w-16" />
          </TableRow>
        </TableHeader>
        <TableBody>
          {Array.from({ length: 5 }).map((_, i) => (
            <TableRow key={i}>
              <TableCell><Skeleton className="h-4 w-24" /></TableCell>
              <TableCell><Skeleton className="h-4 w-32" /></TableCell>
              <TableCell><Skeleton className="h-4 w-32" /></TableCell>
              <TableCell><Skeleton className="h-4 w-24" /></TableCell>
              <TableCell><Skeleton className="h-4 w-36" /></TableCell>
              <TableCell><Skeleton className="h-4 w-4" /></TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
