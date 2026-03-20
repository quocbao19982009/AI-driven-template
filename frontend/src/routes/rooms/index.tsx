import { createFileRoute } from "@tanstack/react-router";
import { RoomsPage } from "@/features/rooms/components/rooms-page";

export const Route = createFileRoute("/rooms/")({
  component: RoomsPage,
});
