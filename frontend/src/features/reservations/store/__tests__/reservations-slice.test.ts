import { configureStore } from "@reduxjs/toolkit";
import reservationsReducer, {
  setFactoryFilter,
  setPersonFilter,
  toggleSelected,
  clearSelection,
} from "../reservations-slice";

// Partition analysis:
// factoryFilter: null (cleared), number (set)
// personFilter:  null (cleared), number (set)
// selectedIds:   empty [], id absent (add), id present (remove), multiple (clear)

function makeStore() {
  return configureStore({ reducer: { reservations: reservationsReducer } });
}

describe("reservationsSlice", () => {
  it("has correct initial state", () => {
    const store = makeStore();
    const state = store.getState().reservations;
    expect(state.factoryFilter).toBeNull();
    expect(state.personFilter).toBeNull();
    expect(state.selectedIds).toEqual([]);
  });

  it("setFactoryFilter sets a factory id", () => {
    const store = makeStore();
    store.dispatch(setFactoryFilter(5));
    expect(store.getState().reservations.factoryFilter).toBe(5);
  });

  it("setFactoryFilter can be cleared to null", () => {
    const store = makeStore();
    store.dispatch(setFactoryFilter(5));
    store.dispatch(setFactoryFilter(null));
    expect(store.getState().reservations.factoryFilter).toBeNull();
  });

  it("setPersonFilter sets a person id", () => {
    const store = makeStore();
    store.dispatch(setPersonFilter(3));
    expect(store.getState().reservations.personFilter).toBe(3);
  });

  it("toggleSelected adds an id when not already present", () => {
    const store = makeStore();
    store.dispatch(toggleSelected(1));
    expect(store.getState().reservations.selectedIds).toContain(1);
  });

  it("toggleSelected removes an id when already present", () => {
    const store = makeStore();
    store.dispatch(toggleSelected(1));
    store.dispatch(toggleSelected(1));
    expect(store.getState().reservations.selectedIds).toEqual([]);
  });

  it("clearSelection empties selectedIds", () => {
    const store = makeStore();
    store.dispatch(toggleSelected(1));
    store.dispatch(toggleSelected(2));
    store.dispatch(clearSelection());
    expect(store.getState().reservations.selectedIds).toEqual([]);
  });
});
