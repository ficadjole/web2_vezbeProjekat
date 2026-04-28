import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      "/api": {
        target: "http://desktop-86sgm64:8453",
        changeOrigin: true,
        secure: false,
      },
    },
  },
});
