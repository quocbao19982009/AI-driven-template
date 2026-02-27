import { createFileRoute } from "@tanstack/react-router";
import { SchedulingPage } from "@/features/scheduling";

export const Route = createFileRoute("/scheduling/")({
  component: SchedulingPage,
});
