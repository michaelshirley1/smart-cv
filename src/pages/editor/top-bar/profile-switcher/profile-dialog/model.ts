export interface ProfileDialogProps {
    title: string;
    confirmLabel: string;
    initialName: string;
    onSubmit: (name: string) => Promise<void>;
    onClose: () => void;
}
