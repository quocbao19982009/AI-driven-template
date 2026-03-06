import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

interface RoomsUiState {
  searchQuery: string;
  locationIdFilter: number | null;
  sortBy: "name" | "capacity" | "createdAt";
  sortDir: "asc" | "desc";
  activeTab: "rooms" | "calendar";
  selectedRoomIdForCalendar: number | null;
}

const initialState: RoomsUiState = {
  searchQuery: "",
  locationIdFilter: null,
  sortBy: "createdAt",
  sortDir: "asc",
  activeTab: "rooms",
  selectedRoomIdForCalendar: null,
};

export const roomsSlice = createSlice({
  name: "rooms",
  initialState,
  reducers: {
    setSearchQuery(state, action: PayloadAction<string>) {
      state.searchQuery = action.payload;
    },
    setLocationIdFilter(state, action: PayloadAction<number | null>) {
      state.locationIdFilter = action.payload;
    },
    setSortBy(state, action: PayloadAction<RoomsUiState["sortBy"]>) {
      state.sortBy = action.payload;
    },
    setSortDir(state, action: PayloadAction<"asc" | "desc">) {
      state.sortDir = action.payload;
    },
    setActiveTab(state, action: PayloadAction<"rooms" | "calendar">) {
      state.activeTab = action.payload;
    },
    setSelectedRoomIdForCalendar(state, action: PayloadAction<number | null>) {
      state.selectedRoomIdForCalendar = action.payload;
    },
  },
});

export const {
  setSearchQuery,
  setLocationIdFilter,
  setSortBy,
  setSortDir,
  setActiveTab,
  setSelectedRoomIdForCalendar,
} = roomsSlice.actions;

export default roomsSlice.reducer;
