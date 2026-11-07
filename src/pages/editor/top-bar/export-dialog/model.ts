export interface ExportDialogProps {
    profileId: number;
    sectionIds: number[] | null;
    lineIds: number[] | null;
    onSaved: (path: string) => void;
    onClose: () => void;
}

export const withPdfExtension = (name: string) => (name.trim().toLowerCase().endsWith('.pdf') ? name.trim() : `${name.trim()}.pdf`);
