import React, { useState } from 'react';
import { NameDialogProps } from './model';
import Modal from '../modal';
import TextField from '../text-field';
import Button from '../button';

import './styles.scss';

const NameDialog: React.FC<NameDialogProps> = ({ title, label, confirmLabel, initialName, placeholder, maxLength = 100, onSubmit, onClose }) => {
    const [name, setName] = useState(initialName);
    const [saving, setSaving] = useState(false);
    const trimmed = name.trim();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!trimmed || saving) return;
        setSaving(true);
        await onSubmit(trimmed);
        onClose();
    };

    return (
        <Modal title={title} onClose={onClose}>
            <form className="name-dialog" onSubmit={handleSubmit}>
                <TextField
                    label={label}
                    value={name}
                    onChange={setName}
                    maxLength={maxLength}
                    placeholder={placeholder}
                    autoFocus
                />
                <div className="name-dialog-actions">
                    <Button variant="secondary" onClick={onClose}>Cancel</Button>
                    <Button type="submit" disabled={!trimmed || saving}>{confirmLabel}</Button>
                </div>
            </form>
        </Modal>
    );
};

export default NameDialog;
