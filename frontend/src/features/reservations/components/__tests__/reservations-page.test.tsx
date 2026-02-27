import { render, screen } from "@/test/test-utils";
import { ReservationsPage } from "../reservations-page";

// Generated hooks consumed by ReservationsPage and its sub-components:
// reservations-page → useGetApiFactoriesAll, useGetApiPersonnelAll,
//                     useReservationsPagination (mocked below)
// reservation-form-dialog → usePostApiReservationsWithJson, usePutApiReservationsIdWithJson,
//                           getGetApiReservationsQueryKey, useGetApiFactoriesAll,
//                           useGetApiPersonnelAll, getGetApiSchedulingByPersonQueryKey,
//                           getGetApiSchedulingByFactoryQueryKey
// reservation-delete-dialog → useDeleteApiReservationsId, getGetApiReservationsQueryKey,
//                             getGetApiSchedulingByPersonQueryKey, getGetApiSchedulingByFactoryQueryKey

vi.mock("@/api/generated/reservations/reservations", () => ({
  useGetApiReservations: () => ({ data: undefined, isLoading: false }),
  usePostApiReservationsWithJson: () => ({ mutate: vi.fn(), isPending: false }),
  usePutApiReservationsIdWithJson: () => ({ mutate: vi.fn(), isPending: false }),
  useDeleteApiReservationsId: () => ({ mutate: vi.fn(), isPending: false }),
  getGetApiReservationsQueryKey: () => ["api", "reservations"],
}));

vi.mock("@/api/generated/factories/factories", () => ({
  useGetApiFactoriesAll: () => ({ data: undefined, isLoading: false }),
  getGetApiFactoriesAllQueryKey: () => ["api", "factories", "all"],
}));

vi.mock("@/api/generated/personnel/personnel", () => ({
  useGetApiPersonnelAll: () => ({ data: undefined, isLoading: false }),
  getGetApiPersonnelAllQueryKey: () => ["api", "personnel", "all"],
}));

vi.mock("@/api/generated/scheduling/scheduling", () => ({
  useGetApiSchedulingByPerson: () => ({ data: undefined, isLoading: false }),
  useGetApiSchedulingByFactory: () => ({ data: undefined, isLoading: false }),
  getGetApiSchedulingByPersonQueryKey: () => ["api", "scheduling", "by-person"],
  getGetApiSchedulingByFactoryQueryKey: () => ["api", "scheduling", "by-factory"],
}));

vi.mock("@/features/reservations/hooks", () => ({
  useReservationsPagination: () => ({
    reservations: [],
    isLoading: false,
    page: 1,
    totalPages: 1,
    setPage: vi.fn(),
  }),
}));

describe("ReservationsPage", () => {
  it("renders the page title", () => {
    render(<ReservationsPage />);
    expect(screen.getByText("reservations.title")).toBeInTheDocument();
  });

  it("renders the description text", () => {
    render(<ReservationsPage />);
    expect(screen.getByText("reservations.description")).toBeInTheDocument();
  });

  it("renders the New Reservation button", () => {
    render(<ReservationsPage />);
    expect(screen.getByText("reservations.newReservation")).toBeInTheDocument();
  });

  it("renders the factory filter dropdown", () => {
    render(<ReservationsPage />);
    expect(
      screen.getByText("reservations.filter.allFactories"),
    ).toBeInTheDocument();
  });

  it("renders the personnel filter dropdown", () => {
    render(<ReservationsPage />);
    expect(
      screen.getByText("reservations.filter.allPersonnel"),
    ).toBeInTheDocument();
  });
});
