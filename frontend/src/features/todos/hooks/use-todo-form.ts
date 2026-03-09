import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { PostApiTodosWithJsonBody } from "@/api/generated/todos/todos.zod";
import type { TodoDto } from "@/api/generated/models";

export type { TodoDto };

export const todoFormSchema = PostApiTodosWithJsonBody;

export type TodoFormValues = z.infer<typeof todoFormSchema>;

export function useTodoForm(todo?: TodoDto | null) {
  return useForm<TodoFormValues>({
    resolver: zodResolver(todoFormSchema),
    defaultValues: {
      title: todo?.title ?? "",
      description: todo?.description ?? "",
      dueDate: todo?.dueDate ?? null,
    },
  });
}
