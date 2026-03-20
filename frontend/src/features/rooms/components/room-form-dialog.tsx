import { useRef, useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import type { RoomDto } from "@/api/generated/models";
import { assetUrl } from "@/lib/utils";
import {
  usePostApiRooms,
  usePutApiRoomsId,
  getGetApiRoomsQueryKey,
  getGetApiRoomsAllQueryKey,
} from "@/api/generated/rooms/rooms";
import { useGetApiLocations } from "@/api/generated/locations/locations";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";

interface RoomFormDialogProps {
  room?: RoomDto | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function RoomFormDialog({ room, open, onOpenChange }: RoomFormDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      {open && (
        <RoomFormDialogContent
          key={`${room?.id ?? "new"}-${open}`}
          room={room}
          onOpenChange={onOpenChange}
        />
      )}
    </Dialog>
  );
}

const roomFormSchema = z.object({
  Name: z.string().min(1, "Name is required").max(200, "Name must not exceed 200 characters"),
  Capacity: z
    .number({ invalid_type_error: "Capacity must be a number" })
    .int()
    .min(1, "Capacity must be greater than 0"),
  LocationId: z
    .number({ invalid_type_error: "Location is required" })
    .min(1, "Location is required"),
  Purpose: z.string().max(500, "Purpose must not exceed 500 characters").optional(),
  Image: z.instanceof(File).optional(),
});

type RoomFormValues = z.infer<typeof roomFormSchema>;

interface RoomFormDialogContentProps {
  room?: RoomDto | null;
  onOpenChange: (open: boolean) => void;
}

function RoomFormDialogContent({ room, onOpenChange }: RoomFormDialogContentProps) {
  const { t } = useTranslation();
  const isEditing = !!room;
  const queryClient = useQueryClient();
  const fileInputRef = useRef<HTMLInputElement>(null);

  // selectedFile state initialises once on mount — no need for an effect reset
  const [selectedFile, setSelectedFile] = useState<File | null>(null);

  const { data: locationsResponse } = useGetApiLocations();
  const locations = locationsResponse?.data?.data ?? [];

  const form = useForm<RoomFormValues>({
    resolver: zodResolver(roomFormSchema),
    defaultValues: {
      Name: room?.name ?? "",
      Capacity: room?.capacity ?? 1,
      LocationId: room?.locationId ?? undefined,
      Purpose: room?.purpose ?? "",
    },
  });

  const imagePreview = selectedFile
    ? URL.createObjectURL(selectedFile)
    : room?.imagePath
      ? assetUrl(room.imagePath)
      : null;

  const createMutation = usePostApiRooms({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiRoomsQueryKey() });
        queryClient.invalidateQueries({ queryKey: getGetApiRoomsAllQueryKey() });
        onOpenChange(false);
        toast.success(t("rooms.toast.created"));
      },
      onError: (error) => {
        toast.error(error instanceof Error ? error.message : t("rooms.toast.createError"));
      },
    },
  });

  const updateMutation = usePutApiRoomsId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiRoomsQueryKey() });
        queryClient.invalidateQueries({ queryKey: getGetApiRoomsAllQueryKey() });
        onOpenChange(false);
        toast.success(t("rooms.toast.updated"));
      },
      onError: (error) => {
        toast.error(error instanceof Error ? error.message : t("rooms.toast.updateError"));
      },
    },
  });

  const isPending = createMutation.isPending || updateMutation.isPending;

  function onSubmit(values: RoomFormValues) {
    const body = {
      Name: values.Name,
      Capacity: values.Capacity,
      LocationId: values.LocationId,
      Purpose: values.Purpose,
      Image: values.Image,
    };

    if (isEditing && room?.id) {
      updateMutation.mutate({ id: room.id, data: body });
    } else {
      createMutation.mutate({ data: body });
    }
  }

  function handleFileChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (file) {
      form.setValue("Image", file);
      setSelectedFile(file);
    }
  }

  return (
    <DialogContent className="sm:max-w-lg">
      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)}>
          <DialogHeader>
            <DialogTitle>
              {isEditing ? t("rooms.form.editTitle") : t("rooms.form.createTitle")}
            </DialogTitle>
            <DialogDescription>
              {isEditing ? t("rooms.form.editDescription") : t("rooms.form.createDescription")}
            </DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <FormField
              control={form.control}
              name="Name"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>{t("rooms.form.nameLabel")}</FormLabel>
                  <FormControl>
                    <Input placeholder={t("rooms.form.namePlaceholder")} {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="Capacity"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>{t("rooms.form.capacityLabel")}</FormLabel>
                  <FormControl>
                    <Input
                      type="number"
                      min={1}
                      {...field}
                      onChange={(e) => field.onChange(e.target.valueAsNumber)}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="LocationId"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>{t("rooms.form.locationLabel")}</FormLabel>
                  <FormControl>
                    <select
                      className="border-input bg-background ring-offset-background focus-visible:ring-ring flex h-10 w-full rounded-md border px-3 py-2 text-sm focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:outline-none"
                      value={field.value ?? ""}
                      onChange={(e) => field.onChange(Number(e.target.value) || undefined)}
                    >
                      <option value="">{t("rooms.form.locationPlaceholder")}</option>
                      {locations.map((loc) => (
                        <option key={loc.id} value={loc.id}>
                          {loc.name}
                        </option>
                      ))}
                    </select>
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="Purpose"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>{t("rooms.form.purposeLabel")}</FormLabel>
                  <FormControl>
                    <Textarea
                      placeholder={t("rooms.form.purposePlaceholder")}
                      rows={3}
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormItem>
              <FormLabel>{t("rooms.form.imageLabel")}</FormLabel>
              <FormControl>
                <Input
                  ref={fileInputRef}
                  type="file"
                  accept="image/jpeg,image/png"
                  onChange={handleFileChange}
                />
              </FormControl>
              {imagePreview && (
                <img
                  src={imagePreview}
                  alt={t("rooms.form.imagePreviewAlt")}
                  className="mt-2 h-24 w-auto rounded-md object-cover"
                />
              )}
              <FormMessage />
            </FormItem>
          </div>
          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={isPending}
            >
              {t("common.cancel")}
            </Button>
            <Button type="submit" disabled={isPending}>
              {isPending
                ? t("common.saving")
                : isEditing
                  ? t("common.save")
                  : t("common.create")}
            </Button>
          </DialogFooter>
        </form>
      </Form>
    </DialogContent>
  );
}
