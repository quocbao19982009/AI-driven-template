import { useEffect } from "react";
import { useQueryClient } from "@tanstack/react-query";
import type { ExpenseTrackerDto } from "@/api/generated/models";
import {
  usePostApiExpenseTrackers,
  usePutApiExpenseTrackersId,
  getGetApiExpenseTrackersQueryKey,
} from "@/api/generated/expense-trackers/expense-trackers";
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useExpenseTrackerForm } from "../hooks/use-expense-tracker-form";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

const CATEGORIES = ["Food", "Transport", "Utilities", "Entertainment", "Other"];

interface ExpenseTrackerFormDialogProps {
  expense?: ExpenseTrackerDto | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function ExpenseTrackerFormDialog({
  expense,
  open,
  onOpenChange,
}: ExpenseTrackerFormDialogProps) {
  const { t } = useTranslation();
  const isEditing = !!expense;
  const form = useExpenseTrackerForm(expense);
  const queryClient = useQueryClient();

  useEffect(() => {
    if (open) {
      form.reset({
        amount: expense?.amount ?? 0,
        category: expense?.category ?? "",
        description: expense?.description ?? "",
        date: expense?.date
          ? new Date(expense.date).toISOString().split("T")[0]
          : "",
      });
    }
  }, [open, expense, form]);

  const createMutation = usePostApiExpenseTrackers({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiExpenseTrackersQueryKey(),
        });
        onOpenChange(false);
        toast.success(t("expenseTrackers.toast.created"));
      },
      onError: (error) => {
        toast.error(
          error instanceof Error
            ? error.message
            : t("expenseTrackers.toast.createError")
        );
      },
    },
  });

  const updateMutation = usePutApiExpenseTrackersId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiExpenseTrackersQueryKey(),
        });
        onOpenChange(false);
        toast.success(t("expenseTrackers.toast.updated"));
      },
      onError: (error) => {
        toast.error(
          error instanceof Error
            ? error.message
            : t("expenseTrackers.toast.updateError")
        );
      },
    },
  });

  const isPending = createMutation.isPending || updateMutation.isPending;

  function onSubmit(values: Record<string, unknown>) {
    const payload = {
      amount: Number(values.amount),
      category: String(values.category),
      description: values.description ? String(values.description) : null,
      date: new Date(String(values.date)).toISOString(),
    };

    if (isEditing && expense?.id) {
      updateMutation.mutate({ id: expense.id, data: payload });
    } else {
      createMutation.mutate({ data: payload });
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
                  ? t("expenseTrackers.form.editTitle")
                  : t("expenseTrackers.form.createTitle")}
              </DialogTitle>
              <DialogDescription>
                {isEditing
                  ? t("expenseTrackers.form.editDescription")
                  : t("expenseTrackers.form.createDescription")}
              </DialogDescription>
            </DialogHeader>
            <div className="grid gap-4 py-4">
              <FormField
                control={form.control}
                name="amount"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>
                      {t("expenseTrackers.form.amountLabel")}
                    </FormLabel>
                    <FormControl>
                      <Input
                        type="number"
                        step="0.01"
                        min="0.01"
                        max="999999.99"
                        placeholder={t(
                          "expenseTrackers.form.amountPlaceholder"
                        )}
                        {...field}
                        onChange={(e) =>
                          field.onChange(parseFloat(e.target.value) || 0)
                        }
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="category"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>
                      {t("expenseTrackers.form.categoryLabel")}
                    </FormLabel>
                    <Select
                      onValueChange={field.onChange}
                      defaultValue={field.value}
                      value={field.value}
                    >
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue
                            placeholder={t(
                              "expenseTrackers.form.categoryPlaceholder"
                            )}
                          />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {CATEGORIES.map((cat) => (
                          <SelectItem key={cat} value={cat}>
                            {t(`expenseTrackers.filter.${cat.toLowerCase()}`)}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="description"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>
                      {t("expenseTrackers.form.descriptionLabel")}
                    </FormLabel>
                    <FormControl>
                      <Textarea
                        placeholder={t(
                          "expenseTrackers.form.descriptionPlaceholder"
                        )}
                        {...field}
                        value={field.value ?? ""}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="date"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t("expenseTrackers.form.dateLabel")}</FormLabel>
                    <FormControl>
                      <Input type="date" {...field} />
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

