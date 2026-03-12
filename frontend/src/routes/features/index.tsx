import { createFileRoute } from "@tanstack/react-router";
import { FeaturesPage } from "@/features/_template-feature/components/features-page";

export const Route = createFileRoute("/features/")({
  component: FeaturesPage,
});
