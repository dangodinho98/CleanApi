import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export function AppLayout() {
  const { user, meError, clearMeError, logout } = useAuth();

  return (
    <div className="page">
      <header className="topbar">
        <div className="brand-block">
          <NavLink to="/" className="brand" end>
            <h1>CleanApi</h1>
          </NavLink>
        </div>
      </header>

      <nav className="main-nav" aria-label="Main">
        {user ? (
          <>
            <NavLink to="/items" className={({ isActive }) => `nav-link${isActive ? " active" : ""}`} end>
              Items
            </NavLink>
            <NavLink to="/items/new" className={({ isActive }) => `nav-link${isActive ? " active" : ""}`}>
              Add item
            </NavLink>
            <span className="nav-spacer" />
            <span className="nav-user muted small">
              {user.displayName} <span className="muted">({user.email})</span>
            </span>
            <button type="button" className="ghost nav-signout" onClick={logout}>
              Sign out
            </button>
          </>
        ) : (
          <NavLink to="/auth" className={({ isActive }) => `nav-link${isActive ? " active" : ""}`}>
            Sign in / Register
          </NavLink>
        )}
      </nav>

      {meError ? (
        <p className="error banner banner-dismissible">
          {meError}{" "}
          <button type="button" className="ghost inline-dismiss" onClick={clearMeError}>
            Dismiss
          </button>
        </p>
      ) : null}

      <main className="main-outlet">
        <Outlet />
      </main>
    </div>
  );
}
