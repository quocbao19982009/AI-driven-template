import { useEffect } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { z } from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import type { FlashcardDto } from "@/api/generated/models";
import {
  usePostApiFlashcardsWithJson,
  usePutApiFlashcardsIdWithJson,
  getGetApiFlashcardsQueryKey,
} from "@/api/generated/flashcards/flashcards";
import {
  PostApiFlashcardsWithJsonBody,
  PutApiFlashcardsIdWithJsonBody,
} from "@/api/generated/flashcards/flashcards.zod";
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

type CreateFormValues = z.infer<typeof PostApiFlashcardsWithJsonBody>;
type UpdateFormValues = z.infer<typeof PutApiFlashcardsIdWithJsonBody>;

interface FlashcardFormDialogProps {
  flashcard?: FlashcardDto | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  categories: string[];
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

  const createForm = useForm<CreateFormValues>({
    resolver: zodResolver(PostApiFlashcardsWithJsonBody),
    defaultValues: { finnishWord: "", englishTranslation: "", category: "" },
  });

  const updateForm = useForm<UpdateFormValues>({
    resolver: zodResolver(PutApiFlashcardsIdWithJsonBody),
    defaultValues: {
      finnishWord: flashcard?.finnishWord ?? "",
      englishTranslation: flashcard?.englishTranslation ?? "",
      category: flashcard?.category ?? "",
    },
  });

  useEffect(() => {
    if (open) {
      if (isEditing && flashcard) {
        updateForm.reset({
          finnishWord: flashcard.finnishWord,
          englishTranslation: flashcard.englishTranslation,
          category: flashcard.category,
        });
      } else {
        createForm.reset({
          finnishWord: "",
          englishTranslation: "",
          category: "",
        });
      }
    }
  }, [open, flashcard, isEditing, createForm, updateForm]);

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

  function onSubmitCreate(values: CreateFormValues) {
    createMutation.mutate({ data: values });
  }

  function onSubmitUpdate(values: UpdateFormValues) {
    if (flashcard?.id) {
      updateMutation.mutate({ id: flashcard.id, data: values });
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

        {isEditing ? (
          <Form {...updateForm}>
            <form onSubmit={updateForm.handleSubmit(onSubmitUpdate)}>
              <FlashcardFormFields
                form={updateForm}
                t={t}
                categories={categories}
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
              <FlashcardFormFields
                form={createForm}
                t={t}
                categories={categories}
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

interface FlashcardFormFieldsProps {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  form: any;
  t: (key: string) => string;
  categories: string[];
}

function FlashcardFormFields({
  form,
  t,
  categories,
}: FlashcardFormFieldsProps) {
  return (
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
            <FormLabel>{t("flashcards.form.finnishWordLabel")}</FormLabel>
            <FormControl>
              <Input
                placeholder={t("flashcards.form.finnishWordPlaceholder")}
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
        name="category"
        render={({
          field,
        }: {
          field: React.InputHTMLAttributes<HTMLInputElement>;
        }) => (
          <FormItem>
            <FormLabel>{t("flashcards.form.categoryLabel")}</FormLabel>
            <FormControl>
              <>
                <Input
                  placeholder={t("flashcards.form.categoryPlaceholder")}
                  list="flashcard-categories"
                  {...field}
                />
                <datalist id="flashcard-categories">
                  {categories.map((cat) => (
                    <option key={cat} value={cat} />
                  ))}
                </datalist>
              </>
            </FormControl>
            <FormMessage />
          </FormItem>
        )}
      />
    </div>
  );
}
