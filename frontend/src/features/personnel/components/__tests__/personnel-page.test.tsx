import { render, screen } from "@/test/test-utils";
import { PersonnelPage } from "../personnel-page";

// Generated hooks consumed by PersonnelPage and its sub-components:
// person-form-dialog → usePostApiPersonnelWithJson, usePutApiPersonnelIdWithJson,
//                      getGetApiPersonnelQueryKey, getGetApiPersonnelAllQueryKey,
//                      useGetApiFactoriesAll, getGetApiReservationsQueryKey
// person-delete-dialog → useDeleteApiPersonnelId, getGetApiPersonnelQueryKey,
//                        getGetApiPersonnelAllQueryKey, getGetApiReservationsQueryKey

vi.mock("@/api/generated/personnel/personnel", () => ({
  useGetApiPersonnel: () => ({ data: undefined, isLoading: false }),
  useGetApiPersonnelAll: () => ({ data: undefined, isLoading: false }),
  usePostApiPersonnelWithJson: () => ({ mutate: vi.fn(), isPending: false }),
  usePutApiPersonnelIdWithJson: () => ({ mutate: vi.fn(), isPending: false }),
  useDeleteApiPersonnelId: () => ({ mutate: vi.fn(), isPending: false }),
  getGetApiPersonnelQueryKey: () => ["api", "personnel"],
  getGetApiPersonnelAllQueryKey: () => ["api", "personnel", "all"],
}));

vi.mock("@/api/generated/factories/factories", () => ({
  useGetApiFactoriesAll: () => ({ data: undefined, isLoading: false }),
  getGetApiFactoriesAllQueryKey: () => ["api", "factories", "all"],
}));

vi.mock("@/api/generated/reservations/reservations", () => ({
  useGetApiReservations: () => ({ data: undefined, isLoading: false }),
  getGetApiReservationsQueryKey: () => ["api", "reservations"],
}));

vi.mock("@/features/personnel/hooks", () => ({
  usePersonnelPagination: () => ({
    personnel: [],
    isLoading: false,
    page: 1,
    totalPages: 1,
    setPage: vi.fn(),
  }),
}));

describe("PersonnelPage", () => {
  it("renders the page title", () => {
    render(<PersonnelPage />);
    expect(screen.getByText("personnel.title")).toBeInTheDocument();
  });

  it("renders the description text", () => {
    render(<PersonnelPage />);
    expect(screen.getByText("personnel.description")).toBeInTheDocument();
  });

  it("renders the New Person button", () => {
    render(<PersonnelPage />);
    expect(screen.getByText("personnel.newPerson")).toBeInTheDocument();
  });

  it("renders the search input", () => {
    render(<PersonnelPage />);
    expect(
      screen.getByPlaceholderText("personnel.searchPlaceholder"),
    ).toBeInTheDocument();
  });
});
