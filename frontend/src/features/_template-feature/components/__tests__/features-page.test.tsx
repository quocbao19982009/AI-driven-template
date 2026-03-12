import { render, screen } from "@/test/test-utils";
import { FeaturesPage } from "../features-page";

vi.mock("@/api/generated/feature/feature", () => ({
  useGetApiFeatures: () => ({ data: undefined, isLoading: true }),
  usePostApiFeatures: () => ({ mutate: vi.fn(), isPending: false }),
  usePutApiFeaturesId: () => ({ mutate: vi.fn(), isPending: false }),
  useDeleteApiFeaturesId: () => ({ mutate: vi.fn(), isPending: false }),
  getGetApiFeaturesQueryKey: () => ["api", "features"],
}));

describe("FeaturesPage", () => {
  it("renders the page title", () => {
    render(<FeaturesPage />);
    expect(screen.getByText("features.title")).toBeInTheDocument();
  });

  it("renders the New Feature button", () => {
    render(<FeaturesPage />);
    expect(screen.getByText("features.newFeature")).toBeInTheDocument();
  });

  it("renders the description text", () => {
    render(<FeaturesPage />);
    expect(screen.getByText("features.description")).toBeInTheDocument();
  });
});
