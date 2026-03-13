import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

interface ExpenseTrackersUiState {
  searchQuery: string;
  categoryFilter: string;
}

const initialState: ExpenseTrackersUiState = {
  searchQuery: "",
  categoryFilter: "all",
};

export const expenseTrackersSlice = createSlice({
  name: "expenseTrackers",
  initialState,
  reducers: {
    setSearchQuery(state, action: PayloadAction<string>) {
      state.searchQuery = action.payload;
    },
    setCategoryFilter(state, action: PayloadAction<string>) {
      state.categoryFilter = action.payload;
    },
  },
});

export const { setSearchQuery, setCategoryFilter } =
  expenseTrackersSlice.actions;

export default expenseTrackersSlice.reducer;

