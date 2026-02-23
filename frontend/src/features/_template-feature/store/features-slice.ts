import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

interface FeaturesUiState {
  searchQuery: string;
  selectedIds: string[];
}

const initialState: FeaturesUiState = {
  searchQuery: "",
  selectedIds: [],
};

export const featuresSlice = createSlice({
  name: "features",
  initialState,
  reducers: {
    setSearchQuery(state, action: PayloadAction<string>) {
      state.searchQuery = action.payload;
    },
    toggleSelected(state, action: PayloadAction<string>) {
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

export const { setSearchQuery, toggleSelected, clearSelection } =
  featuresSlice.actions;

export default featuresSlice.reducer;
