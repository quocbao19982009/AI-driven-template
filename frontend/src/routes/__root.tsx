import { createRootRoute } from "@tanstack/react-router";
import { TanStackRouterDevtools } from "@tanstack/react-router-devtools";
import { AppLayout } from "@/components/layout/app-layout";
import { ErrorBoundary } from "@/components/error-boundary";

const RootLayout = () => (
  <ErrorBoundary>
    <AppLayout />
    <TanStackRouterDevtools />
  </ErrorBoundary>
);

export const Route = createRootRoute({ component: RootLayout });
