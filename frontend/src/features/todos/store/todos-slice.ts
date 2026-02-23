import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

type StatusFilter = "all" | "active" | "completed";

interface TodosUiState {
  searchQuery: string;
  selectedIds: number[];
  statusFilter: StatusFilter;
  priorityFilter: number | null;
}

const initialState: TodosUiState = {
  searchQuery: "",
  selectedIds: [],
  statusFilter: "all",
  priorityFilter: null,
};

export const todosSlice = createSlice({
  name: "todos",
  initialState,
  reducers: {
    setSearchQuery(state, action: PayloadAction<string>) {
      state.searchQuery = action.payload;
    },
    setStatusFilter(state, action: PayloadAction<StatusFilter>) {
      state.statusFilter = action.payload;
    },
    setPriorityFilter(state, action: PayloadAction<number | null>) {
      state.priorityFilter = action.payload;
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

export const {
  setSearchQuery,
  setStatusFilter,
  setPriorityFilter,
  toggleSelected,
  clearSelection,
} = todosSlice.actions;

export default todosSlice.reducer;
