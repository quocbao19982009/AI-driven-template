import { render, screen } from "@/test/test-utils";
import { FactoriesPage } from "../factories-page";

// Generated hooks consumed directly by FactoriesPage and its sub-components:
// factories-page → useFactoriesPagination (mocked below)
// factory-form-dialog → usePostApiFactoriesWithJson, usePutApiFactoriesIdWithJson,
//                       getGetApiFactoriesQueryKey, getGetApiFactoriesAllQueryKey,
//                       getGetApiPersonnelQueryKey, getGetApiPersonnelAllQueryKey,
//                       getGetApiReservationsQueryKey
// factory-delete-dialog → useDeleteApiFactoriesId, getGetApiFactoriesQueryKey,
//                         getGetApiFactoriesAllQueryKey, getGetApiPersonnelQueryKey,
//                         getGetApiPersonnelAllQueryKey

vi.mock("@/api/generated/factories/factories", () => ({
  useGetApiFactories: () => ({ data: undefined, isLoading: false }),
  useGetApiFactoriesAll: () => ({ data: undefined, isLoading: false }),
  usePostApiFactoriesWithJson: () => ({ mutate: vi.fn(), isPending: false }),
  usePutApiFactoriesIdWithJson: () => ({ mutate: vi.fn(), isPending: false }),
  useDeleteApiFactoriesId: () => ({ mutate: vi.fn(), isPending: false }),
  getGetApiFactoriesQueryKey: () => ["api", "factories"],
  getGetApiFactoriesAllQueryKey: () => ["api", "factories", "all"],
}));

vi.mock("@/api/generated/personnel/personnel", () => ({
  useGetApiPersonnel: () => ({ data: undefined, isLoading: false }),
  useGetApiPersonnelAll: () => ({ data: undefined, isLoading: false }),
  getGetApiPersonnelQueryKey: () => ["api", "personnel"],
  getGetApiPersonnelAllQueryKey: () => ["api", "personnel", "all"],
}));

vi.mock("@/api/generated/reservations/reservations", () => ({
  useGetApiReservations: () => ({ data: undefined, isLoading: false }),
  getGetApiReservationsQueryKey: () => ["api", "reservations"],
}));

vi.mock("@/features/factories/hooks", () => ({
  useFactoriesPagination: () => ({
    factories: [],
    isLoading: false,
    page: 1,
    totalPages: 1,
    setPage: vi.fn(),
  }),
}));

describe("FactoriesPage", () => {
  it("renders the page title", () => {
    render(<FactoriesPage />);
    expect(screen.getByText("factories.title")).toBeInTheDocument();
  });

  it("renders the description text", () => {
    render(<FactoriesPage />);
    expect(screen.getByText("factories.description")).toBeInTheDocument();
  });

  it("renders the New Factory button", () => {
    render(<FactoriesPage />);
    expect(screen.getByText("factories.newFactory")).toBeInTheDocument();
  });

  it("renders the search input", () => {
    render(<FactoriesPage />);
    expect(
      screen.getByPlaceholderText("factories.searchPlaceholder"),
    ).toBeInTheDocument();
  });
});
