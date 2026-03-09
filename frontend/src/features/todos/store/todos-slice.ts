import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

interface TodosUiState {
  searchQuery: string;
  selectedIds: number[];
}

const initialState: TodosUiState = {
  searchQuery: "",
  selectedIds: [],
};

export const todosSlice = createSlice({
  name: "todos",
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

export const { setSearchQuery, toggleSelected, clearSelection } =
  todosSlice.actions;

export default todosSlice.reducer;
