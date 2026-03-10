const BASE_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5054";

export class ApiError extends Error {
  status: number;
  errors: string[] | null;

  constructor(status: number, message: string, errors: string[] | null = null) {
    super(message);
    this.status = status;
    this.errors = errors;
  }
}

export const apiFetch = async <T>(
  url: string,
  options?: RequestInit
): Promise<T> => {
  const response = await fetch(`${BASE_URL}${url}`, {
    ...options,
    headers: {
      ...options?.headers,
    },
  });

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
