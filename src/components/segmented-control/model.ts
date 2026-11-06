export interface SegmentedOption<T extends string> {
    value: T;
    label: string;
    disabled?: boolean;
}

export interface SegmentedControlProps<T extends string> {
    label: string;
    value: T;
    options: SegmentedOption<T>[];
    onChange: (value: T) => void;
}
