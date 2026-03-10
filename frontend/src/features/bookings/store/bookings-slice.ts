import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

interface BookingsUiState {
  roomIdFilter: number | null;
  fromDate: string | null;
  toDate: string | null;
}

const initialState: BookingsUiState = {
  roomIdFilter: null,
  fromDate: null,
  toDate: null,
};

export const bookingsSlice = createSlice({
  name: "bookings",
  initialState,
  reducers: {
    setRoomIdFilter(state, action: PayloadAction<number | null>) {
      state.roomIdFilter = action.payload;
    },
    setFromDate(state, action: PayloadAction<string | null>) {
      state.fromDate = action.payload;
    },
    setToDate(state, action: PayloadAction<string | null>) {
      state.toDate = action.payload;
    },
  },
});

export const { setRoomIdFilter, setFromDate, setToDate } =
  bookingsSlice.actions;

export default bookingsSlice.reducer;
