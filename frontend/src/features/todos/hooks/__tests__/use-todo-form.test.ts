import { renderHook } from "@testing-library/react";
import { useTodoForm, todoFormSchema } from "../use-todo-form";

describe("todoFormSchema", () => {
  it("accepts a valid title", () => {
    const result = todoFormSchema.safeParse({ title: "Buy milk" });
    expect(result.success).toBe(true);
  });

  it("rejects a missing title field", () => {
    const result = todoFormSchema.safeParse({});
    expect(result.success).toBe(false);
  });

  it("accepts optional description and dueDate", () => {
    const result = todoFormSchema.safeParse({
      title: "Test",
      description: "Some desc",
      dueDate: new Date(Date.now() + 86400000).toISOString(),
    });
    expect(result.success).toBe(true);
  });
});

describe("useTodoForm", () => {
  it("returns default empty values when no todo provided", () => {
    const { result } = renderHook(() => useTodoForm());
    expect(result.current.getValues("title")).toBe("");
    expect(result.current.getValues("description")).toBe("");
  });

  it("pre-fills values when editing an existing todo", () => {
    const todo = {
      id: 1,
      title: "Existing Todo",
      description: "Some description",
      isCompleted: false,
      dueDate: null,
      createdAt: new Date().toISOString(),
    };
    const { result } = renderHook(() => useTodoForm(todo));
    expect(result.current.getValues("title")).toBe("Existing Todo");
    expect(result.current.getValues("description")).toBe("Some description");
  });
});
