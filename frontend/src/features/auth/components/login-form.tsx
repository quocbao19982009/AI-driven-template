import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Eye, EyeOff, Loader2, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { cn } from "@/lib/utils";
import { useLoginForm } from "../hooks/use-login-form";
import { useAppDispatch, useAppSelector } from "@/store/hooks";
import {
  setLoggingIn,
  setLoginError,
  clearLoginError,
  setUser,
} from "../store/auth-slice";
import { usePostApiAuthLogin } from "@/api/generated/auth/auth";
import { useGetApiAuthMe } from "@/api/generated/auth/auth";
import { useAuth } from "@/auth/auth-context";
import { ApiError, setApiFetchToken } from "@/api/mutator/apiFetch";
import type { MeDto } from "@/api/generated/models";

interface LoginFormProps {
  onSuccess: () => void;
}

export function LoginForm({ onSuccess }: LoginFormProps) {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const { isLoggingIn, loginError } = useAppSelector((s) => s.auth);
  const { login } = useAuth();
  const [showPassword, setShowPassword] = useState(false);

  const form = useLoginForm();
  const {
    register,
    handleSubmit,
    formState: { errors },
    resetField,
  } = form;

  // We use the me query lazily after login to get user profile
  const meQuery = useGetApiAuthMe({ query: { enabled: false } });

  const loginMutation = usePostApiAuthLogin({
    mutation: {
      onMutate: () => {
        dispatch(setLoggingIn(true));
        dispatch(clearLoginError());
      },
      onSuccess: async (res) => {
        const token = res.data?.data?.accessToken;
        if (!token) {
          dispatch(setLoginError(t("auth.error.generic")));
          dispatch(setLoggingIn(false));
          return;
        }

        // Set token immediately so the /me request is authenticated
        setApiFetchToken(token);

        // Fetch user profile and store in Redux
        try {
          const meRes = await meQuery.refetch();
          const userData = (meRes.data?.data?.data ?? null) as MeDto | null;
          dispatch(setUser(userData));
        } catch {
          // /me failed — user stays null, not critical
        }

        login(token);
        dispatch(setLoggingIn(false));
        onSuccess();
      },
      onError: (error: unknown) => {
        dispatch(setLoggingIn(false));
        if (error instanceof ApiError) {
          dispatch(setLoginError(error.message));
        } else {
          dispatch(setLoginError(t("auth.error.generic")));
        }
        resetField("password");
      },
    },
  });

  const onSubmit = handleSubmit((values) => {
    loginMutation.mutate({ data: values });
  });

  return (
    <form onSubmit={onSubmit} className="space-y-4" noValidate>
      {loginError && (
        <div
          role="alert"
          className="border-destructive/50 bg-destructive/10 text-destructive flex items-start gap-2 rounded-md border px-3 py-2 text-sm"
        >
          <span className="flex-1">{loginError}</span>
          <button
            type="button"
            aria-label={t("common.dismiss")}
            onClick={() => dispatch(clearLoginError())}
            className="mt-0.5 shrink-0 rounded hover:opacity-70"
          >
            <X className="h-4 w-4" />
          </button>
        </div>
      )}

      <div className="space-y-1">
        <Label htmlFor="email">{t("auth.form.emailLabel")}</Label>
        <Input
          id="email"
          type="email"
          autoFocus
          autoComplete="email"
          placeholder={t("auth.form.emailPlaceholder")}
          aria-invalid={!!errors.email}
          {...register("email")}
        />
        {errors.email && (
          <p className="text-destructive text-xs">{errors.email.message}</p>
        )}
      </div>

      <div className="space-y-1">
        <Label htmlFor="password">{t("auth.form.passwordLabel")}</Label>
        <div className="relative">
          <Input
            id="password"
            type={showPassword ? "text" : "password"}
            autoComplete="current-password"
            placeholder={t("auth.form.passwordPlaceholder")}
            aria-invalid={!!errors.password}
            className="pr-10"
            {...register("password")}
          />
          <button
            type="button"
            aria-label={
              showPassword
                ? t("auth.form.hidePassword")
                : t("auth.form.showPassword")
            }
            onClick={() => setShowPassword((v) => !v)}
            className={cn(
              "text-muted-foreground hover:text-foreground absolute inset-y-0 right-0 flex items-center px-3"
            )}
          >
            {showPassword ? (
              <EyeOff className="h-4 w-4" />
            ) : (
              <Eye className="h-4 w-4" />
            )}
          </button>
        </div>
        {errors.password && (
          <p className="text-destructive text-xs">{errors.password.message}</p>
        )}
      </div>

      <Button type="submit" className="w-full" disabled={isLoggingIn}>
        {isLoggingIn && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
        {t("auth.form.submitButton")}
      </Button>
    </form>
  );
}
