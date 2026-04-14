import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import { AuthProvider } from "./context/AuthContext";
import { AppLayout } from "./layouts/AppLayout";
import { ProtectedRoute } from "./layouts/ProtectedRoute";
import { AddItemPage } from "./pages/AddItemPage";
import { AuthPage } from "./pages/AuthPage";
import { ItemsListPage } from "./pages/ItemsListPage";

export default function App() {
  return (
    <BrowserRouter basename={import.meta.env.BASE_URL}>
      <AuthProvider>
        <Routes>
          <Route element={<AppLayout />}>
            <Route index element={<Navigate to="/items" replace />} />
            <Route path="auth" element={<AuthPage />} />
            <Route
              path="items"
              element={
                <ProtectedRoute>
                  <ItemsListPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="items/new"
              element={
                <ProtectedRoute>
                  <AddItemPage />
                </ProtectedRoute>
              }
            />
            <Route path="*" element={<Navigate to="/items" replace />} />
          </Route>
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}
