import { useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";
import type { LocationDto } from "@/api/generated/models";
import {
  useGetApiLocations,
  usePostApiLocations,
  usePutApiLocationsId,
  useDeleteApiLocationsId,
  getGetApiLocationsQueryKey,
} from "@/api/generated/locations/locations";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Pencil, Trash2, Plus, Check, X } from "lucide-react";
import { Skeleton } from "@/components/ui/skeleton";

interface LocationsManagerProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function LocationsManager({
  open,
  onOpenChange,
}: LocationsManagerProps) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [editingId, setEditingId] = useState<number | null>(null);
  const [editingName, setEditingName] = useState("");
  const [newName, setNewName] = useState("");
  const [isAddingNew, setIsAddingNew] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<LocationDto | null>(null);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  const { data: response, isLoading } = useGetApiLocations();
  const locations: LocationDto[] = response?.data?.data ?? [];

  const createMutation = usePostApiLocations({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiLocationsQueryKey(),
        });
        setNewName("");
        setIsAddingNew(false);
        toast.success(t("locations.toast.created"));
      },
      onError: (error) => {
        toast.error(
          error instanceof Error
            ? error.message
            : t("locations.toast.createError")
        );
      },
    },
  });

  const updateMutation = usePutApiLocationsId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiLocationsQueryKey(),
        });
        setEditingId(null);
        toast.success(t("locations.toast.updated"));
      },
      onError: (error) => {
        toast.error(
          error instanceof Error
            ? error.message
            : t("locations.toast.updateError")
        );
      },
    },
  });

  const deleteMutation = useDeleteApiLocationsId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiLocationsQueryKey(),
        });
        setDeleteTarget(null);
        setDeleteError(null);
        toast.success(t("locations.toast.deleted"));
      },
      onError: (error) => {
        setDeleteError(
          error instanceof Error
            ? error.message
            : t("locations.toast.deleteError")
        );
      },
    },
  });

  function startEdit(location: LocationDto) {
    setEditingId(location.id);
    setEditingName(location.name);
  }

  function cancelEdit() {
    setEditingId(null);
    setEditingName("");
  }

  function commitEdit(id: number) {
    if (!editingName.trim()) return;
    updateMutation.mutate({ id, data: { name: editingName.trim() } });
  }

  function commitNew() {
    if (!newName.trim()) return;
    createMutation.mutate({ data: { name: newName.trim() } });
  }

  function handleDeleteConfirm() {
    if (deleteTarget) {
      setDeleteError(null);
      deleteMutation.mutate({ id: deleteTarget.id });
    }
  }

  return (
    <>
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent className="sm:max-w-lg">
          <DialogHeader>
            <DialogTitle>{t("locations.title")}</DialogTitle>
            <DialogDescription>{t("locations.description")}</DialogDescription>
          </DialogHeader>

          {isLoading ? (
            <div className="space-y-2">
              {Array.from({ length: 3 }).map((_, i) => (
                <Skeleton key={i} className="h-8 w-full" />
              ))}
            </div>
          ) : (
            <div className="rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>{t("locations.table.name")}</TableHead>
                    <TableHead className="w-24" />
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {locations.length === 0 && !isAddingNew && (
                    <TableRow>
                      <TableCell
                        colSpan={2}
                        className="text-muted-foreground h-16 text-center"
                      >
                        {t("locations.table.empty")}
                      </TableCell>
                    </TableRow>
                  )}
                  {locations.map((location) => (
                    <TableRow key={location.id}>
                      <TableCell>
                        {editingId === location.id ? (
                          <Input
                            value={editingName}
                            onChange={(e) => setEditingName(e.target.value)}
                            onKeyDown={(e) => {
                              if (e.key === "Enter") commitEdit(location.id);
                              if (e.key === "Escape") cancelEdit();
                            }}
                            autoFocus
                            className="h-7"
                          />
                        ) : (
                          location.name
                        )}
                      </TableCell>
                      <TableCell>
                        <div className="flex gap-1">
                          {editingId === location.id ? (
                            <>
                              <Button
                                variant="ghost"
                                size="icon-xs"
                                onClick={() => commitEdit(location.id)}
                                disabled={updateMutation.isPending}
                              >
                                <Check className="h-4 w-4" />
                              </Button>
                              <Button
                                variant="ghost"
                                size="icon-xs"
                                onClick={cancelEdit}
                              >
                                <X className="h-4 w-4" />
                              </Button>
                            </>
                          ) : (
                            <>
                              <Button
                                variant="ghost"
                                size="icon-xs"
                                onClick={() => startEdit(location)}
                              >
                                <Pencil className="h-4 w-4" />
                              </Button>
                              <Button
                                variant="ghost"
                                size="icon-xs"
                                onClick={() => {
                                  setDeleteTarget(location);
                                  setDeleteError(null);
                                }}
                              >
                                <Trash2 className="text-destructive h-4 w-4" />
                              </Button>
                            </>
                          )}
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                  {isAddingNew && (
                    <TableRow>
                      <TableCell>
                        <Input
                          value={newName}
                          onChange={(e) => setNewName(e.target.value)}
                          placeholder={t("locations.form.namePlaceholder")}
                          onKeyDown={(e) => {
                            if (e.key === "Enter") commitNew();
                            if (e.key === "Escape") {
                              setIsAddingNew(false);
                              setNewName("");
                            }
                          }}
                          autoFocus
                          className="h-7"
                        />
                      </TableCell>
                      <TableCell>
                        <div className="flex gap-1">
                          <Button
                            variant="ghost"
                            size="icon-xs"
                            onClick={commitNew}
                            disabled={createMutation.isPending}
                          >
                            <Check className="h-4 w-4" />
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon-xs"
                            onClick={() => {
                              setIsAddingNew(false);
                              setNewName("");
                            }}
                          >
                            <X className="h-4 w-4" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </div>
          )}

          <div className="flex justify-between pt-2">
            <Button
              variant="outline"
              size="sm"
              onClick={() => {
                setIsAddingNew(true);
                setNewName("");
              }}
              disabled={isAddingNew}
            >
              <Plus className="mr-1 h-4 w-4" />
              {t("locations.addLocation")}
            </Button>
            <Button variant="outline" onClick={() => onOpenChange(false)}>
              {t("common.cancel")}
            </Button>
          </div>
        </DialogContent>
      </Dialog>

      {/* Delete confirmation dialog */}
      <Dialog
        open={deleteTarget !== null}
        onOpenChange={(open) => {
          if (!open) {
            setDeleteTarget(null);
            setDeleteError(null);
          }
        }}
      >
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>{t("locations.delete.title")}</DialogTitle>
            <DialogDescription>
              {t("locations.delete.description", {
                name: deleteTarget?.name ?? "",
              })}
            </DialogDescription>
          </DialogHeader>
          {deleteError && (
            <p className="text-destructive text-sm">{deleteError}</p>
          )}
          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => {
                setDeleteTarget(null);
                setDeleteError(null);
              }}
              disabled={deleteMutation.isPending}
            >
              {t("common.cancel")}
            </Button>
            <Button
              variant="destructive"
              onClick={handleDeleteConfirm}
              disabled={deleteMutation.isPending}
            >
              {deleteMutation.isPending
                ? t("common.deleting")
                : t("common.delete")}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
