import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

//Vite konfiguracija, ali sam dodao i podatke o serveru da ne bi CORS zabranio slanje podataka
//Sve rute koje pocinju sa /api ce znati da treba da itu na target adresu
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      "/api": {
        target: "http://DESKTOP-PRL97FA:8453",
        changeOrigin: true,
        secure: false,
      },
    },
  },
});
