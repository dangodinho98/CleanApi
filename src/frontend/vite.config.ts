import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

/** Must match the IIS application URL path (e.g. http://localhost/CleanApi/). Change here if the IIS path changes. */
const iisAppBase = "/CleanApi/";

export default defineConfig({
  base: iisAppBase,
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      "/api": {
        target: "https://localhost:7288",
        changeOrigin: true,
        secure: false
      }
    }
  }
});
