import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

interface BookingsUiState {
  roomFilter: number | null;
  fromDate: string | null;
  toDate: string | null;
}

const initialState: BookingsUiState = {
  roomFilter: null,
  fromDate: null,
  toDate: null,
};

export const bookingsSlice = createSlice({
  name: "bookings",
  initialState,
  reducers: {
    setRoomFilter(state, action: PayloadAction<number | null>) {
      state.roomFilter = action.payload;
    },
    setFromDate(state, action: PayloadAction<string | null>) {
      state.fromDate = action.payload;
    },
    setToDate(state, action: PayloadAction<string | null>) {
      state.toDate = action.payload;
    },
  },
});

export const { setRoomFilter, setFromDate, setToDate } = bookingsSlice.actions;

export default bookingsSlice.reducer;
