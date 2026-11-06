export interface SelectOption<T extends string> {
    value: T;
    label: string;
}

export interface SelectProps<T extends string> {
    label: string;
    value: T;
    options: SelectOption<T>[];
    onChange: (value: T) => void;
    className?: string;
}
