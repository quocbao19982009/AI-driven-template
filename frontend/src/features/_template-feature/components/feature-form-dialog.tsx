import { useEffect } from "react";
import { useQueryClient } from "@tanstack/react-query";
import type { FeatureDto } from "@/api/generated/models";
import {
  usePostApiFeaturesWithJson,
  usePutApiFeaturesIdWithJson,
  getGetApiFeaturesQueryKey,
} from "@/api/generated/feature/feature";
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
import { useFeatureForm } from "../hooks/use-feature-form";
import type { CreateFeatureRequest } from "@/api/generated/models";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

interface FeatureFormDialogProps {
  feature?: FeatureDto | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function FeatureFormDialog({
  feature,
  open,
  onOpenChange,
}: FeatureFormDialogProps) {
  const { t } = useTranslation();
  const isEditing = !!feature;
  const form = useFeatureForm(feature);
  const queryClient = useQueryClient();

  useEffect(() => {
    if (open) {
      form.reset({ name: feature?.name ?? "" });
    }
  }, [open, feature, form]);

  const createMutation = usePostApiFeaturesWithJson({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiFeaturesQueryKey(),
        });
        onOpenChange(false);
        toast.success(t("features.toast.created"));
      },
      onError: (error) => {
        toast.error(
          error instanceof Error
            ? error.message
            : t("features.toast.createError")
        );
      },
    },
  });

  const updateMutation = usePutApiFeaturesIdWithJson({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiFeaturesQueryKey(),
        });
        onOpenChange(false);
        toast.success(t("features.toast.updated"));
      },
      onError: (error) => {
        toast.error(
          error instanceof Error
            ? error.message
            : t("features.toast.updateError")
        );
      },
    },
  });

  const isPending = createMutation.isPending || updateMutation.isPending;

  function onSubmit(values: CreateFeatureRequest) {
    if (isEditing && feature?.id) {
      updateMutation.mutate({ id: feature.id, data: values });
    } else {
      createMutation.mutate({ data: values });
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)}>
            <DialogHeader>
              <DialogTitle>
                {isEditing
                  ? t("features.form.editTitle")
                  : t("features.form.createTitle")}
              </DialogTitle>
              <DialogDescription>
                {isEditing
                  ? t("features.form.editDescription")
                  : t("features.form.createDescription")}
              </DialogDescription>
            </DialogHeader>
            <div className="grid gap-4 py-4">
              <FormField
                control={form.control}
                name="name"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t("features.form.nameLabel")}</FormLabel>
                    <FormControl>
                      <Input
                        placeholder={t("features.form.namePlaceholder")}
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
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
    </Dialog>
  );
}
