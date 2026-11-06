import React, { useId } from 'react';
import { ModalProps } from './model';
import IconButton from '../icon-button';

import './styles.scss';

const Modal: React.FC<ModalProps> = ({ title, onClose, actions, children }) => {
    const titleId = useId();

    const handleKeyDown = (e: React.KeyboardEvent) => {
        if (e.key !== 'Escape') return;
        e.stopPropagation();
        onClose();
    };

    return (
        <div
            className="modal-overlay"
            onMouseDown={e => e.target === e.currentTarget && onClose()}
            onKeyDown={handleKeyDown}
        >
            <div className="modal" role="dialog" aria-modal="true" aria-labelledby={titleId}>
                <header className="modal-header">
                    <h2 className="modal-title" id={titleId}>{title}</h2>
                    <IconButton icon="close" label="Close" onClick={onClose} />
                </header>
                <div className="modal-body">{children}</div>
                {actions && <footer className="modal-actions">{actions}</footer>}
            </div>
        </div>
    );
};

export default Modal;
