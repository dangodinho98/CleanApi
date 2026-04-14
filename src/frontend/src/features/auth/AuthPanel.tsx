import { useMemo, useState } from "react";
import * as api from "../../api/client";
import type { UserSummary } from "../../api/types";
import { useAuth } from "../../context/AuthContext";

type Props = {
  onAuthed: (user: UserSummary) => void;
};

export function AuthPanel({ onAuthed }: Props) {
  const { logout } = useAuth();
  const [mode, setMode] = useState<"login" | "register">("login");
  const [email, setEmail] = useState("demo@example.com");
  const [password, setPassword] = useState("Demo#123");
  const [displayName, setDisplayName] = useState("Demo User");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const title = useMemo(() => (mode === "login" ? "Sign in" : "Create account"), [mode]);

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setBusy(true);
    setError(null);
    try {
      if (mode === "login") {
        const auth = await api.login(email, password);
        api.setToken(auth.token);
        onAuthed(auth.user);
      } else {
        const auth = await api.register(email, password, displayName);
        api.setToken(auth.token);
        onAuthed(auth.user);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Something went wrong");
    } finally {
      setBusy(false);
    }
  }

  return (
    <section className="card auth-panel-card">
      <div className="row space-between">
        <h2>{title}</h2>
        <button type="button" className="ghost" onClick={logout}>
          Clear session
        </button>
      </div>

      <div className="segmented" role="tablist" aria-label="Auth mode">
        <button type="button" className={mode === "login" ? "active" : ""} onClick={() => setMode("login")}>
          Login
        </button>
        <button type="button" className={mode === "register" ? "active" : ""} onClick={() => setMode("register")}>
          Register
        </button>
      </div>

      <form className="form" onSubmit={submit}>
        <label>
          Email
          <input value={email} onChange={(e) => setEmail(e.target.value)} autoComplete="email" />
        </label>

        <label>
          Password
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            autoComplete={mode === "login" ? "current-password" : "new-password"}
          />
        </label>

        {mode === "register" ? (
          <label>
            Display name
            <input value={displayName} onChange={(e) => setDisplayName(e.target.value)} autoComplete="name" />
          </label>
        ) : null}

        {error ? <p className="error">{error}</p> : null}

        <button type="submit" disabled={busy}>
          {busy ? "Please wait…" : mode === "login" ? "Login" : "Register"}
        </button>
      </form>

      <p className="muted small">
        Demo seed user: <code>demo@example.com</code> / <code>Demo#123</code>
      </p>
    </section>
  );
}
