import { configureStore } from "@reduxjs/toolkit";
import featuresReducer from "@/features/_template-feature/store/features-slice";
import authReducer from "@/features/auth/store/auth-slice";
import expenseTrackersReducer from "@/features/expense-trackers/store/expense-trackers-slice";

export const store = configureStore({
  reducer: {
    features: featuresReducer,
    auth: authReducer,
    expenseTrackers: expenseTrackersReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
