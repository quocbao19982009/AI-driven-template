import { createFileRoute } from "@tanstack/react-router";
import { ExpenseTrackersPage } from "@/features/expense-trackers/components/expense-trackers-page";

export const Route = createFileRoute("/expense-trackers/")({
  component: ExpenseTrackersPage,
});

