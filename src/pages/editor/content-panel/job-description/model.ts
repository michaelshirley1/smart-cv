import { AppSettings, Tag, TailorResult } from '../../model';

export interface JobDescriptionProps {
    tags: Tag[];
}

export const modeLabel = (result: TailorResult | null, settings: AppSettings | null) => {
    if (result?.source === 'tags') return 'Tailored to chosen tags';
    if (result) return result.mode === 'ollama' ? `Ollama · ${result.model}` : 'Keyword matching';
    if (settings?.matchingMode === 'ollama') return `Ollama · ${settings.ollamaModel || 'no model chosen'}`;
    return 'Keyword matching';
};

export const busyLabel = (settings: AppSettings | null) =>
    settings?.matchingMode === 'ollama' ? 'Asking Ollama…' : 'Tailoring…';
