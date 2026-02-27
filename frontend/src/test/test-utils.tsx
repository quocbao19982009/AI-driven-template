import type { ReactElement, ReactNode } from "react";
import { render, type RenderOptions } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Provider } from "react-redux";
import { configureStore } from "@reduxjs/toolkit";
import { featuresReducer } from "@/features/_template-feature/store";
import { todosReducer } from "@/features/todos/store";
import { factoriesReducer } from "@/features/factories/store";
import { personnelReducer } from "@/features/personnel/store";
import { reservationsReducer } from "@/features/reservations/store";

function createTestQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });
}

function createTestStore() {
  return configureStore({
    reducer: {
      features: featuresReducer,
      todos: todosReducer,
      factories: factoriesReducer,
      personnel: personnelReducer,
      reservations: reservationsReducer,
    },
  });
}

function AllProviders({ children }: { children: ReactNode }) {
  const queryClient = createTestQueryClient();
  const store = createTestStore();

  return (
    <Provider store={store}>
      <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    </Provider>
  );
}

function customRender(
  ui: ReactElement,
  options?: Omit<RenderOptions, "wrapper">,
) {
  return render(ui, { wrapper: AllProviders, ...options });
}

export * from "@testing-library/react";
export { customRender as render };
