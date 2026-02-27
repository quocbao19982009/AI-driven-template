import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

interface PersonnelUiState {
  searchQuery: string;
  selectedIds: number[];
}

const initialState: PersonnelUiState = {
  searchQuery: "",
  selectedIds: [],
};

export const personnelSlice = createSlice({
  name: "personnel",
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

export const { setSearchQuery, toggleSelected, clearSelection } = personnelSlice.actions;
export default personnelSlice.reducer;
