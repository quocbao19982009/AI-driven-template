import { renderHook } from "@testing-library/react";
import { useFeatureForm, featureFormSchema } from "../use-feature-form";

describe("featureFormSchema", () => {
  it("accepts a valid name", () => {
    const result = featureFormSchema.safeParse({ name: "My Feature" });
    expect(result.success).toBe(true);
  });

  it("rejects a missing name field", () => {
    const result = featureFormSchema.safeParse({});
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
