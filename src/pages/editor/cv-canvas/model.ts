export interface CvCanvasProps {}

const tolerance = 0.5;

export const linesToTrim = (page: HTMLElement, trimOrder: number[], overflow: number) => {
    const trimmed: number[] = [];
    let removed = 0;

    for (const id of trimOrder) {
        if (removed >= overflow - tolerance) break;
        const line = page.querySelector<HTMLElement>(`[data-line-id="${id}"]`);
        if (!line) continue;
        removed += line.offsetHeight;
        trimmed.push(id);
    }

    return { trimmed, fits: overflow - removed <= tolerance };
};

export const pageLabel = (pages: number) => {
    const rounded = Math.ceil(pages * 10 - 0.05) / 10;
    return rounded <= 1 ? '1 page' : `${rounded.toFixed(1)} pages`;
};
