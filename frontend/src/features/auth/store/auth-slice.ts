import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

interface AuthUiState {
  isLoggingIn: boolean;
  loginError: string | null;
}

const initialState: AuthUiState = {
  isLoggingIn: false,
  loginError: null,
};

export const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
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

export const { setLoggingIn, setLoginError, clearLoginError } =
  authSlice.actions;

export default authSlice.reducer;
