import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import { useNavigate } from "react-router-dom";
import * as api from "../api/client";
import type { UserSummary } from "../api/types";

type AuthContextValue = {
  user: UserSummary | null;
  setUser: (user: UserSummary | null) => void;
  ready: boolean;
  meError: string | null;
  clearMeError: () => void;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const navigate = useNavigate();
  const [user, setUser] = useState<UserSummary | null>(null);
  const [ready, setReady] = useState(false);
  const [meError, setMeError] = useState<string | null>(null);

  useEffect(() => {
    const token = api.getToken();
    if (!token) {
      setReady(true);
      return;
    }

    let cancelled = false;
    (async () => {
      try {
        const u = await api.me();
        if (!cancelled) setUser(u);
      } catch {
        if (!cancelled) {
          api.setToken(null);
          setUser(null);
          setMeError("Session expired. Please sign in again.");
        }
      } finally {
        if (!cancelled) setReady(true);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  const clearMeError = useCallback(() => setMeError(null), []);

  const logout = useCallback(() => {
    api.setToken(null);
    setUser(null);
    navigate("/auth", { replace: true });
  }, [navigate]);

  const value = useMemo(
    () => ({ user, setUser, ready, meError, clearMeError, logout }),
    [user, ready, meError, clearMeError, logout]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}
