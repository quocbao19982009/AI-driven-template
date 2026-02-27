import { useEffect, useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { z } from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import type { ReservationDto } from "@/api/generated/models";
import {
  usePostApiReservationsWithJson,
  usePutApiReservationsIdWithJson,
  getGetApiReservationsQueryKey,
} from "@/api/generated/reservations/reservations";
import {
  getGetApiSchedulingByPersonQueryKey,
  getGetApiSchedulingByFactoryQueryKey,
} from "@/api/generated/scheduling/scheduling";
import {
  PostApiReservationsWithJsonBody,
  PutApiReservationsIdWithJsonBody,
} from "@/api/generated/reservations/reservations.zod";
import { useGetApiFactoriesAll } from "@/api/generated/factories/factories";
import { useGetApiPersonnelAll } from "@/api/generated/personnel/personnel";
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

type CreateFormValues = z.infer<typeof PostApiReservationsWithJsonBody>;
type UpdateFormValues = z.infer<typeof PutApiReservationsIdWithJsonBody>;

interface ReservationFormDialogProps {
  reservation?: ReservationDto | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function ReservationFormDialog({
  reservation,
  open,
  onOpenChange,
}: ReservationFormDialogProps) {
  const { t } = useTranslation();
  const isEditing = !!reservation;
  const queryClient = useQueryClient();

  const { data: factoriesResponse } = useGetApiFactoriesAll();
  const { data: personnelResponse } = useGetApiPersonnelAll();
  const allFactories = factoriesResponse?.data?.data ?? [];
  const allPersonnel = personnelResponse?.data?.data ?? [];

  const [selectedFactoryId, setSelectedFactoryId] = useState<number | null>(
    reservation?.factoryId ?? null
  );

  const filteredPersonnel = selectedFactoryId
    ? allPersonnel.filter((p) =>
        p.allowedFactories?.some((f) => f.id === selectedFactoryId)
      )
    : [];

  const defaultCreateValues: CreateFormValues = {
    factoryId: 0,
    startTime: "",
    endTime: "",
    personIds: [],
  };

  const createForm = useForm<CreateFormValues>({
    resolver: zodResolver(PostApiReservationsWithJsonBody),
    defaultValues: defaultCreateValues,
  });

  const updateForm = useForm<UpdateFormValues>({
    resolver: zodResolver(PutApiReservationsIdWithJsonBody),
    defaultValues: {
      factoryId: reservation?.factoryId ?? 0,
      startTime: reservation?.startTime ?? "",
      endTime: reservation?.endTime ?? "",
      personIds: reservation?.personnel?.map((p) => p.personId).filter((id): id is number => id !== null && id !== undefined) ?? [],
    },
  });

  useEffect(() => {
    if (open) {
      if (isEditing && reservation) {
        const personIds = reservation.personnel
          ?.map((p) => p.personId)
          .filter((id): id is number => id !== null && id !== undefined) ?? [];
        updateForm.reset({
          factoryId: reservation.factoryId ?? 0,
          startTime: reservation.startTime,
          endTime: reservation.endTime,
          personIds,
        });
        setSelectedFactoryId(reservation.factoryId ?? null);
      } else {
        createForm.reset(defaultCreateValues);
        setSelectedFactoryId(null);
      }
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, reservation, isEditing]);

  const createMutation = usePostApiReservationsWithJson({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiReservationsQueryKey() });
        queryClient.invalidateQueries({ queryKey: getGetApiSchedulingByPersonQueryKey() });
        queryClient.invalidateQueries({ queryKey: getGetApiSchedulingByFactoryQueryKey() });
        onOpenChange(false);
        toast.success(t("reservations.toast.created"));
      },
      onError: (error) => {
        toast.error(error instanceof Error ? error.message : t("reservations.toast.createError"));
      },
    },
  });

  const updateMutation = usePutApiReservationsIdWithJson({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiReservationsQueryKey() });
        queryClient.invalidateQueries({ queryKey: getGetApiSchedulingByPersonQueryKey() });
        queryClient.invalidateQueries({ queryKey: getGetApiSchedulingByFactoryQueryKey() });
        onOpenChange(false);
        toast.success(t("reservations.toast.updated"));
      },
      onError: (error) => {
        toast.error(error instanceof Error ? error.message : t("reservations.toast.updateError"));
      },
    },
  });

  const isPending = createMutation.isPending || updateMutation.isPending;

  function onSubmitCreate(values: CreateFormValues) {
    createMutation.mutate({ data: values });
  }

  function onSubmitUpdate(values: UpdateFormValues) {
    if (reservation?.id) {
      updateMutation.mutate({ id: reservation.id, data: values });
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>
            {isEditing ? t("reservations.form.editTitle") : t("reservations.form.createTitle")}
          </DialogTitle>
          <DialogDescription>
            {isEditing ? t("reservations.form.editDescription") : t("reservations.form.createDescription")}
          </DialogDescription>
        </DialogHeader>

        {isEditing ? (
          <Form {...updateForm}>
            <form onSubmit={updateForm.handleSubmit(onSubmitUpdate)}>
              <ReservationFormFields
                form={updateForm}
                factories={allFactories}
                personnel={filteredPersonnel}
                selectedFactoryId={selectedFactoryId}
                onFactoryChange={(id) => {
                  setSelectedFactoryId(id);
                  updateForm.setValue("personIds", []);
                }}
                t={t}
              />
              <DialogFooter className="mt-4">
                <Button type="button" variant="outline" onClick={() => onOpenChange(false)} disabled={isPending}>
                  {t("common.cancel")}
                </Button>
                <Button type="submit" disabled={isPending}>
                  {isPending ? t("common.saving") : t("common.save")}
                </Button>
              </DialogFooter>
            </form>
          </Form>
        ) : (
          <Form {...createForm}>
            <form onSubmit={createForm.handleSubmit(onSubmitCreate)}>
              <ReservationFormFields
                form={createForm}
                factories={allFactories}
                personnel={filteredPersonnel}
                selectedFactoryId={selectedFactoryId}
                onFactoryChange={(id) => {
                  setSelectedFactoryId(id);
                  createForm.setValue("personIds", []);
                }}
                t={t}
              />
              <DialogFooter className="mt-4">
                <Button type="button" variant="outline" onClick={() => onOpenChange(false)} disabled={isPending}>
                  {t("common.cancel")}
                </Button>
                <Button type="submit" disabled={isPending}>
                  {isPending ? t("common.saving") : t("common.create")}
                </Button>
              </DialogFooter>
            </form>
          </Form>
        )}
      </DialogContent>
    </Dialog>
  );
}

interface ReservationFormFieldsProps {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  form: any;
  factories: { id: number; name: string }[];
  personnel: { id: number; fullName: string }[];
  selectedFactoryId: number | null;
  onFactoryChange: (id: number | null) => void;
  t: (key: string) => string;
}

function ReservationFormFields({
  form,
  factories,
  personnel,
  selectedFactoryId,
  onFactoryChange,
  t,
}: ReservationFormFieldsProps) {
  return (
    <div className="grid gap-4 py-4">
      <FormField
        control={form.control}
        name="factoryId"
        render={({ field }: { field: { value: number; onChange: (v: number) => void } }) => (
          <FormItem>
            <FormLabel>{t("reservations.form.factoryLabel")}</FormLabel>
            <Select
              value={field.value ? String(field.value) : ""}
              onValueChange={(v) => {
                const id = Number(v);
                field.onChange(id);
                onFactoryChange(id || null);
              }}
            >
              <FormControl>
                <SelectTrigger>
                  <SelectValue placeholder={t("reservations.form.factoryPlaceholder")} />
                </SelectTrigger>
              </FormControl>
              <SelectContent>
                {factories.map((f) => (
                  <SelectItem key={f.id} value={String(f.id)}>
                    {f.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FormMessage />
          </FormItem>
        )}
      />

      <div className="grid grid-cols-2 gap-4">
        <FormField
          control={form.control}
          name="startTime"
          render={({ field }: { field: { value: string; onChange: (v: string) => void } }) => (
            <FormItem>
              <FormLabel>{t("reservations.form.startTimeLabel")}</FormLabel>
              <FormControl>
                <Input
                  type="datetime-local"
                  value={field.value ? new Date(field.value).toISOString().slice(0, 16) : ""}
                  onChange={(e) => {
                    const val = e.target.value;
                    field.onChange(val ? new Date(val).toISOString() : "");
                  }}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="endTime"
          render={({ field }: { field: { value: string; onChange: (v: string) => void } }) => (
            <FormItem>
              <FormLabel>{t("reservations.form.endTimeLabel")}</FormLabel>
              <FormControl>
                <Input
                  type="datetime-local"
                  value={field.value ? new Date(field.value).toISOString().slice(0, 16) : ""}
                  onChange={(e) => {
                    const val = e.target.value;
                    field.onChange(val ? new Date(val).toISOString() : "");
                  }}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
      </div>

      <FormField
        control={form.control}
        name="personIds"
        render={({ field }: { field: { value: number[]; onChange: (v: number[]) => void } }) => (
          <FormItem>
            <FormLabel>{t("reservations.form.personnelLabel")}</FormLabel>
            {!selectedFactoryId ? (
              <p className="text-sm text-muted-foreground">{t("reservations.form.selectFactoryFirst")}</p>
            ) : personnel.length === 0 ? (
              <p className="text-sm text-muted-foreground">{t("reservations.form.noPersonnelForFactory")}</p>
            ) : (
              <div className="flex flex-wrap gap-2 mt-1">
                {personnel.map((person) => {
                  const selected = field.value.includes(person.id);
                  return (
                    <Badge
                      key={person.id}
                      className={`cursor-pointer select-none ${selected ? "bg-primary text-primary-foreground" : "bg-muted text-muted-foreground hover:bg-muted/80"}`}
                      onClick={() => {
                        if (selected) {
                          field.onChange(field.value.filter((id: number) => id !== person.id));
                        } else {
                          field.onChange([...field.value, person.id]);
                        }
                      }}
                    >
                      {person.fullName}
                    </Badge>
                  );
                })}
              </div>
            )}
            <FormMessage />
          </FormItem>
        )}
      />
    </div>
  );
}
