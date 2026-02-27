import { createFileRoute } from "@tanstack/react-router";
import { FactoriesPage } from "@/features/factories";

export const Route = createFileRoute("/factories/")({
  component: FactoriesPage,
});
