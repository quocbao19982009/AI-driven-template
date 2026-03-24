import { configureStore } from "@reduxjs/toolkit";
import featuresReducer from "@/features/_template-feature/store/features-slice";
import authReducer from "@/features/auth/store/auth-slice";
import roomsReducer from "@/features/rooms/store/rooms-slice";
import bookingsReducer from "@/features/bookings/store/bookings-slice";

export const store = configureStore({
  reducer: {
    features: featuresReducer,
    auth: authReducer,
    rooms: roomsReducer,
    bookings: bookingsReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
