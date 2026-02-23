import { configureStore } from "@reduxjs/toolkit";
import { featuresReducer } from "@/features/_template-feature/store";
import { todosReducer } from "@/features/todos/store";

export const store = configureStore({
  reducer: {
    features: featuresReducer,
    todos: todosReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
