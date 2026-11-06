export interface ColourFieldProps {
    label: string;
    value: string;
    onChange: (value: string) => void;
    className?: string;
}

export const isHexColour = (value: string) => /^#[0-9a-f]{6}$/i.test(value.trim());
