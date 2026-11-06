import React from 'react';
import { ConfirmDialogProps } from './model';
import Modal from '../modal';
import Button from '../button';

const ConfirmDialog: React.FC<ConfirmDialogProps> = ({ title, message, confirmLabel, onConfirm, onClose }) => (
    <Modal
        title={title}
        onClose={onClose}
        actions={(
            <>
                <Button variant="secondary" onClick={onClose}>Cancel</Button>
                <Button
                    variant="danger"
                    autoFocus
                    onClick={() => {
                        onClose();
                        onConfirm();
                    }}
                >
                    {confirmLabel}
                </Button>
            </>
        )}
    >
        <p>{message}</p>
    </Modal>
);

export default ConfirmDialog;
