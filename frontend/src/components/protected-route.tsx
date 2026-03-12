import { useEffect } from "react";
import { useNavigate, useLocation, Outlet } from "@tanstack/react-router";
import { useAuth } from "@/auth/auth-context";

/**
 * Wraps protected routes. Redirects unauthenticated users to /login?returnUrl=<current-path>.
 * Renders children (via <Outlet />) for authenticated users.
 */
export function ProtectedRoute() {
  const { accessToken, isLoading } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    if (!isLoading && !accessToken) {
      void navigate({
        to: "/login",
        search: { returnUrl: location.pathname },
      });
    }
  }, [isLoading, accessToken, navigate, location.pathname]);

  // While the silent-refresh check runs, render nothing
  if (isLoading) {
    return null;
  }

  // Not authenticated — redirect is in flight
  if (!accessToken) {
    return null;
  }

  return <Outlet />;
}
