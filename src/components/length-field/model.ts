export interface LengthFieldProps {
    label: string;
    value: string;
    onChange: (value: string) => void;
    step?: number;
    max?: number;
    className?: string;
}

const lengthPattern = /^(\d*\.?\d+)(pt|px|mm|cm|in|em|rem)$/;

export const parseLength = (value: string) => {
    const match = lengthPattern.exec(value.trim());
    return match ? { amount: Number.parseFloat(match[1]), unit: match[2] } : null;
};

export const defaultStep = (unit: string) => (unit === 'in' || unit === 'em' || unit === 'rem' || unit === 'cm' ? 0.05 : 0.5);
