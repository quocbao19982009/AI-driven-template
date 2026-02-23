import { useEffect } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { z } from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import type { TodoDto } from "@/api/generated/models";
import {
  usePostApiTodosWithJson,
  usePutApiTodosIdWithJson,
  getGetApiTodosQueryKey,
} from "@/api/generated/todos/todos";
import {
  PostApiTodosWithJsonBody,
  PutApiTodosIdWithJsonBody,
} from "@/api/generated/todos/todos.zod";
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
import { Textarea } from "@/components/ui/textarea";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

type CreateFormValues = z.infer<typeof PostApiTodosWithJsonBody>;
type UpdateFormValues = z.infer<typeof PutApiTodosIdWithJsonBody>;

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
  const queryClient = useQueryClient();

  const createForm = useForm<CreateFormValues>({
    resolver: zodResolver(PostApiTodosWithJsonBody),
    defaultValues: {
      title: "",
      description: "",
      dueDate: undefined,
      priority: 1,
    },
  });

  const updateForm = useForm<UpdateFormValues>({
    resolver: zodResolver(PutApiTodosIdWithJsonBody),
    defaultValues: {
      title: todo?.title ?? "",
      description: todo?.description ?? "",
      dueDate: todo?.dueDate ?? undefined,
      priority: todo?.priority ?? 1,
      isCompleted: todo?.isCompleted ?? false,
    },
  });

  useEffect(() => {
    if (open) {
      if (isEditing && todo) {
        updateForm.reset({
          title: todo.title,
          description: todo.description ?? "",
          dueDate: todo.dueDate ?? undefined,
          priority: todo.priority,
          isCompleted: todo.isCompleted,
        });
      } else {
        createForm.reset({
          title: "",
          description: "",
          dueDate: undefined,
          priority: 1,
        });
      }
    }
  }, [open, todo, isEditing, createForm, updateForm]);

  const createMutation = usePostApiTodosWithJson({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiTodosQueryKey() });
        onOpenChange(false);
        toast.success(t("todos.toast.created"));
      },
      onError: (error) => {
        toast.error(
          error instanceof Error ? error.message : t("todos.toast.createError"),
        );
      },
    },
  });

  const updateMutation = usePutApiTodosIdWithJson({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiTodosQueryKey() });
        onOpenChange(false);
        toast.success(t("todos.toast.updated"));
      },
      onError: (error) => {
        toast.error(
          error instanceof Error ? error.message : t("todos.toast.updateError"),
        );
      },
    },
  });

  const isPending = createMutation.isPending || updateMutation.isPending;

  function onSubmitCreate(values: CreateFormValues) {
    createMutation.mutate({ data: values });
  }

  function onSubmitUpdate(values: UpdateFormValues) {
    if (todo?.id) {
      updateMutation.mutate({ id: todo.id, data: values });
    }
  }

  const priorityOptions = [
    { value: "0", label: t("todos.priority.low") },
    { value: "1", label: t("todos.priority.medium") },
    { value: "2", label: t("todos.priority.high") },
  ];

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>
            {isEditing ? t("todos.form.editTitle") : t("todos.form.createTitle")}
          </DialogTitle>
          <DialogDescription>
            {isEditing
              ? t("todos.form.editDescription")
              : t("todos.form.createDescription")}
          </DialogDescription>
        </DialogHeader>

        {isEditing ? (
          <Form {...updateForm}>
            <form onSubmit={updateForm.handleSubmit(onSubmitUpdate)}>
              <TodoFormFields
                form={updateForm}
                priorityOptions={priorityOptions}
                t={t}
              />
              <DialogFooter className="mt-4">
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => onOpenChange(false)}
                  disabled={isPending}
                >
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
              <TodoFormFields
                form={createForm}
                priorityOptions={priorityOptions}
                t={t}
              />
              <DialogFooter className="mt-4">
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => onOpenChange(false)}
                  disabled={isPending}
                >
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

interface TodoFormFieldsProps {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  form: any;
  priorityOptions: { value: string; label: string }[];
  t: (key: string) => string;
}

function TodoFormFields({ form, priorityOptions, t }: TodoFormFieldsProps) {
  return (
    <div className="grid gap-4 py-4">
      <FormField
        control={form.control}
        name="title"
        render={({ field }: { field: React.InputHTMLAttributes<HTMLInputElement> }) => (
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
        render={({ field }: { field: React.TextareaHTMLAttributes<HTMLTextAreaElement> }) => (
          <FormItem>
            <FormLabel>{t("todos.form.descriptionLabel")}</FormLabel>
            <FormControl>
              <Textarea
                placeholder={t("todos.form.descriptionPlaceholder")}
                rows={3}
                {...field}
                value={field.value as string ?? ""}
              />
            </FormControl>
            <FormMessage />
          </FormItem>
        )}
      />

      <FormField
        control={form.control}
        name="priority"
        render={({ field }: { field: { value: number; onChange: (v: number) => void } }) => (
          <FormItem>
            <FormLabel>{t("todos.form.priorityLabel")}</FormLabel>
            <Select
              value={String(field.value)}
              onValueChange={(v) => field.onChange(Number(v))}
            >
              <FormControl>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
              </FormControl>
              <SelectContent>
                {priorityOptions.map((opt) => (
                  <SelectItem key={opt.value} value={opt.value}>
                    {opt.label}
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
        name="dueDate"
        render={({ field }: { field: { value: string | null | undefined; onChange: (v: string | undefined) => void } }) => (
          <FormItem>
            <FormLabel>{t("todos.form.dueDateLabel")}</FormLabel>
            <FormControl>
              <Input
                type="date"
                value={
                  field.value
                    ? new Date(field.value).toISOString().split("T")[0]
                    : ""
                }
                onChange={(e) => {
                  const val = e.target.value;
                  field.onChange(val ? new Date(val).toISOString() : undefined);
                }}
              />
            </FormControl>
            <FormMessage />
          </FormItem>
        )}
      />
    </div>
  );
}
