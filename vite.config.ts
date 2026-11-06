import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

const apiServer = 'http://127.0.0.1:5080';

export default defineConfig({
  plugins: [react()],
  publicDir: 'public',
  build: {
    outDir: 'server/wwwroot',
    emptyOutDir: true,
  },
  server: {
    proxy: {
      '/api': apiServer,
      '/templates': apiServer,
    },
  },
});
