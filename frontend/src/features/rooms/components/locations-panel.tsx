import { useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Pencil, Trash2, Check, X, Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Skeleton } from "@/components/ui/skeleton";
import { toast } from "sonner";
import {
  useGetApiLocations,
  usePostApiLocationsWithJson,
  usePutApiLocationsIdWithJson,
  useDeleteApiLocationsId,
  getGetApiLocationsQueryKey,
} from "@/api/generated/locations/locations";
import type { LocationDto } from "@/api/generated/models";

interface LocationsPanelProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function LocationsPanel({ open, onOpenChange }: LocationsPanelProps) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const { data: response, isLoading } = useGetApiLocations();
  const locations: LocationDto[] = response?.data?.data ?? [];

  const [editingId, setEditingId] = useState<number | null>(null);
  const [editingName, setEditingName] = useState("");
  const [newName, setNewName] = useState("");
  const [deleteError, setDeleteError] = useState<Record<number, string>>({});

  const invalidate = () =>
    queryClient.invalidateQueries({ queryKey: getGetApiLocationsQueryKey() });

  const createMutation = usePostApiLocationsWithJson({
    mutation: {
      onSuccess: () => {
        invalidate();
        setNewName("");
        toast.success(t("rooms.locations.toast.created"));
      },
      onError: (err) =>
        toast.error(
          err instanceof Error
            ? err.message
            : t("rooms.locations.toast.createError")
        ),
    },
  });

  const updateMutation = usePutApiLocationsIdWithJson({
    mutation: {
      onSuccess: () => {
        invalidate();
        setEditingId(null);
        toast.success(t("rooms.locations.toast.updated"));
      },
      onError: (err) =>
        toast.error(
          err instanceof Error
            ? err.message
            : t("rooms.locations.toast.updateError")
        ),
    },
  });

  const deleteMutation = useDeleteApiLocationsId({
    mutation: {
      onSuccess: (_, { id }) => {
        invalidate();
        setDeleteError((prev) => {
          const next = { ...prev };
          delete next[id];
          return next;
        });
        toast.success(t("rooms.locations.toast.deleted"));
      },
      onError: (err, { id }) => {
        const msg =
          err instanceof Error
            ? err.message
            : t("rooms.locations.toast.deleteError");
        setDeleteError((prev) => ({ ...prev, [id]: msg }));
      },
    },
  });

  function startEdit(loc: LocationDto) {
    setEditingId(loc.id!);
    setEditingName(loc.name!);
  }

  function commitEdit() {
    if (!editingId || !editingName.trim()) return;
    updateMutation.mutate({
      id: editingId,
      data: { name: editingName.trim() },
    });
  }

  function handleAdd() {
    if (!newName.trim()) return;
    createMutation.mutate({ data: { name: newName.trim() } });
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>{t("rooms.locations.title")}</DialogTitle>
        </DialogHeader>

        {isLoading ? (
          <div className="space-y-2">
            {Array.from({ length: 4 }).map((_, i) => (
              <Skeleton key={i} className="h-8 w-full" />
            ))}
          </div>
        ) : (
          <div className="rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>{t("rooms.locations.table.name")}</TableHead>
                  <TableHead className="w-20" />
                </TableRow>
              </TableHeader>
              <TableBody>
                {locations.map((loc) => (
                  <TableRow key={loc.id}>
                    <TableCell>
                      {editingId === loc.id ? (
                        <Input
                          value={editingName}
                          onChange={(e) => setEditingName(e.target.value)}
                          onKeyDown={(e) => {
                            if (e.key === "Enter") commitEdit();
                            if (e.key === "Escape") setEditingId(null);
                          }}
                          autoFocus
                          className="h-7"
                        />
                      ) : (
                        <div>
                          <span>{loc.name}</span>
                          {deleteError[loc.id!] && (
                            <p className="text-xs text-destructive mt-0.5">
                              {deleteError[loc.id!]}
                            </p>
                          )}
                        </div>
                      )}
                    </TableCell>
                    <TableCell>
                      <div className="flex gap-1 justify-end">
                        {editingId === loc.id ? (
                          <>
                            <Button
                              size="icon-xs"
                              variant="ghost"
                              onClick={commitEdit}
                              disabled={updateMutation.isPending}
                            >
                              <Check className="h-3.5 w-3.5" />
                            </Button>
                            <Button
                              size="icon-xs"
                              variant="ghost"
                              onClick={() => setEditingId(null)}
                            >
                              <X className="h-3.5 w-3.5" />
                            </Button>
                          </>
                        ) : (
                          <>
                            <Button
                              size="icon-xs"
                              variant="ghost"
                              onClick={() => startEdit(loc)}
                            >
                              <Pencil className="h-3.5 w-3.5" />
                            </Button>
                            <Button
                              size="icon-xs"
                              variant="ghost"
                              onClick={() =>
                                deleteMutation.mutate({ id: loc.id! })
                              }
                              disabled={deleteMutation.isPending}
                            >
                              <Trash2 className="h-3.5 w-3.5 text-destructive" />
                            </Button>
                          </>
                        )}
                      </div>
                    </TableCell>
                  </TableRow>
                ))}

                {/* Add new row */}
                <TableRow>
                  <TableCell>
                    <Input
                      value={newName}
                      onChange={(e) => setNewName(e.target.value)}
                      placeholder={t("rooms.locations.table.newPlaceholder")}
                      onKeyDown={(e) => e.key === "Enter" && handleAdd()}
                      className="h-7"
                    />
                  </TableCell>
                  <TableCell>
                    <Button
                      size="icon-xs"
                      variant="ghost"
                      onClick={handleAdd}
                      disabled={!newName.trim() || createMutation.isPending}
                    >
                      <Plus className="h-3.5 w-3.5" />
                    </Button>
                  </TableCell>
                </TableRow>
              </TableBody>
            </Table>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}
