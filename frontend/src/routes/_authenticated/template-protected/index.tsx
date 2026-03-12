import { createFileRoute } from "@tanstack/react-router";

// Template for protected routes.
// To create a new protected route:
// 1. Copy this folder to `src/routes/_authenticated/your-route/`
// 2. Update the route path in createFileRoute()
// 3. Replace the component with your actual page

function TemplateProtectedPage() {
  return (
    <div className="flex flex-col items-center justify-center min-h-[60vh] gap-3 text-center">
      <h1 className="text-2xl font-semibold">Protected Route</h1>
      <p className="text-muted-foreground max-w-sm">
        This route is protected. Only authenticated users can see this page.
      </p>
    </div>
  );
}

export const Route = createFileRoute("/_authenticated/template-protected/")({
  component: TemplateProtectedPage,
});
