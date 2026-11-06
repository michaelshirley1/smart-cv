export interface EditorPageProps {}

export type MatchingMode = 'keyword' | 'ollama';

export interface AppSettings {
    matchingMode: MatchingMode;
    ollamaUrl: string;
    ollamaModel: string;
    lastProfileId: number | null;
}
