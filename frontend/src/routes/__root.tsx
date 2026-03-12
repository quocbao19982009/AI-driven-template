import { lazy, Suspense } from "react";
import { createRootRoute } from "@tanstack/react-router";
import { AppLayout } from "@/components/layout/app-layout";
import { ErrorBoundary } from "@/components/error-boundary";

const TanStackRouterDevtools = import.meta.env.DEV
  ? lazy(() =>
      import("@tanstack/react-router-devtools").then((m) => ({
        default: m.TanStackRouterDevtools,
      }))
    )
  : () => null;

const RootLayout = () => (
  <ErrorBoundary>
    <AppLayout />
    <Suspense fallback={null}>
      <TanStackRouterDevtools />
    </Suspense>
  </ErrorBoundary>
);

export const Route = createRootRoute({ component: RootLayout });
