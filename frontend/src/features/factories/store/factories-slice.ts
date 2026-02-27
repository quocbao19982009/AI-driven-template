import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

interface FactoriesUiState {
  searchQuery: string;
  selectedIds: number[];
}

const initialState: FactoriesUiState = {
  searchQuery: "",
  selectedIds: [],
};

export const factoriesSlice = createSlice({
  name: "factories",
  initialState,
  reducers: {
    setSearchQuery(state, action: PayloadAction<string>) {
      state.searchQuery = action.payload;
    },
    toggleSelected(state, action: PayloadAction<number>) {
      const id = action.payload;
      const index = state.selectedIds.indexOf(id);
      if (index === -1) {
        state.selectedIds.push(id);
      } else {
        state.selectedIds.splice(index, 1);
      }
    },
    clearSelection(state) {
      state.selectedIds = [];
    },
  },
});

export const { setSearchQuery, toggleSelected, clearSelection } = factoriesSlice.actions;
export default factoriesSlice.reducer;
