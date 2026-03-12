import type { ReactNode } from "react";
import { QueryProvider } from "./query-provider";
import { StoreProvider } from "./store-provider";
import { I18nProvider } from "./i18n-provider";
import { Toaster } from "@/components/ui/sonner";
import { AuthProvider } from "@/auth/auth-provider";

export function AppProvider({ children }: { children: ReactNode }) {
  return (
    <I18nProvider>
      <StoreProvider>
        <QueryProvider>
          <AuthProvider>
            {children}
            <Toaster />
          </AuthProvider>
        </QueryProvider>
      </StoreProvider>
    </I18nProvider>
  );
}
