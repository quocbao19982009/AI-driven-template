import { createSlice, type PayloadAction } from "@reduxjs/toolkit";
import type { MeDto } from "@/api/generated/models";

interface AuthUiState {
  user: MeDto | null;
  isLoggingIn: boolean;
  loginError: string | null;
}

const initialState: AuthUiState = {
  user: null,
  isLoggingIn: false,
  loginError: null,
};

export const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    setUser(state, action: PayloadAction<MeDto | null>) {
      state.user = action.payload;
    },
    clearUser(state) {
      state.user = null;
    },
    setLoggingIn(state, action: PayloadAction<boolean>) {
      state.isLoggingIn = action.payload;
    },
    setLoginError(state, action: PayloadAction<string | null>) {
      state.loginError = action.payload;
    },
    clearLoginError(state) {
      state.loginError = null;
    },
  },
});

export const {
  setUser,
  clearUser,
  setLoggingIn,
  setLoginError,
  clearLoginError,
} = authSlice.actions;

export default authSlice.reducer;
