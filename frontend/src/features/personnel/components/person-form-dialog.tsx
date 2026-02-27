import { useEffect } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { z } from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import type { PersonDto } from "@/api/generated/models";
import {
  usePostApiPersonnelWithJson,
  usePutApiPersonnelIdWithJson,
  getGetApiPersonnelQueryKey,
  getGetApiPersonnelAllQueryKey,
} from "@/api/generated/personnel/personnel";
import {
  PostApiPersonnelWithJsonBody,
  PutApiPersonnelIdWithJsonBody,
} from "@/api/generated/personnel/personnel.zod";
import { useGetApiFactoriesAll } from "@/api/generated/factories/factories";
import { getGetApiReservationsQueryKey } from "@/api/generated/reservations/reservations";
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
import { Badge } from "@/components/ui/badge";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

type CreateFormValues = z.infer<typeof PostApiPersonnelWithJsonBody>;
type UpdateFormValues = z.infer<typeof PutApiPersonnelIdWithJsonBody>;

interface PersonFormDialogProps {
  person?: PersonDto | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function PersonFormDialog({
  person,
  open,
  onOpenChange,
}: PersonFormDialogProps) {
  const { t } = useTranslation();
  const isEditing = !!person;
  const queryClient = useQueryClient();

  const { data: factoriesResponse } = useGetApiFactoriesAll();
  const factories = factoriesResponse?.data?.data ?? [];

  const defaultValues = {
    personalId: "",
    fullName: "",
    email: "",
    allowedFactoryIds: [] as number[],
  };

  const createForm = useForm<CreateFormValues>({
    resolver: zodResolver(PostApiPersonnelWithJsonBody),
    defaultValues,
  });

  const updateForm = useForm<UpdateFormValues>({
    resolver: zodResolver(PutApiPersonnelIdWithJsonBody),
    defaultValues: {
      personalId: person?.personalId ?? "",
      fullName: person?.fullName ?? "",
      email: person?.email ?? "",
      allowedFactoryIds: person?.allowedFactories?.map((f) => f.id) ?? [],
    },
  });

  useEffect(() => {
    if (open) {
      if (isEditing && person) {
        updateForm.reset({
          personalId: person.personalId,
          fullName: person.fullName,
          email: person.email,
          allowedFactoryIds: person.allowedFactories?.map((f) => f.id) ?? [],
        });
      } else {
        createForm.reset(defaultValues);
      }
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, person, isEditing]);

  const createMutation = usePostApiPersonnelWithJson({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiPersonnelQueryKey() });
        queryClient.invalidateQueries({ queryKey: getGetApiPersonnelAllQueryKey() });
        onOpenChange(false);
        toast.success(t("personnel.toast.created"));
      },
      onError: (error) => {
        toast.error(error instanceof Error ? error.message : t("personnel.toast.createError"));
      },
    },
  });

  const updateMutation = usePutApiPersonnelIdWithJson({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiPersonnelQueryKey() });
        queryClient.invalidateQueries({ queryKey: getGetApiPersonnelAllQueryKey() });
        queryClient.invalidateQueries({ queryKey: getGetApiReservationsQueryKey() });
        onOpenChange(false);
        toast.success(t("personnel.toast.updated"));
      },
      onError: (error) => {
        toast.error(error instanceof Error ? error.message : t("personnel.toast.updateError"));
      },
    },
  });

  const isPending = createMutation.isPending || updateMutation.isPending;

  function onSubmitCreate(values: CreateFormValues) {
    createMutation.mutate({ data: values });
  }

  function onSubmitUpdate(values: UpdateFormValues) {
    if (person?.id) {
      updateMutation.mutate({ id: person.id, data: values });
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>
            {isEditing ? t("personnel.form.editTitle") : t("personnel.form.createTitle")}
          </DialogTitle>
          <DialogDescription>
            {isEditing ? t("personnel.form.editDescription") : t("personnel.form.createDescription")}
          </DialogDescription>
        </DialogHeader>

        {isEditing ? (
          <Form {...updateForm}>
            <form onSubmit={updateForm.handleSubmit(onSubmitUpdate)}>
              <PersonFormFields form={updateForm} factories={factories} t={t} />
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
              <PersonFormFields form={createForm} factories={factories} t={t} />
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

interface PersonFormFieldsProps {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  form: any;
  factories: { id: number; name: string }[];
  t: (key: string) => string;
}

function PersonFormFields({ form, factories, t }: PersonFormFieldsProps) {
  return (
    <div className="grid gap-4 py-4">
      <FormField
        control={form.control}
        name="personalId"
        render={({ field }: { field: React.InputHTMLAttributes<HTMLInputElement> }) => (
          <FormItem>
            <FormLabel>{t("personnel.form.personalIdLabel")}</FormLabel>
            <FormControl>
              <Input placeholder={t("personnel.form.personalIdPlaceholder")} {...field} />
            </FormControl>
            <FormMessage />
          </FormItem>
        )}
      />

      <FormField
        control={form.control}
        name="fullName"
        render={({ field }: { field: React.InputHTMLAttributes<HTMLInputElement> }) => (
          <FormItem>
            <FormLabel>{t("personnel.form.fullNameLabel")}</FormLabel>
            <FormControl>
              <Input placeholder={t("personnel.form.fullNamePlaceholder")} {...field} />
            </FormControl>
            <FormMessage />
          </FormItem>
        )}
      />

      <FormField
        control={form.control}
        name="email"
        render={({ field }: { field: React.InputHTMLAttributes<HTMLInputElement> }) => (
          <FormItem>
            <FormLabel>{t("personnel.form.emailLabel")}</FormLabel>
            <FormControl>
              <Input type="email" placeholder={t("personnel.form.emailPlaceholder")} {...field} />
            </FormControl>
            <FormMessage />
          </FormItem>
        )}
      />

      <FormField
        control={form.control}
        name="allowedFactoryIds"
        render={({ field }: { field: { value: number[]; onChange: (v: number[]) => void } }) => (
          <FormItem>
            <FormLabel>{t("personnel.form.allowedFactoriesLabel")}</FormLabel>
            <div className="flex flex-wrap gap-2 mt-1">
              {factories.map((factory) => {
                const selected = field.value.includes(factory.id);
                return (
                  <Badge
                    key={factory.id}
                    className={`cursor-pointer select-none ${selected ? "bg-primary text-primary-foreground" : "bg-muted text-muted-foreground hover:bg-muted/80"}`}
                    onClick={() => {
                      if (selected) {
                        field.onChange(field.value.filter((id: number) => id !== factory.id));
                      } else {
                        field.onChange([...field.value, factory.id]);
                      }
                    }}
                  >
                    {factory.name}
                  </Badge>
                );
              })}
              {factories.length === 0 && (
                <span className="text-sm text-muted-foreground">{t("personnel.form.noFactories")}</span>
              )}
            </div>
            <FormMessage />
          </FormItem>
        )}
      />
    </div>
  );
}
