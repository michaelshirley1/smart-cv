import React, { useState } from 'react';
import { ProfileDialogProps } from './model';
import Modal from '../../../../../components/modal';
import TextField from '../../../../../components/text-field';
import Button from '../../../../../components/button';

import './styles.scss';

const maxNameLength = 100;

const ProfileDialog: React.FC<ProfileDialogProps> = ({ title, confirmLabel, initialName, onSubmit, onClose }) => {
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
            <form className="profile-dialog" onSubmit={handleSubmit}>
                <TextField
                    label="Profile name"
                    value={name}
                    onChange={setName}
                    maxLength={maxNameLength}
                    placeholder="e.g. Backend roles"
                    autoFocus
                />
                <div className="profile-dialog-actions">
                    <Button variant="secondary" onClick={onClose}>Cancel</Button>
                    <Button type="submit" disabled={!trimmed || saving}>{confirmLabel}</Button>
                </div>
            </form>
        </Modal>
    );
};

export default ProfileDialog;
