import { createFileRoute } from "@tanstack/react-router";
import { RoomsPage } from "@/features/rooms";

export const Route = createFileRoute("/rooms/")({
  component: RoomsPage,
});
