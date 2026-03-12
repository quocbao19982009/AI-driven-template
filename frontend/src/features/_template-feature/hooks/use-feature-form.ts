import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { PostApiFeaturesWithJsonBody } from "@/api/generated/feature/feature.zod";
import type { FeatureDto } from "@/api/generated/models";

export const featureFormSchema = PostApiFeaturesWithJsonBody;

export type FeatureFormValues = z.infer<typeof featureFormSchema>;

export function useFeatureForm(feature?: FeatureDto | null) {
  return useForm<FeatureFormValues>({
    resolver: zodResolver(featureFormSchema),
    defaultValues: {
      name: feature?.name ?? "",
    },
  });
}
