import { useEffect } from "react";
import { useQueryClient } from "@tanstack/react-query";
import {
  usePostApiTodosWithJson,
  usePutApiTodosIdWithJson,
  getGetApiTodosQueryKey,
} from "@/api/generated/todos/todos";
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
import { useTodoForm, type TodoFormValues, type TodoDto } from "../hooks";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

interface TodoFormDialogProps {
  todo?: TodoDto | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function TodoFormDialog({
  todo,
  open,
  onOpenChange,
}: TodoFormDialogProps) {
  const { t } = useTranslation();
  const isEditing = !!todo;
  const form = useTodoForm(todo);
  const queryClient = useQueryClient();

  useEffect(() => {
    if (open) {
      form.reset({
        title: todo?.title ?? "",
        description: todo?.description ?? "",
        dueDate: todo?.dueDate ?? null,
      });
    }
  }, [open, todo, form]);

  const createMutation = usePostApiTodosWithJson({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiTodosQueryKey(),
        });
        onOpenChange(false);
        toast.success(t("todos.toast.created"));
      },
      onError: (error) => {
        toast.error(
          error instanceof Error ? error.message : t("todos.toast.createError")
        );
      },
    },
  });

  const updateMutation = usePutApiTodosIdWithJson({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiTodosQueryKey(),
        });
        onOpenChange(false);
        toast.success(t("todos.toast.updated"));
      },
      onError: (error) => {
        toast.error(
          error instanceof Error ? error.message : t("todos.toast.updateError")
        );
      },
    },
  });

  const isPending = createMutation.isPending || updateMutation.isPending;

  function onSubmit(values: TodoFormValues) {
    if (isEditing && todo?.id) {
      updateMutation.mutate({ id: todo.id, data: values });
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
                  ? t("todos.form.editTitle")
                  : t("todos.form.createTitle")}
              </DialogTitle>
              <DialogDescription>
                {isEditing
                  ? t("todos.form.editDescription")
                  : t("todos.form.createDescription")}
              </DialogDescription>
            </DialogHeader>
            <div className="grid gap-4 py-4">
              <FormField
                control={form.control}
                name="title"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t("todos.form.titleLabel")}</FormLabel>
                    <FormControl>
                      <Input
                        placeholder={t("todos.form.titlePlaceholder")}
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="description"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t("todos.form.descriptionLabel")}</FormLabel>
                    <FormControl>
                      <Textarea
                        placeholder={t("todos.form.descriptionPlaceholder")}
                        rows={3}
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
                name="dueDate"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t("todos.form.dueDateLabel")}</FormLabel>
                    <FormControl>
                      <Input
                        type="datetime-local"
                        {...field}
                        value={
                          field.value
                            ? new Date(field.value).toISOString().slice(0, 16)
                            : ""
                        }
                        onChange={(e) => {
                          const val = e.target.value;
                          field.onChange(
                            val ? new Date(val).toISOString() : null
                          );
                        }}
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
