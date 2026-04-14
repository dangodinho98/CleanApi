import { Navigate, useLocation, useNavigate } from "react-router-dom";
import { AuthPanel } from "../features/auth/AuthPanel";
import { useAuth } from "../context/AuthContext";

export function AuthPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const { user, ready, setUser, clearMeError } = useAuth();

  const from = (location.state as { from?: string } | null)?.from ?? "/items";

  if (!ready) {
    return (
      <section className="card subtle">
        <p className="muted">Loading…</p>
      </section>
    );
  }

  if (user) {
    return <Navigate to="/items" replace />;
  }

  return (
    <AuthPanel
      onAuthed={(u) => {
        clearMeError();
        setUser(u);
        navigate(from, { replace: true });
      }}
    />
  );
}
