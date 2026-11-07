import React, { useEffect, useRef, useState } from 'react';
import { ProfileDialogKind, ProfileSwitcherProps } from './model';
import { useProfileContext } from '../../profile-context';
import Icon from '../../../../components/icon';
import Button from '../../../../components/button';
import Modal from '../../../../components/modal';
import NameDialog from '../../../../components/name-dialog';

import './styles.scss';

const ProfileSwitcher: React.FC<ProfileSwitcherProps> = () => {
    const { profiles, profile, loading, selectProfile, createProfile, renameProfile, duplicateProfile, deleteProfile, importProfile } = useProfileContext();
    const fileInput = useRef<HTMLInputElement>(null);
    const [importing, setImporting] = useState(false);

    const handleFile = async (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        e.target.value = '';
        if (!file) return;
        setImporting(true);
        await importProfile(file);
        setImporting(false);
    };
    const [open, setOpen] = useState(false);
    const [dialog, setDialog] = useState<ProfileDialogKind>(null);
    const container = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (!open) return;

        const handleMouseDown = (e: MouseEvent) => {
            if (!container.current?.contains(e.target as Node)) setOpen(false);
        };
        const handleKeyDown = (e: KeyboardEvent) => {
            if (e.key === 'Escape') setOpen(false);
        };

        document.addEventListener('mousedown', handleMouseDown);
        document.addEventListener('keydown', handleKeyDown);
        return () => {
            document.removeEventListener('mousedown', handleMouseDown);
            document.removeEventListener('keydown', handleKeyDown);
        };
    }, [open]);

    const choose = (action: () => void) => {
        setOpen(false);
        action();
    };

    const closeDialog = () => setDialog(null);

    return (
        <div className="profile-switcher" ref={container}>
            <button
                type="button"
                className="profile-switcher-trigger"
                aria-haspopup="menu"
                aria-expanded={open}
                disabled={loading || !profile}
                onClick={() => setOpen(current => !current)}
            >
                <span className="profile-switcher-label">{importing ? 'Importing…' : profile?.name ?? 'Loading'}</span>
                <Icon name="chevron-down" />
            </button>

            {open && (
                <div className="profile-switcher-menu" role="menu">
                    <ul className="profile-switcher-list">
                        {profiles.map(p => (
                            <li key={p.id}>
                                <button
                                    type="button"
                                    role="menuitemradio"
                                    aria-checked={p.id === profile?.id}
                                    className={`profile-switcher-option${p.id === profile?.id ? ' profile-switcher-option--active' : ''}`}
                                    onClick={() => choose(() => p.id !== profile?.id && selectProfile(p.id))}
                                >
                                    <span className="profile-switcher-option-name">{p.name}</span>
                                    {p.id === profile?.id && <Icon name="check" />}
                                </button>
                            </li>
                        ))}
                    </ul>
                    <div className="profile-switcher-actions">
                        <button type="button" role="menuitem" className="profile-switcher-action" onClick={() => choose(() => setDialog('create'))}>New profile</button>
                        <button type="button" role="menuitem" className="profile-switcher-action" onClick={() => choose(() => fileInput.current?.click())}>Import CV…</button>
                        <button type="button" role="menuitem" className="profile-switcher-action" onClick={() => choose(() => setDialog('rename'))}>Rename</button>
                        <button type="button" role="menuitem" className="profile-switcher-action" onClick={() => choose(duplicateProfile)}>Duplicate</button>
                        <button type="button" role="menuitem" className="profile-switcher-action profile-switcher-action--danger" onClick={() => choose(() => setDialog('delete'))}>Delete</button>
                    </div>
                </div>
            )}

            <input
                ref={fileInput}
                type="file"
                className="profile-switcher-file"
                accept=".docx,.pdf,.txt,.md"
                aria-label="Choose a CV to import"
                onChange={handleFile}
            />

            {dialog === 'create' && (
                <NameDialog title="New profile" label="Profile name" placeholder="e.g. Backend roles" confirmLabel="Create" initialName="" onSubmit={createProfile} onClose={closeDialog} />
            )}

            {dialog === 'rename' && profile && (
                <NameDialog title="Rename profile" label="Profile name" confirmLabel="Save" initialName={profile.name} onSubmit={renameProfile} onClose={closeDialog} />
            )}

            {dialog === 'delete' && profile && (
                <Modal
                    title="Delete profile"
                    onClose={closeDialog}
                    actions={(
                        <>
                            <Button variant="secondary" onClick={closeDialog}>Cancel</Button>
                            <Button variant="danger" autoFocus onClick={() => { closeDialog(); deleteProfile(); }}>Delete</Button>
                        </>
                    )}
                >
                    <p>Delete <strong>{profile.name}</strong> and all of its sections, lines and tags? This can't be undone.</p>
                </Modal>
            )}
        </div>
    );
};

export default ProfileSwitcher;
