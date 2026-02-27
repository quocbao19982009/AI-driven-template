import { createFileRoute } from "@tanstack/react-router";
import { ReservationsPage } from "@/features/reservations";

export const Route = createFileRoute("/reservations/")({
  component: ReservationsPage,
});
