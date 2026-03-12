import { createContext, useContext } from "react";
import type { MeDto } from "@/api/generated/models";

export interface AuthContextValue {
  accessToken: string | null;
  user: MeDto | null;
  isLoading: boolean;
  login: (token: string, user: MeDto) => void;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextValue | null>(null);

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth must be used within AuthProvider");
  }
  return ctx;
}
