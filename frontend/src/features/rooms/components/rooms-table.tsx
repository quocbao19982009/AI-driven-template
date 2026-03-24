import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useAppDispatch, useAppSelector } from "@/store/hooks";
import { setSortBy, setSortDir } from "../store/rooms-slice";
import type { RoomDto } from "@/api/generated/models";
import { assetUrl } from "@/lib/utils";
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
import {
  MoreHorizontal,
  Pencil,
  Trash2,
  ChevronUp,
  ChevronDown,
  Image,
} from "lucide-react";
import { RoomFormDialog } from "./room-form-dialog";
import { RoomDeleteDialog } from "./room-delete-dialog";

interface RoomsTableProps {
  rooms: RoomDto[];
  isLoading: boolean;
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

export function RoomsTable({
  rooms,
  isLoading,
  page,
  totalPages,
  onPageChange,
}: RoomsTableProps) {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const { sortBy, sortDir } = useAppSelector((s) => s.rooms);
  const [editRoom, setEditRoom] = useState<RoomDto | null>(null);
  const [deleteRoom, setDeleteRoom] = useState<RoomDto | null>(null);

  function handleSort(column: "name" | "capacity" | "createdAt") {
    if (sortBy === column) {
      dispatch(setSortDir(sortDir === "asc" ? "desc" : "asc"));
    } else {
      dispatch(setSortBy(column));
      dispatch(setSortDir("asc"));
    }
  }

  function sortIcon(column: string) {
    if (sortBy !== column) return null;
    return sortDir === "asc" ? (
      <ChevronUp className="ml-1 inline h-3 w-3" />
    ) : (
      <ChevronDown className="ml-1 inline h-3 w-3" />
    );
  }

  if (isLoading) {
    return <RoomsTableSkeleton />;
  }

  return (
    <>
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="w-16">{t("rooms.table.image")}</TableHead>
              <TableHead
                className="cursor-pointer select-none"
                onClick={() => handleSort("name")}
              >
                {t("rooms.table.name")}
                {sortIcon("name")}
              </TableHead>
              <TableHead
                className="cursor-pointer select-none"
                onClick={() => handleSort("capacity")}
              >
                {t("rooms.table.capacity")}
                {sortIcon("capacity")}
              </TableHead>
              <TableHead>{t("rooms.table.location")}</TableHead>
              <TableHead>{t("rooms.table.purpose")}</TableHead>
              <TableHead className="w-16" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {rooms.length === 0 ? (
              <TableRow>
                <TableCell
                  colSpan={6}
                  className="text-muted-foreground h-24 text-center"
                >
                  {t("rooms.table.empty")}
                </TableCell>
              </TableRow>
            ) : (
              rooms.map((room) => (
                <TableRow key={room.id}>
                  <TableCell>
                    {room.imagePath ? (
                      <img
                        src={assetUrl(room.imagePath)}
                        alt={room.name}
                        className="h-10 w-10 rounded object-cover"
                      />
                    ) : (
                      <div className="bg-muted flex h-10 w-10 items-center justify-center rounded">
                        <Image className="text-muted-foreground h-5 w-5" />
                      </div>
                    )}
                  </TableCell>
                  <TableCell className="font-medium">{room.name}</TableCell>
                  <TableCell>{room.capacity}</TableCell>
                  <TableCell>{room.locationName}</TableCell>
                  <TableCell className="text-muted-foreground max-w-xs truncate">
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
        <span className="text-muted-foreground text-sm">
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

function RoomsTableSkeleton() {
  const { t } = useTranslation();
  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead className="w-16">{t("rooms.table.image")}</TableHead>
            <TableHead>{t("rooms.table.name")}</TableHead>
            <TableHead>{t("rooms.table.capacity")}</TableHead>
            <TableHead>{t("rooms.table.location")}</TableHead>
            <TableHead>{t("rooms.table.purpose")}</TableHead>
            <TableHead className="w-16" />
          </TableRow>
        </TableHeader>
        <TableBody>
          {Array.from({ length: 5 }).map((_, i) => (
            <TableRow key={i}>
              <TableCell>
                <Skeleton className="h-10 w-10 rounded" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-4 w-32" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-4 w-12" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-4 w-24" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-4 w-40" />
              </TableCell>
              <TableCell>
                <Skeleton className="h-4 w-4" />
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
