import { renderHook } from "@testing-library/react";
import { PostApiAuthLoginWithJsonBody } from "@/api/generated/auth/auth.zod";
import { useLoginForm } from "../use-login-form";

describe("PostApiAuthLoginWithJsonBody (Orval Zod schema)", () => {
  it("accepts valid email and password", () => {
    const result = PostApiAuthLoginWithJsonBody.safeParse({
      email: "user@example.com",
      password: "secret",
    });
    expect(result.success).toBe(true);
  });

  it("rejects missing email", () => {
    const result = PostApiAuthLoginWithJsonBody.safeParse({ password: "secret" });
    expect(result.success).toBe(false);
  });

  it("rejects missing password", () => {
    const result = PostApiAuthLoginWithJsonBody.safeParse({
      email: "user@example.com",
    });
    expect(result.success).toBe(false);
  });
});

describe("useLoginForm", () => {
  it("returns default empty values", () => {
    const { result } = renderHook(() => useLoginForm());
    expect(result.current.getValues("email")).toBe("");
    expect(result.current.getValues("password")).toBe("");
  });
});
