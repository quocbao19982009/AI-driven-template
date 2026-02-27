import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

interface ReservationsUiState {
  selectedIds: number[];
  factoryFilter: number | null;
  personFilter: number | null;
}

const initialState: ReservationsUiState = {
  selectedIds: [],
  factoryFilter: null,
  personFilter: null,
};

export const reservationsSlice = createSlice({
  name: "reservations",
  initialState,
  reducers: {
    setFactoryFilter(state, action: PayloadAction<number | null>) {
      state.factoryFilter = action.payload;
    },
    setPersonFilter(state, action: PayloadAction<number | null>) {
      state.personFilter = action.payload;
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

export const { setFactoryFilter, setPersonFilter, toggleSelected, clearSelection } =
  reservationsSlice.actions;
export default reservationsSlice.reducer;
