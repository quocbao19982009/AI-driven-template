import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

const API_BASE_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5054";

export function assetUrl(
  relativePath: string | null | undefined
): string | undefined {
  if (!relativePath) return undefined;
  return `${API_BASE_URL}${relativePath}`;
}
