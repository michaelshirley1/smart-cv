export interface FontFieldProps {
    label: string;
    value: string;
    fonts: string[];
    onChange: (value: string) => void;
    className?: string;
}

const genericFamilies = ['serif', 'sans-serif', 'monospace'];

export const primaryFamily = (stack: string) =>
    (stack.split(',')[0] ?? '').trim().replace(/^["']|["']$/g, '');

export const withFamily = (stack: string, family: string) => {
    const generic = stack.split(',').map(part => part.trim()).find(part => genericFamilies.includes(part)) ?? 'sans-serif';
    return `"${family.replace(/"/g, '')}", ${generic}`;
};
