import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { PostApiTodosWithJsonBody } from "@/api/generated/todos/todos.zod";

export const todoFormSchema = PostApiTodosWithJsonBody;

export type TodoFormValues = z.infer<typeof todoFormSchema>;

export interface TodoDto {
  id: number;
  title: string;
  description?: string | null;
  isCompleted: boolean;
  dueDate?: string | null;
  createdAt: string;
  updatedAt?: string | null;
}

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
