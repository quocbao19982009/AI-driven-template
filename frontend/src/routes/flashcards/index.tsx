import { createFileRoute } from "@tanstack/react-router";
import { FlashcardsPage } from "@/features/flashcards";

export const Route = createFileRoute("/flashcards/")({
  component: FlashcardsPage,
});
