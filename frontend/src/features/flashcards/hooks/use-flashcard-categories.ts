import { useGetApiFlashcardCategoriesAll } from "@/api/generated/flashcard-categories/flashcard-categories";

export function useFlashcardCategories() {
  const { data: response } = useGetApiFlashcardCategoriesAll();
  return response?.data?.data ?? [];
}
