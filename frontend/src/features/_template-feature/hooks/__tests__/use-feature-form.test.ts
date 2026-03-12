import { renderHook } from "@testing-library/react";
import { PostApiFeaturesBody } from "@/api/generated/feature/feature.zod";
import { useFeatureForm } from "../use-feature-form";

describe("PostApiFeaturesBody", () => {
  it("accepts a valid name", () => {
    const result = PostApiFeaturesBody.safeParse({ name: "My Feature" });
    expect(result.success).toBe(true);
  });

  it("rejects a missing name", () => {
    const result = PostApiFeaturesBody.safeParse({});
    expect(result.success).toBe(false);
  });
});

describe("useFeatureForm", () => {
  it("returns default empty name when no feature provided", () => {
    const { result } = renderHook(() => useFeatureForm());
    expect(result.current.getValues("name")).toBe("");
  });

  it("pre-fills name when editing an existing feature", () => {
    const feature = {
      id: 1,
      name: "Existing",
      createdAt: new Date().toISOString(),
    };
    const { result } = renderHook(() => useFeatureForm(feature));
    expect(result.current.getValues("name")).toBe("Existing");
  });
});
