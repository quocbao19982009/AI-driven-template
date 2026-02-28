import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

interface FlashcardsUiState {
  searchQuery: string;
  selectedIds: number[];
  activeCategoryFilter: string;
  activeTab: "manage" | "study";
  studyCategory: string;
}

const initialState: FlashcardsUiState = {
  searchQuery: "",
  selectedIds: [],
  activeCategoryFilter: "",
  activeTab: "manage",
  studyCategory: "",
};

export const flashcardsSlice = createSlice({
  name: "flashcards",
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
    setActiveCategoryFilter(state, action: PayloadAction<string>) {
      state.activeCategoryFilter = action.payload;
    },
    setActiveTab(state, action: PayloadAction<"manage" | "study">) {
      state.activeTab = action.payload;
    },
    setStudyCategory(state, action: PayloadAction<string>) {
      state.studyCategory = action.payload;
    },
  },
});

export const {
  setSearchQuery,
  toggleSelected,
  clearSelection,
  setActiveCategoryFilter,
  setActiveTab,
  setStudyCategory,
} = flashcardsSlice.actions;
export default flashcardsSlice.reducer;
