import { useEffect, useRef } from 'react';

const isEditable = (target: EventTarget | null) =>
    target instanceof HTMLElement && (target.isContentEditable || ['INPUT', 'TEXTAREA', 'SELECT'].includes(target.tagName));

export const withCommand = (e: KeyboardEvent) => e.ctrlKey || e.metaKey;

export function useShortcut(matches: (e: KeyboardEvent) => boolean, handler: () => void, allowInFields = false) {
    const latest = useRef(handler);
    latest.current = handler;

    useEffect(() => {
        const handleKeyDown = (e: KeyboardEvent) => {
            if (!matches(e) || (!allowInFields && isEditable(e.target))) return;
            e.preventDefault();
            latest.current();
        };

        window.addEventListener('keydown', handleKeyDown);
        return () => window.removeEventListener('keydown', handleKeyDown);
    }, []);
}
