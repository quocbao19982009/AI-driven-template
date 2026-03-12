import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useTranslation } from "react-i18next";
import { PostApiAuthLoginWithJsonBody } from "@/api/generated/auth/auth.zod";

export function useLoginForm() {
  const { t } = useTranslation();

  const schema = PostApiAuthLoginWithJsonBody.extend({
    email: z
      .email(t("auth.validation.emailInvalid"))
      .min(1, t("auth.validation.emailRequired"))
      .max(256, t("auth.validation.emailTooLong")),
    password: z
      .string()
      .min(1, t("auth.validation.passwordRequired"))
      .max(200, t("auth.validation.passwordTooLong")),
  });

  return useForm({
    resolver: zodResolver(schema),
    defaultValues: {
      email: "",
      password: "",
    },
  });
}
