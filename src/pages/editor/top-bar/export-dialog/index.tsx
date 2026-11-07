import React, { useEffect, useState } from 'react';
import { ExportDialogProps, withPdfExtension } from './model';
import { getErrorMessage } from '../../../../api/errors';
import * as api from '../../api';
import Modal from '../../../../components/modal';
import TextField from '../../../../components/text-field';
import Button from '../../../../components/button';

import './styles.scss';

const ExportDialog: React.FC<ExportDialogProps> = ({ profileId, sectionIds, lineIds, onSaved, onClose }) => {
    const [folder, setFolder] = useState('');
    const [fileName, setFileName] = useState('');
    const [canBrowse, setCanBrowse] = useState(false);
    const [loading, setLoading] = useState(true);
    const [browsing, setBrowsing] = useState(false);
    const [exporting, setExporting] = useState(false);
    const [existing, setExisting] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        api.getExportLocation(profileId)
            .then(location => {
                setFolder(location.folder);
                setFileName(location.fileName);
                setCanBrowse(location.canBrowse);
            })
            .catch(e => setError(getErrorMessage(e, 'Could not find a folder to save to.')))
            .finally(() => setLoading(false));
    }, [profileId]);

    const changeFolder = (value: string) => {
        setFolder(value);
        setExisting(null);
    };

    const changeFileName = (value: string) => {
        setFileName(value);
        setExisting(null);
    };

    const browse = async () => {
        setBrowsing(true);
        setError(null);
        try {
            const chosen = await api.browseFolder(folder);
            if (chosen.folder) changeFolder(chosen.folder);
        } catch (e) {
            setError(getErrorMessage(e, 'Could not open the folder picker.'));
        } finally {
            setBrowsing(false);
        }
    };

    const canExport = !loading && !exporting && folder.trim().length > 0 && fileName.trim().length > 0;

    const submit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!canExport) return;
        setExporting(true);
        setError(null);
        try {
            const outcome = await api.exportPdf({
                profileId,
                sectionIds,
                lineIds,
                folder: folder.trim(),
                fileName: withPdfExtension(fileName),
                overwrite: existing !== null,
            });
            if (outcome.exists) {
                setExisting(outcome.path);
            } else if (outcome.saved && outcome.path) {
                onSaved(outcome.path);
                onClose();
            }
        } catch (e) {
            setError(getErrorMessage(e, 'Could not export the PDF.'));
        } finally {
            setExporting(false);
        }
    };

    return (
        <Modal title="Export PDF" onClose={onClose}>
            <form className="export-dialog" onSubmit={submit}>
                <TextField
                    label="File name"
                    value={fileName}
                    onChange={changeFileName}
                    maxLength={200}
                    disabled={loading}
                    spellCheck={false}
                    autoFocus
                />
                <div className="export-dialog-folder">
                    <TextField
                        className="export-dialog-folder-input"
                        label="Save to folder"
                        value={folder}
                        onChange={changeFolder}
                        maxLength={1000}
                        disabled={loading}
                        spellCheck={false}
                    />
                    {canBrowse && (
                        <Button variant="secondary" onClick={browse} disabled={loading || browsing}>
                            {browsing ? 'Choosing…' : 'Browse…'}
                        </Button>
                    )}
                </div>
                <p className="export-dialog-hint">The folder is remembered for next time.</p>
                {existing && (
                    <p className="export-dialog-warning" role="alert">
                        {withPdfExtension(fileName)} already exists in this folder. Export again to replace it, or change the name.
                    </p>
                )}
                {error && <p className="export-dialog-error" role="alert">{error}</p>}
                <div className="export-dialog-actions">
                    <Button variant="secondary" onClick={onClose}>Cancel</Button>
                    <Button type="submit" variant={existing ? 'danger' : 'primary'} disabled={!canExport}>
                        {exporting ? 'Exporting…' : existing ? 'Replace' : 'Export'}
                    </Button>
                </div>
            </form>
        </Modal>
    );
};

export default ExportDialog;
