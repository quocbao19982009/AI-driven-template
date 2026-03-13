import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useTranslation } from "react-i18next";
import { PostApiExpenseTrackersBody } from "@/api/generated/expense-trackers/expense-trackers.zod";
import type { ExpenseTrackerDto } from "@/api/generated/models";

export function useExpenseTrackerForm(expense?: ExpenseTrackerDto | null) {
  const { t } = useTranslation();

  const schema = PostApiExpenseTrackersBody.extend({
    amount: z
      .number({ error: t("expenseTrackers.validation.amountRequired") })
      .gt(0, t("expenseTrackers.validation.amountPositive"))
      .max(999999.99, t("expenseTrackers.validation.amountMax")),
    category: z
      .string()
      .min(1, t("expenseTrackers.validation.categoryRequired")),
    description: z
      .string()
      .max(500, t("expenseTrackers.validation.descriptionMax"))
      .nullish(),
    date: z.string().min(1, t("expenseTrackers.validation.dateRequired")),
  });

  return useForm({
    resolver: zodResolver(schema),
    defaultValues: {
      amount: expense?.amount ?? 0,
      category: expense?.category ?? "",
      description: expense?.description ?? "",
      date: expense?.date
        ? new Date(expense.date).toISOString().split("T")[0]
        : "",
    },
  });
}

