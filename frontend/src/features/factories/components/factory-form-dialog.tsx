import { useEffect } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { z } from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import type { FactoryDto } from "@/api/generated/models";
import {
  usePostApiFactoriesWithJson,
  usePutApiFactoriesIdWithJson,
  getGetApiFactoriesQueryKey,
} from "@/api/generated/factories/factories";
import {
  PostApiFactoriesWithJsonBody,
  PutApiFactoriesIdWithJsonBody,
} from "@/api/generated/factories/factories.zod";
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
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

type CreateFormValues = z.infer<typeof PostApiFactoriesWithJsonBody>;
type UpdateFormValues = z.infer<typeof PutApiFactoriesIdWithJsonBody>;

interface FactoryFormDialogProps {
  factory?: FactoryDto | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function FactoryFormDialog({
  factory,
  open,
  onOpenChange,
}: FactoryFormDialogProps) {
  const { t } = useTranslation();
  const isEditing = !!factory;
  const queryClient = useQueryClient();

  const createForm = useForm<CreateFormValues>({
    resolver: zodResolver(PostApiFactoriesWithJsonBody),
    defaultValues: { name: "", timeZone: "" },
  });

  const updateForm = useForm<UpdateFormValues>({
    resolver: zodResolver(PutApiFactoriesIdWithJsonBody),
    defaultValues: {
      name: factory?.name ?? "",
      timeZone: factory?.timeZone ?? "",
    },
  });

  useEffect(() => {
    if (open) {
      if (isEditing && factory) {
        updateForm.reset({ name: factory.name, timeZone: factory.timeZone });
      } else {
        createForm.reset({ name: "", timeZone: "" });
      }
    }
  }, [open, factory, isEditing, createForm, updateForm]);

  const createMutation = usePostApiFactoriesWithJson({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiFactoriesQueryKey() });
        onOpenChange(false);
        toast.success(t("factories.toast.created"));
      },
      onError: (error) => {
        toast.error(error instanceof Error ? error.message : t("factories.toast.createError"));
      },
    },
  });

  const updateMutation = usePutApiFactoriesIdWithJson({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiFactoriesQueryKey() });
        onOpenChange(false);
        toast.success(t("factories.toast.updated"));
      },
      onError: (error) => {
        toast.error(error instanceof Error ? error.message : t("factories.toast.updateError"));
      },
    },
  });

  const isPending = createMutation.isPending || updateMutation.isPending;

  function onSubmitCreate(values: CreateFormValues) {
    createMutation.mutate({ data: values });
  }

  function onSubmitUpdate(values: UpdateFormValues) {
    if (factory?.id) {
      updateMutation.mutate({ id: factory.id, data: values });
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>
            {isEditing ? t("factories.form.editTitle") : t("factories.form.createTitle")}
          </DialogTitle>
          <DialogDescription>
            {isEditing ? t("factories.form.editDescription") : t("factories.form.createDescription")}
          </DialogDescription>
        </DialogHeader>

        {isEditing ? (
          <Form {...updateForm}>
            <form onSubmit={updateForm.handleSubmit(onSubmitUpdate)}>
              <FactoryFormFields form={updateForm} t={t} />
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
              <FactoryFormFields form={createForm} t={t} />
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

interface FactoryFormFieldsProps {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  form: any;
  t: (key: string) => string;
}

function FactoryFormFields({ form, t }: FactoryFormFieldsProps) {
  return (
    <div className="grid gap-4 py-4">
      <FormField
        control={form.control}
        name="name"
        render={({ field }: { field: React.InputHTMLAttributes<HTMLInputElement> }) => (
          <FormItem>
            <FormLabel>{t("factories.form.nameLabel")}</FormLabel>
            <FormControl>
              <Input placeholder={t("factories.form.namePlaceholder")} {...field} />
            </FormControl>
            <FormMessage />
          </FormItem>
        )}
      />

      <FormField
        control={form.control}
        name="timeZone"
        render={({ field }: { field: React.InputHTMLAttributes<HTMLInputElement> }) => (
          <FormItem>
            <FormLabel>{t("factories.form.timeZoneLabel")}</FormLabel>
            <FormControl>
              <Input placeholder={t("factories.form.timeZonePlaceholder")} {...field} />
            </FormControl>
            <FormMessage />
          </FormItem>
        )}
      />
    </div>
  );
}
