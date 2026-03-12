import { configureStore } from "@reduxjs/toolkit";
import featuresReducer from "@/features/_template-feature/store/features-slice";

export const store = configureStore({
  reducer: {
    features: featuresReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
