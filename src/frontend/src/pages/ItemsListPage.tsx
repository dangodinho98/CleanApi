import { useCallback, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import * as api from "../api/client";
import type { ItemResponse } from "../api/types";

export function ItemsListPage() {
  const [items, setItems] = useState<ItemResponse[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [editingId, setEditingId] = useState<string | null>(null);
  const [editTitle, setEditTitle] = useState("");
  const [editDescription, setEditDescription] = useState("");

  const refresh = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setItems(await api.listItems());
    } catch (e) {
      setError(e instanceof Error ? e.message : "Failed to load items");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  async function onDelete(id: string) {
    if (!window.confirm("Delete this item?")) return;
    setError(null);
    try {
      await api.deleteItem(id);
      await refresh();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Delete failed");
    }
  }

  function startEdit(item: ItemResponse) {
    setEditingId(item.id);
    setEditTitle(item.title);
    setEditDescription(item.description);
  }

  async function saveEdit(e: React.FormEvent) {
    e.preventDefault();
    if (!editingId) return;
    setError(null);
    try {
      await api.updateItem(editingId, editTitle, editDescription);
      setEditingId(null);
      await refresh();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Update failed");
    }
  }

  return (
    <section className="card">
      <div className="row space-between">
        <h2>Items</h2>
        <div className="row">
          <Link to="/items/new" className="button-link">
            New item
          </Link>
          <button type="button" className="ghost" onClick={() => void refresh()} disabled={loading}>
            Refresh
          </button>
        </div>
      </div>

      {error ? <p className="error">{error}</p> : null}

      <div className="items">
        {loading ? <p className="muted">Loading…</p> : null}
        {!loading && items.length === 0 ? (
          <p className="muted">
            No items yet.{" "}
            <Link to="/items/new">Create one</Link>.
          </p>
        ) : null}
        {items.map((item) => (
          <article key={item.id} className="item">
            {editingId === item.id ? (
              <form className="form tight" onSubmit={saveEdit}>
                <label>
                  Title
                  <input value={editTitle} onChange={(e) => setEditTitle(e.target.value)} />
                </label>
                <label>
                  Description
                  <textarea value={editDescription} onChange={(e) => setEditDescription(e.target.value)} rows={3} />
                </label>
                <div className="row">
                  <button type="submit">Save</button>
                  <button type="button" className="ghost" onClick={() => setEditingId(null)}>
                    Cancel
                  </button>
                </div>
              </form>
            ) : (
              <>
                <header className="row space-between">
                  <h3 className="h3">{item.title}</h3>
                  <div className="row">
                    <button type="button" className="ghost" onClick={() => startEdit(item)}>
                      Edit
                    </button>
                    <button type="button" className="danger" onClick={() => void onDelete(item.id)}>
                      Delete
                    </button>
                  </div>
                </header>
                <p className="muted small">{new Date(item.createdAtUtc).toLocaleString()}</p>
                <p>{item.description}</p>
              </>
            )}
          </article>
        ))}
      </div>
    </section>
  );
}
