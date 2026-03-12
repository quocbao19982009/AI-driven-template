import { Component, type ErrorInfo, type ReactNode } from "react";
import i18n from "@/lib/i18n";

interface Props {
  children: ReactNode;
  fallback?: (error: Error, reset: () => void) => ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
}

export class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    if (import.meta.env.DEV) {
      console.error("ErrorBoundary caught:", error, info);
    }
  }

  reset = () => {
    this.setState({ hasError: false, error: null });
  };

  render() {
    if (this.state.hasError && this.state.error) {
      if (this.props.fallback) {
        return this.props.fallback(this.state.error, this.reset);
      }

      return (
        <div className="flex min-h-screen flex-col items-center justify-center gap-4 p-8 text-center">
          <h1 className="text-2xl font-bold">
            {import.meta.env.DEV ? this.state.error.message : i18n.t("errors.somethingWentWrong")}
          </h1>
          {import.meta.env.DEV && (
            <pre className="max-w-xl overflow-auto rounded bg-muted p-4 text-left text-sm">
              {this.state.error.stack}
            </pre>
          )}
          <button
            onClick={this.reset}
            className="rounded bg-primary px-4 py-2 text-primary-foreground hover:bg-primary/90"
          >
            {i18n.t("errors.tryAgain")}
          </button>
        </div>
      );
    }

    return this.props.children;
  }
}
