export interface CvCanvasProps {}

const tolerance = 0.5;

export const linesToTrim = (page: HTMLElement, trimOrder: number[], overflow: number) => {
    const heights = new Map<number, number>();
    for (const id of trimOrder) {
        const line = page.querySelector<HTMLElement>(`[data-line-id="${id}"]`);
        if (line) heights.set(id, line.offsetHeight);
    }

    const trimmed: number[] = [];
    let removed = 0;
    for (const [id, height] of heights) {
        if (removed >= overflow - tolerance) break;
        removed += height;
        trimmed.push(id);
    }

    const fits = overflow - removed <= tolerance;
    if (!fits) return { trimmed, fits };

    const needed = [...trimmed].reverse().filter(id => {
        const height = heights.get(id) ?? 0;
        if (removed - height < overflow - tolerance) return true;
        removed -= height;
        return false;
    });

    return { trimmed: trimmed.filter(id => needed.includes(id)), fits };
};

export const pageLabel = (pages: number) => {
    const rounded = Math.ceil(pages * 10 - 0.05) / 10;
    return rounded <= 1 ? '1 page' : `${rounded.toFixed(1)} pages`;
};
