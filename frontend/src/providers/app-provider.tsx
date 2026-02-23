import type { ReactNode } from "react";
import { QueryProvider } from "./query-provider";
import { StoreProvider } from "./store-provider";
import { I18nProvider } from "./i18n-provider";
import { Toaster } from "@/components/ui/sonner";

export function AppProvider({ children }: { children: ReactNode }) {
  return (
    <I18nProvider>
      <StoreProvider>
        <QueryProvider>
          {children}
          <Toaster />
        </QueryProvider>
      </StoreProvider>
    </I18nProvider>
  );
}
