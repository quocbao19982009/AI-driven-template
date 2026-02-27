import { createFileRoute } from "@tanstack/react-router";
import { PersonnelPage } from "@/features/personnel";

export const Route = createFileRoute("/personnel/")({
  component: PersonnelPage,
});
