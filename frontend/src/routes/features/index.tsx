import { createFileRoute } from "@tanstack/react-router";
import { FeaturesPage } from "@/features/_template-feature";

export const Route = createFileRoute("/features/")({
  component: FeaturesPage,
});
