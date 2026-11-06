import React from 'react';

export interface ConfirmDialogProps {
    title: string;
    message: React.ReactNode;
    confirmLabel: string;
    onConfirm: () => void;
    onClose: () => void;
}
