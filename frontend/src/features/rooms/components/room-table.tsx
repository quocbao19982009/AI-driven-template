import { useState } from "react";
import { useTranslation } from "react-i18next";
import { MoreHorizontal, Pencil, Trash2, ChevronUp, ChevronDown } from "lucide-react";

function SortIcon({ col, sortBy, sortDir }: { col: string; sortBy: string; sortDir: string }) {
  if (sortBy !== col) return null;
  return sortDir === "asc" ? (
    <ChevronUp className="inline h-3.5 w-3.5 ml-0.5" />
  ) : (
    <ChevronDown className="inline h-3.5 w-3.5 ml-0.5" />
  );
}
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
import { RoomFormDialog } from "./room-form-dialog";
import { RoomDeleteDialog } from "./room-delete-dialog";
import type { RoomDto } from "@/api/generated/models";
import { useAppDispatch, useAppSelector } from "@/store/hooks";
import { setSortBy, setSortDir } from "../store";

interface RoomTableProps {
  rooms: RoomDto[];
  isLoading: boolean;
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

export function RoomTable({ rooms, isLoading, page, totalPages, onPageChange }: RoomTableProps) {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const { sortBy, sortDir } = useAppSelector((s) => s.rooms);
  const [editRoom, setEditRoom] = useState<RoomDto | null>(null);
  const [deleteRoom, setDeleteRoom] = useState<RoomDto | null>(null);

  function handleSort(col: "name" | "capacity" | "createdAt") {
    if (sortBy === col) {
      dispatch(setSortDir(sortDir === "asc" ? "desc" : "asc"));
    } else {
      dispatch(setSortBy(col));
      dispatch(setSortDir("asc"));
    }
  }

  if (isLoading) return <RoomTableSkeleton />;

  return (
    <>
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="w-14">{t("rooms.table.image")}</TableHead>
              <TableHead
                className="cursor-pointer select-none"
                onClick={() => handleSort("name")}
              >
                {t("rooms.table.name")}
                <SortIcon col="name" sortBy={sortBy} sortDir={sortDir} />
              </TableHead>
              <TableHead
                className="cursor-pointer select-none w-28"
                onClick={() => handleSort("capacity")}
              >
                {t("rooms.table.capacity")}
                <SortIcon col="capacity" sortBy={sortBy} sortDir={sortDir} />
              </TableHead>
              <TableHead>{t("rooms.table.location")}</TableHead>
              <TableHead>{t("rooms.table.purpose")}</TableHead>
              <TableHead className="w-16" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {rooms.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} className="text-center text-muted-foreground h-24">
                  {t("rooms.table.empty")}
                </TableCell>
              </TableRow>
            ) : (
              rooms.map((room) => (
                <TableRow key={room.id}>
                  <TableCell>
                    {room.imagePath ? (
                      <img
                        src={room.imagePath}
                        alt={room.name ?? ""}
                        className="h-10 w-10 rounded object-cover"
                      />
                    ) : (
                      <div className="h-10 w-10 rounded bg-muted flex items-center justify-center text-muted-foreground text-xs">
                        —
                      </div>
                    )}
                  </TableCell>
                  <TableCell className="font-medium">{room.name}</TableCell>
                  <TableCell>{room.capacity}</TableCell>
                  <TableCell>{room.locationName}</TableCell>
                  <TableCell className="max-w-48 truncate text-muted-foreground">
                    {room.purpose ?? "—"}
                  </TableCell>
                  <TableCell>
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button variant="ghost" size="icon-xs">
                          <MoreHorizontal className="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuItem onClick={() => setEditRoom(room)}>
                          <Pencil className="mr-2 h-4 w-4" />
                          {t("common.edit")}
                        </DropdownMenuItem>
                        <DropdownMenuItem
                          variant="destructive"
                          onClick={() => setDeleteRoom(room)}
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

      <RoomFormDialog
        room={editRoom}
        open={editRoom !== null}
        onOpenChange={(open) => !open && setEditRoom(null)}
      />
      <RoomDeleteDialog
        room={deleteRoom}
        open={deleteRoom !== null}
        onOpenChange={(open) => !open && setDeleteRoom(null)}
      />
    </>
  );
}

function RoomTableSkeleton() {
  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead className="w-14">Image</TableHead>
            <TableHead>Name</TableHead>
            <TableHead>Capacity</TableHead>
            <TableHead>Location</TableHead>
            <TableHead>Purpose</TableHead>
            <TableHead className="w-16" />
          </TableRow>
        </TableHeader>
        <TableBody>
          {Array.from({ length: 5 }).map((_, i) => (
            <TableRow key={i}>
              <TableCell><Skeleton className="h-10 w-10 rounded" /></TableCell>
              <TableCell><Skeleton className="h-4 w-32" /></TableCell>
              <TableCell><Skeleton className="h-4 w-12" /></TableCell>
              <TableCell><Skeleton className="h-4 w-24" /></TableCell>
              <TableCell><Skeleton className="h-4 w-40" /></TableCell>
              <TableCell><Skeleton className="h-4 w-4" /></TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
