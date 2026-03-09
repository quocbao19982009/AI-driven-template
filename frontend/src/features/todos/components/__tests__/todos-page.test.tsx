import { render, screen } from "@/test/test-utils";
import { TodosPage } from "../todos-page";

vi.mock("@/api/generated/todos/todos", () => ({
  useGetApiTodos: () => ({ data: undefined, isLoading: true }),
  usePostApiTodosWithJson: () => ({ mutate: vi.fn(), isPending: false }),
  usePutApiTodosIdWithJson: () => ({ mutate: vi.fn(), isPending: false }),
  useDeleteApiTodosId: () => ({ mutate: vi.fn(), isPending: false }),
  usePatchApiTodosIdToggle: () => ({ mutate: vi.fn(), isPending: false }),
  getGetApiTodosQueryKey: () => ["/api/todos"],
}));

describe("TodosPage", () => {
  it("renders the page title", () => {
    render(<TodosPage />);
    expect(screen.getByText("todos.title")).toBeInTheDocument();
  });

  it("renders the New Todo button", () => {
    render(<TodosPage />);
    expect(screen.getByText("todos.newTodo")).toBeInTheDocument();
  });

  it("renders the description text", () => {
    render(<TodosPage />);
    expect(screen.getByText("todos.description")).toBeInTheDocument();
  });
});
