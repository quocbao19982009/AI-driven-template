import { useRef, useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";
import { ImageIcon } from "lucide-react";
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
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import {
  usePostApiRooms,
  usePutApiRoomsId,
  getGetApiRoomsQueryKey,
  getGetApiRoomsAllQueryKey,
} from "@/api/generated/rooms/rooms";
import { useGetApiLocations } from "@/api/generated/locations/locations";
import type { PostApiRoomsBody, PutApiRoomsIdBody, RoomDto } from "@/api/generated/models";

interface RoomFormDialogProps {
  room?: RoomDto | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function RoomFormDialog({ room, open, onOpenChange }: RoomFormDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <RoomFormContent
        key={open ? (room?.id ?? "new") : "closed"}
        room={room}
        open={open}
        onOpenChange={onOpenChange}
      />
    </Dialog>
  );
}

function RoomFormContent({ room, open, onOpenChange }: RoomFormDialogProps) {
  const { t } = useTranslation();
  const isEditing = !!room;
  const queryClient = useQueryClient();

  const { data: locationsRes } = useGetApiLocations();
  const locations = locationsRes?.data?.data ?? [];

  const [name, setName] = useState(room?.name ?? "");
  const [capacity, setCapacity] = useState(room?.capacity ?? 1);
  const [locationId, setLocationId] = useState<string>(
    room?.locationId ? String(room.locationId) : "",
  );
  const [purpose, setPurpose] = useState(room?.purpose ?? "");
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [imagePreview, setImagePreview] = useState<string | null>(room?.imagePath ?? null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: getGetApiRoomsQueryKey() });
    queryClient.invalidateQueries({ queryKey: getGetApiRoomsAllQueryKey() });
  };

  const createMutation = usePostApiRooms({
    mutation: {
      onSuccess: () => {
        invalidate();
        onOpenChange(false);
        toast.success(t("rooms.toast.created"));
      },
      onError: (err) =>
        toast.error(err instanceof Error ? err.message : t("rooms.toast.createError")),
    },
  });

  const updateMutation = usePutApiRoomsId({
    mutation: {
      onSuccess: () => {
        invalidate();
        onOpenChange(false);
        toast.success(t("rooms.toast.updated"));
      },
      onError: (err) =>
        toast.error(err instanceof Error ? err.message : t("rooms.toast.updateError")),
    },
  });

  const isPending = createMutation.isPending || updateMutation.isPending;

  function handleImageChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0] ?? null;
    setImageFile(file);
    if (file) {
      setImagePreview(URL.createObjectURL(file));
    }
  }

  function buildBody(): PostApiRoomsBody {
    return {
      Name: name,
      Capacity: capacity,
      LocationId: Number(locationId),
      Purpose: purpose || undefined,
      image: imageFile ?? undefined,
    };
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const body = buildBody();
    if (isEditing && room?.id) {
      updateMutation.mutate({ id: room.id, data: body as PutApiRoomsIdBody });
    } else {
      createMutation.mutate({ data: body });
    }
  }

  if (!open) return null;

  return (
    <DialogContent className="sm:max-w-lg">
      <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>
              {isEditing ? t("rooms.form.editTitle") : t("rooms.form.createTitle")}
            </DialogTitle>
            <DialogDescription>
              {isEditing ? t("rooms.form.editDescription") : t("rooms.form.createDescription")}
            </DialogDescription>
          </DialogHeader>

          <div className="grid gap-4 py-4">
            <div className="grid gap-1.5">
              <Label htmlFor="room-name">{t("rooms.form.nameLabel")}</Label>
              <Input
                id="room-name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder={t("rooms.form.namePlaceholder")}
                required
              />
            </div>

            <div className="grid gap-1.5">
              <Label htmlFor="room-capacity">{t("rooms.form.capacityLabel")}</Label>
              <Input
                id="room-capacity"
                type="number"
                min={1}
                value={capacity}
                onChange={(e) => setCapacity(Number(e.target.value))}
                required
              />
            </div>

            <div className="grid gap-1.5">
              <Label>{t("rooms.form.locationLabel")}</Label>
              <Select value={locationId} onValueChange={setLocationId} required>
                <SelectTrigger>
                  <SelectValue placeholder={t("rooms.form.locationPlaceholder")} />
                </SelectTrigger>
                <SelectContent>
                  {locations.map((loc) => (
                    <SelectItem key={loc.id} value={String(loc.id)}>
                      {loc.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="grid gap-1.5">
              <Label htmlFor="room-purpose">{t("rooms.form.purposeLabel")}</Label>
              <Textarea
                id="room-purpose"
                value={purpose}
                onChange={(e) => setPurpose(e.target.value)}
                placeholder={t("rooms.form.purposePlaceholder")}
                rows={2}
              />
            </div>

            <div className="grid gap-1.5">
              <Label>{t("rooms.form.imageLabel")}</Label>
              {imagePreview ? (
                <div className="flex items-center gap-3">
                  <img
                    src={imagePreview}
                    alt="Preview"
                    className="h-16 w-16 rounded object-cover border"
                  />
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={() => {
                      setImageFile(null);
                      setImagePreview(null);
                      if (fileInputRef.current) fileInputRef.current.value = "";
                    }}
                  >
                    {t("rooms.form.removeImage")}
                  </Button>
                </div>
              ) : (
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={() => fileInputRef.current?.click()}
                  className="w-fit"
                >
                  <ImageIcon className="mr-2 h-4 w-4" />
                  {t("rooms.form.uploadImage")}
                </Button>
              )}
              <input
                ref={fileInputRef}
                type="file"
                accept="image/jpeg,image/png"
                className="hidden"
                onChange={handleImageChange}
              />
            </div>
          </div>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)} disabled={isPending}>
              {t("common.cancel")}
            </Button>
            <Button type="submit" disabled={isPending || !locationId}>
              {isPending ? t("common.saving") : isEditing ? t("common.save") : t("common.create")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
  );
}
