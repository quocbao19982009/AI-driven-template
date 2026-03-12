import {
  useState,
  useEffect,
  useCallback,
  useRef,
  type ReactNode,
} from "react";
import { AuthContext } from "./auth-context";
import type { AuthContextValue } from "./auth-context";
import type { MeDto } from "@/api/generated/models";
import { postApiAuthRefresh, getApiAuthMe } from "@/api/generated/auth/auth";
import { setApiFetchToken, setOnAuthFailure } from "@/api/mutator/apiFetch";
import { useAppDispatch } from "@/store/hooks";
import { setUser, clearUser } from "@/features/auth/store/auth-slice";

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [accessToken, setAccessToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const didSilentRefresh = useRef(false);
  const dispatch = useAppDispatch();

  // Silent refresh on mount — tries to get a new access token using the HttpOnly cookie
  useEffect(() => {
    if (didSilentRefresh.current) return;
    didSilentRefresh.current = true;

    postApiAuthRefresh()
      .then(async (res) => {
        const token = res.data?.data?.accessToken;
        if (token) {
          setAccessToken(token);
          setApiFetchToken(token);

          // Fetch user profile and store in Redux
          try {
            const meRes = await getApiAuthMe();
            const userData = meRes.data?.data ?? null;
            dispatch(setUser(userData as MeDto | null));
          } catch {
            // Token is valid but /me failed — user stays null, not critical
          }
        }
      })
      .catch(() => {
        // 401 means no valid cookie — treat as logged out, no action needed
      })
      .finally(() => {
        setIsLoading(false);
      });
  }, [dispatch]);

  const login = useCallback((token: string) => {
    setAccessToken(token);
    setApiFetchToken(token);
  }, []);

  const logout = useCallback(() => {
    setAccessToken(null);
    setApiFetchToken(null);
    dispatch(clearUser());
  }, [dispatch]);

  // Register auth failure callback so apiFetch can trigger logout on refresh failure
  useEffect(() => {
    setOnAuthFailure(() => logout());
    return () => setOnAuthFailure(null);
  }, [logout]);

  const value: AuthContextValue = {
    accessToken,
    isLoading,
    login,
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
