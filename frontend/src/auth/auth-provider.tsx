import { useState, useEffect, useCallback, useRef, type ReactNode } from "react";
import { AuthContext } from "./auth-context";
import type { AuthContextValue } from "./auth-context";
import type { MeDto } from "@/api/generated/models";
import { postApiAuthRefresh } from "@/api/generated/auth/auth";
import { setApiFetchToken } from "@/api/mutator/apiFetch";

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [accessToken, setAccessToken] = useState<string | null>(null);
  const [user, setUser] = useState<MeDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const didSilentRefresh = useRef(false);

  // Silent refresh on mount — tries to get a new access token using the HttpOnly cookie
  useEffect(() => {
    if (didSilentRefresh.current) return;
    didSilentRefresh.current = true;

    postApiAuthRefresh()
      .then((res) => {
        const token = res.data?.data?.accessToken;
        if (token) {
          setAccessToken(token);
          setApiFetchToken(token);
        }
      })
      .catch(() => {
        // 401 means no valid cookie — treat as logged out, no action needed
      })
      .finally(() => {
        setIsLoading(false);
      });
  }, []);

  const login = useCallback((token: string, userData: MeDto) => {
    setAccessToken(token);
    setUser(userData);
    setApiFetchToken(token);
  }, []);

  const logout = useCallback(() => {
    setAccessToken(null);
    setUser(null);
    setApiFetchToken(null);
  }, []);

  const value: AuthContextValue = {
    accessToken,
    user,
    isLoading,
    login,
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
