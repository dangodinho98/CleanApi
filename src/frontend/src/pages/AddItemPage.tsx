import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import * as api from "../api/client";

export function AddItemPage() {
  const navigate = useNavigate();
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setBusy(true);
    try {
      await api.createItem(title, description);
      navigate("/items", { replace: false });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Create failed");
    } finally {
      setBusy(false);
    }
  }

  return (
    <section className="card">
      <div className="row space-between">
        <h2>New item</h2>
        <Link to="/items" className="button-link ghost-link">
          ← Back to list
        </Link>
      </div>

      {error ? <p className="error">{error}</p> : null}

      <form className="form" onSubmit={onSubmit}>
        <label>
          Title
          <input value={title} onChange={(e) => setTitle(e.target.value)} autoFocus />
        </label>
        <label>
          Description
          <textarea value={description} onChange={(e) => setDescription(e.target.value)} rows={4} />
        </label>
        <div className="row">
          <button type="submit" disabled={busy || !title.trim()}>
            {busy ? "Saving…" : "Create item"}
          </button>
          <Link to="/items" className="button-link ghost-link">
            Cancel
          </Link>
        </div>
      </form>
    </section>
  );
}
