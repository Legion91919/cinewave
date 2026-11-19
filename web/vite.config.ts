/// <reference types="vitest" />
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// The API runs on 5104. /cdn stands in for the CDN host: in production the manifest URL is absolute and points elsewhere.
const api = 'http://localhost:5104';

export default defineConfig({
  plugins: [react()],
  server: { port: 5173, proxy: { '/api': api, '/cdn': api, '/health': api } },
  test: { environment: 'jsdom', globals: true },
});
