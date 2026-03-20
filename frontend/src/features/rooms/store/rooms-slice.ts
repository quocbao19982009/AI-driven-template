import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

interface RoomsUiState {
  searchQuery: string;
  locationFilter: number | null;
  sortBy: "name" | "capacity" | "createdAt";
  sortDir: "asc" | "desc";
  activeTab: "rooms" | "calendar";
  selectedRoomId: number | null;
}

const initialState: RoomsUiState = {
  searchQuery: "",
  locationFilter: null,
  sortBy: "name",
  sortDir: "asc",
  activeTab: "rooms",
  selectedRoomId: null,
};

export const roomsSlice = createSlice({
  name: "rooms",
  initialState,
  reducers: {
    setSearchQuery(state, action: PayloadAction<string>) {
      state.searchQuery = action.payload;
    },
    setLocationFilter(state, action: PayloadAction<number | null>) {
      state.locationFilter = action.payload;
    },
    setSortBy(state, action: PayloadAction<"name" | "capacity" | "createdAt">) {
      state.sortBy = action.payload;
    },
    setSortDir(state, action: PayloadAction<"asc" | "desc">) {
      state.sortDir = action.payload;
    },
    setActiveTab(state, action: PayloadAction<"rooms" | "calendar">) {
      state.activeTab = action.payload;
    },
    setSelectedRoomId(state, action: PayloadAction<number | null>) {
      state.selectedRoomId = action.payload;
    },
  },
});

export const {
  setSearchQuery,
  setLocationFilter,
  setSortBy,
  setSortDir,
  setActiveTab,
  setSelectedRoomId,
} = roomsSlice.actions;

export default roomsSlice.reducer;
