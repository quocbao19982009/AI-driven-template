const BASE_URL = import.meta.env.VITE_API_URL! ?? "http://localhost:5054";

export class ApiError extends Error {
  status: number;
  errors: string[] | null;

  constructor(status: number, message: string, errors: string[] | null = null) {
    super(message);
    this.status = status;
    this.errors = errors;
  }
}

// In-memory access token — set by AuthProvider after login or silent refresh
let _accessToken: string | null = null;
let _isRefreshing = false;
let _refreshPromise: Promise<string | null> | null = null;

export function setApiFetchToken(token: string | null): void {
  _accessToken = token;
}

async function silentRefresh(): Promise<string | null> {
  if (_isRefreshing && _refreshPromise) {
    return _refreshPromise;
  }
  _isRefreshing = true;
  _refreshPromise = fetch(`${BASE_URL}/api/auth/refresh`, { method: "POST" })
    .then(async (res) => {
      if (!res.ok) return null;
      const body = await res.json().catch(() => null);
      const token = body?.data?.accessToken ?? null;
      _accessToken = token;
      return token;
    })
    .catch(() => null)
    .finally(() => {
      _isRefreshing = false;
      _refreshPromise = null;
    });
  return _refreshPromise;
}

export const apiFetch = async <T>(
  url: string,
  options?: RequestInit
): Promise<T> => {
  const authHeader: Record<string, string> = _accessToken
    ? { Authorization: `Bearer ${_accessToken}` }
    : {};

  const response = await fetch(`${BASE_URL}${url}`, {
    ...options,
    headers: {
      ...authHeader,
      ...options?.headers,
    },
  });

  // On 401, attempt one silent refresh then retry
  if (response.status === 401 && !url.includes("/api/auth/")) {
    const newToken = await silentRefresh();
    if (newToken) {
      const retryResponse = await fetch(`${BASE_URL}${url}`, {
        ...options,
        headers: {
          Authorization: `Bearer ${newToken}`,
          ...options?.headers,
        },
      });

      if (!retryResponse.ok) {
        const body = await retryResponse.json().catch(() => null);
        throw new ApiError(
          retryResponse.status,
          body?.message || `${retryResponse.status} ${retryResponse.statusText}`,
          body?.errors ?? null
        );
      }

      const json =
        retryResponse.status === 204 ? undefined : await retryResponse.json();
      return {
        data: json,
        status: retryResponse.status,
        headers: retryResponse.headers,
      } as T;
    }

    // Refresh failed — redirect to login
    if (typeof window !== "undefined") {
      window.location.href = `/login?returnUrl=${encodeURIComponent(window.location.pathname)}`;
    }
    throw new ApiError(401, "Unauthorized");
  }

  if (!response.ok) {
    const body = await response.json().catch(() => null);
    throw new ApiError(
      response.status,
      body?.message || `${response.status} ${response.statusText}`,
      body?.errors ?? null
    );
  }

  const json = response.status === 204 ? undefined : await response.json();
  return {
    data: json,
    status: response.status,
    headers: response.headers,
  } as T;
};

export default apiFetch;
