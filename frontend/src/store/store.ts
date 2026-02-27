import { configureStore } from "@reduxjs/toolkit";
import { featuresReducer } from "@/features/_template-feature/store";
import { todosReducer } from "@/features/todos/store";
import { factoriesReducer } from "@/features/factories/store";
import { personnelReducer } from "@/features/personnel/store";
import { reservationsReducer } from "@/features/reservations/store";

export const store = configureStore({
  reducer: {
    features: featuresReducer,
    todos: todosReducer,
    factories: factoriesReducer,
    personnel: personnelReducer,
    reservations: reservationsReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
