export interface NameDialogProps {
    title: string;
    label: string;
    confirmLabel: string;
    initialName: string;
    placeholder?: string;
    maxLength?: number;
    onSubmit: (name: string) => Promise<void>;
    onClose: () => void;
}
