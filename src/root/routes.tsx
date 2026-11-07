import { Routes, Route } from 'react-router-dom';
import EditorPage from '../pages/editor';
import PrintPage from '../pages/print';

export default function App() {
  return (
    <Routes>
      <Route path="/print/:jobId" element={<PrintPage />} />
      <Route path="*" element={<EditorPage />} />
    </Routes>
  );
}
