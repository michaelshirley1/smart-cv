export interface TagOption {
    id: number;
    name: string;
    colour: string;
}

export interface TagInputProps {
    tags: TagOption[];
    selectedIds: number[];
    onChange: (ids: number[]) => void;
    onCreate: (name: string) => Promise<number | null>;
    placeholder?: string;
}
