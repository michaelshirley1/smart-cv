import React from 'react';

export interface ModalProps {
    title: string;
    onClose: () => void;
    actions?: React.ReactNode;
    children: React.ReactNode;
}
