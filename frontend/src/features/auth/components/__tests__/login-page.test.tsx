import { render, screen } from "@/test/test-utils";
import { LoginPage } from "../login-page";

vi.mock("@/api/generated/auth/auth", () => ({
  usePostApiAuthLoginWithJson: () => ({ mutate: vi.fn(), isPending: false }),
  useGetApiAuthMe: () => ({
    data: undefined,
    isLoading: false,
    refetch: vi.fn(),
  }),
  postApiAuthRefresh: () => Promise.reject(new Error("401")),
}));

vi.mock("@tanstack/react-router", async (importOriginal) => {
  const actual =
    await importOriginal<typeof import("@tanstack/react-router")>();
  return {
    ...actual,
    useNavigate: () => vi.fn(),
    useSearch: () => ({}),
  };
});

vi.mock("@/auth/auth-context", () => ({
  useAuth: () => ({
    accessToken: null,
    isLoading: false,
    login: vi.fn(),
    logout: vi.fn(),
  }),
  AuthContext: {
    Provider: ({ children }: { children: React.ReactNode }) => children,
  },
}));

describe("LoginPage", () => {
  it("renders the page title", () => {
    render(<LoginPage />);
    expect(screen.getByText("auth.page.title")).toBeInTheDocument();
  });

  it("renders email and password fields", () => {
    render(<LoginPage />);
    expect(screen.getByLabelText("auth.form.emailLabel")).toBeInTheDocument();
    expect(
      screen.getByLabelText("auth.form.passwordLabel")
    ).toBeInTheDocument();
  });

  it("renders the sign in button", () => {
    render(<LoginPage />);
    expect(
      screen.getByRole("button", { name: "auth.form.submitButton" })
    ).toBeInTheDocument();
  });
});
