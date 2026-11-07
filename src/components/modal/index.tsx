import React, { useEffect, useId, useRef } from 'react';
import { ModalProps } from './model';
import IconButton from '../icon-button';

import './styles.scss';

const Modal: React.FC<ModalProps> = ({ title, onClose, actions, children }) => {
    const titleId = useId();
    const close = useRef(onClose);
    close.current = onClose;

    useEffect(() => {
        const handleKeyDown = (e: KeyboardEvent) => {
            if (e.key !== 'Escape') return;
            e.stopPropagation();
            close.current();
        };

        window.addEventListener('keydown', handleKeyDown, true);
        return () => window.removeEventListener('keydown', handleKeyDown, true);
    }, []);

    return (
        <div
            className="modal-overlay"
            onMouseDown={e => e.target === e.currentTarget && onClose()}
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
