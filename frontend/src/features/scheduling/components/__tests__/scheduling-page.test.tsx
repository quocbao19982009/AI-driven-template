import { render, screen } from "@/test/test-utils";
import { SchedulingPage } from "../scheduling-page";

// scheduling-page.tsx imports useGetApiSchedulingByPerson and useGetApiSchedulingByFactory
// directly — both are called inside sub-tab components rendered on mount.

vi.mock("@/api/generated/scheduling/scheduling", () => ({
  useGetApiSchedulingByPerson: () => ({ data: undefined, isLoading: false }),
  useGetApiSchedulingByFactory: () => ({ data: undefined, isLoading: false }),
}));

describe("SchedulingPage", () => {
  it("renders the page title", () => {
    render(<SchedulingPage />);
    expect(screen.getByText("scheduling.title")).toBeInTheDocument();
  });

  it("renders the description text", () => {
    render(<SchedulingPage />);
    expect(screen.getByText("scheduling.description")).toBeInTheDocument();
  });

  it("renders the by-person tab button", () => {
    render(<SchedulingPage />);
    expect(screen.getByText("scheduling.tabs.byPerson")).toBeInTheDocument();
  });

  it("renders the by-factory tab button", () => {
    render(<SchedulingPage />);
    expect(screen.getByText("scheduling.tabs.byFactory")).toBeInTheDocument();
  });
});
