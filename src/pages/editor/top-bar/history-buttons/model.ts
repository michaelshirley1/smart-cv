export interface HistoryButtonsProps {}

export const isUndo = (e: KeyboardEvent) => (e.ctrlKey || e.metaKey) && !e.shiftKey && e.key.toLowerCase() === 'z';

export const isRedo = (e: KeyboardEvent) =>
    (e.ctrlKey || e.metaKey) && ((e.shiftKey && e.key.toLowerCase() === 'z') || e.key.toLowerCase() === 'y');
