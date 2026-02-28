import { useEffect } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { z } from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import type { FlashcardDto, FlashcardCategoryDto } from "@/api/generated/models";
import {
  usePostApiFlashcardsWithJson,
  usePutApiFlashcardsIdWithJson,
  getGetApiFlashcardsQueryKey,
} from "@/api/generated/flashcards/flashcards";
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

const flashcardFormSchema = z.object({
  finnishWord: z.string().min(1),
  englishTranslation: z.string().min(1),
  categoryId: z.number().nullable().optional(),
});

type FormValues = z.infer<typeof flashcardFormSchema>;

interface FlashcardFormDialogProps {
  flashcard?: FlashcardDto | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  categories: FlashcardCategoryDto[];
}

export function FlashcardFormDialog({
  flashcard,
  open,
  onOpenChange,
  categories,
}: FlashcardFormDialogProps) {
  const { t } = useTranslation();
  const isEditing = !!flashcard;
  const queryClient = useQueryClient();

  const form = useForm<FormValues>({
    resolver: zodResolver(flashcardFormSchema),
    defaultValues: {
      finnishWord: "",
      englishTranslation: "",
      categoryId: null,
    },
  });

  useEffect(() => {
    if (open) {
      if (isEditing && flashcard) {
        form.reset({
          finnishWord: flashcard.finnishWord,
          englishTranslation: flashcard.englishTranslation,
          categoryId: flashcard.categoryId ?? null,
        });
      } else {
        form.reset({
          finnishWord: "",
          englishTranslation: "",
          categoryId: null,
        });
      }
    }
  }, [open, flashcard, isEditing, form]);

  const createMutation = usePostApiFlashcardsWithJson({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiFlashcardsQueryKey(),
        });
        onOpenChange(false);
        toast.success(t("flashcards.toast.created"));
      },
      onError: (error) => {
        toast.error(
          error instanceof Error
            ? error.message
            : t("flashcards.toast.createError"),
        );
      },
    },
  });

  const updateMutation = usePutApiFlashcardsIdWithJson({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiFlashcardsQueryKey(),
        });
        onOpenChange(false);
        toast.success(t("flashcards.toast.updated"));
      },
      onError: (error) => {
        toast.error(
          error instanceof Error
            ? error.message
            : t("flashcards.toast.updateError"),
        );
      },
    },
  });

  const isPending = createMutation.isPending || updateMutation.isPending;

  function onSubmit(values: FormValues) {
    const payload = {
      finnishWord: values.finnishWord,
      englishTranslation: values.englishTranslation,
      categoryId: values.categoryId,
    };

    if (isEditing && flashcard?.id) {
      updateMutation.mutate({ id: flashcard.id, data: payload });
    } else {
      createMutation.mutate({ data: payload });
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>
            {isEditing
              ? t("flashcards.form.editTitle")
              : t("flashcards.form.createTitle")}
          </DialogTitle>
          <DialogDescription>
            {isEditing
              ? t("flashcards.form.editDescription")
              : t("flashcards.form.createDescription")}
          </DialogDescription>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)}>
            <div className="grid gap-4 py-4">
              <FormField
                control={form.control}
                name="finnishWord"
                render={({
                  field,
                }: {
                  field: React.InputHTMLAttributes<HTMLInputElement>;
                }) => (
                  <FormItem>
                    <FormLabel>
                      {t("flashcards.form.finnishWordLabel")}
                    </FormLabel>
                    <FormControl>
                      <Input
                        placeholder={t(
                          "flashcards.form.finnishWordPlaceholder",
                        )}
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="englishTranslation"
                render={({
                  field,
                }: {
                  field: React.InputHTMLAttributes<HTMLInputElement>;
                }) => (
                  <FormItem>
                    <FormLabel>
                      {t("flashcards.form.englishTranslationLabel")}
                    </FormLabel>
                    <FormControl>
                      <Input
                        placeholder={t(
                          "flashcards.form.englishTranslationPlaceholder",
                        )}
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="categoryId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t("flashcards.form.categoryLabel")}</FormLabel>
                    <FormControl>
                      <select
                        className="flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
                        value={field.value ?? ""}
                        onChange={(e) => {
                          const val = e.target.value;
                          field.onChange(val ? parseInt(val, 10) : null);
                        }}
                      >
                        <option value="">
                          {t("flashcards.form.categoryPlaceholder")}
                        </option>
                        {categories.map((cat) => (
                          <option key={cat.id} value={cat.id}>
                            {cat.name}
                          </option>
                        ))}
                      </select>
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>
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
