import type { AuthResponse, ItemResponse, UserSummary } from "./types";

const TOKEN_KEY = "cleanapi_token";

/** When set at build time (e.g. deploy to IIS while API runs on VS), requests go to Kestrel. Dev server uses Vite proxy instead. */
const API_BASE = (import.meta.env.VITE_API_BASE as string | undefined)?.replace(/\/$/, "") ?? "";

function apiUrl(path: string): string {
  const p = path.startsWith("/") ? path : `/${path}`;
  return API_BASE ? `${API_BASE}${p}` : p;
}

export function getToken(): string | null {
  return sessionStorage.getItem(TOKEN_KEY);
}

export function setToken(token: string | null): void {
  if (!token) sessionStorage.removeItem(TOKEN_KEY);
  else sessionStorage.setItem(TOKEN_KEY, token);
}

async function apiFetch(path: string, init: RequestInit = {}): Promise<Response> {
  const headers = new Headers(init.headers);
  if (!headers.has("Content-Type") && init.body && !(init.body instanceof FormData))
    headers.set("Content-Type", "application/json");

  const token = getToken();
  if (token) headers.set("Authorization", `Bearer ${token}`);

  const res = await fetch(apiUrl(path), { ...init, headers });
  return res;
}

export async function register(email: string, password: string, displayName: string): Promise<AuthResponse> {
  const res = await apiFetch("/api/auth/register", {
    method: "POST",
    body: JSON.stringify({ email, password, displayName })
  });
  if (!res.ok) {
    const err = await safeJson(res);
    throw new Error(errorMessageFromBody(err, `Register failed (${res.status})`));
  }
  return (await res.json()) as AuthResponse;
}

export async function login(email: string, password: string): Promise<AuthResponse> {
  const res = await apiFetch("/api/auth/login", {
    method: "POST",
    body: JSON.stringify({ email, password })
  });
  if (!res.ok) {
    if (res.status === 401) throw new Error("Invalid credentials");
    const err = await safeJson(res);
    throw new Error(errorMessageFromBody(err, `Login failed (${res.status})`));
  }
  return (await res.json()) as AuthResponse;
}

export async function me(): Promise<UserSummary> {
  const res = await apiFetch("/api/account/me");
  if (!res.ok) throw new Error(`Me failed (${res.status})`);
  return (await res.json()) as UserSummary;
}

export async function listItems(): Promise<ItemResponse[]> {
  const res = await apiFetch("/api/items");
  if (!res.ok) throw new Error(`List items failed (${res.status})`);
  return (await res.json()) as ItemResponse[];
}

export async function createItem(title: string, description: string): Promise<ItemResponse> {
  const res = await apiFetch("/api/items", {
    method: "POST",
    body: JSON.stringify({ title, description })
  });
  if (!res.ok) {
    const err = await safeJson(res);
    throw new Error(errorMessageFromBody(err, `Create failed (${res.status})`));
  }
  return (await res.json()) as ItemResponse;
}

export async function updateItem(id: string, title: string, description: string): Promise<void> {
  const res = await apiFetch(`/api/items/${id}`, {
    method: "PUT",
    body: JSON.stringify({ title, description })
  });
  if (!res.ok) {
    const err = await safeJson(res);
    throw new Error(errorMessageFromBody(err, `Update failed (${res.status})`));
  }
}

export async function deleteItem(id: string): Promise<void> {
  const res = await apiFetch(`/api/items/${id}`, { method: "DELETE" });
  if (!res.ok) throw new Error(`Delete failed (${res.status})`);
}

type ApiErrorBody = {
  title?: string;
  error?: string;
  errors?: Record<string, string[]>;
};

async function safeJson(res: Response): Promise<ApiErrorBody | null> {
  try {
    return (await res.json()) as ApiErrorBody;
  } catch {
    return null;
  }
}

/** ASP.NET validation problems use `errors`; controllers often return `{ error: string }`. */
function errorMessageFromBody(data: ApiErrorBody | null, fallback: string): string {
  if (!data) return fallback;
  if (typeof data.error === "string" && data.error.length > 0) return data.error;
  if (data.errors && typeof data.errors === "object") {
    const msgs = Object.values(data.errors)
      .flat()
      .filter((m): m is string => typeof m === "string" && m.length > 0);
    if (msgs.length > 0) return msgs.join(" ");
  }
  if (typeof data.title === "string" && data.title.length > 0) return data.title;
  return fallback;
}
