import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useTranslation } from "react-i18next";
import { PostApiFeaturesWithJsonBody } from "@/api/generated/feature/feature.zod";
import type { FeatureDto } from "@/api/generated/models";

export function useFeatureForm(feature?: FeatureDto | null) {
  const { t } = useTranslation();

  const schema = PostApiFeaturesWithJsonBody.extend({
    name: z.string().min(1, t("features.validation.nameRequired")),
  });

  return useForm({
    resolver: zodResolver(schema),
    defaultValues: {
      name: feature?.name ?? "",
    },
  });
}
