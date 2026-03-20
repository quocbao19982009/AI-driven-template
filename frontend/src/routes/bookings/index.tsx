import { createFileRoute } from "@tanstack/react-router";
import { BookingsPage } from "@/features/bookings/components/bookings-page";

export const Route = createFileRoute("/bookings/")({
  component: BookingsPage,
});
