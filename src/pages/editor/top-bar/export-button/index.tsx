import React, { useEffect, useState } from 'react';
import { ExportButtonProps, fileName } from './model';
import { useProfileContext } from '../../profile-context';
import { useTailoring } from '../../tailoring-context';
import { useShortcut, withCommand } from '../../../../hooks/useShortcut';
import Button from '../../../../components/button';
import ExportDialog from '../export-dialog';

import './styles.scss';

const noticeDuration = 6000;

const ExportButton: React.FC<ExportButtonProps> = () => {
    const { profile } = useProfileContext();
    const { result, isTailored, fit } = useTailoring();
    const [open, setOpen] = useState(false);
    const [savedPath, setSavedPath] = useState<string | null>(null);

    useEffect(() => {
        if (!savedPath) return;
        const timer = window.setTimeout(() => setSavedPath(null), noticeDuration);
        return () => window.clearTimeout(timer);
    }, [savedPath]);

    const isEmpty = !profile || profile.sections.length === 0;
    const tailored = isTailored && result ? result : null;

    const openDialog = () => {
        if (isEmpty) return;
        setSavedPath(null);
        setOpen(true);
    };

    useShortcut(e => withCommand(e) && e.key.toLowerCase() === 'p', openDialog, true);

    return (
        <div className="export-button">
            {savedPath && (
                <span className="export-button-saved" role="status" title={savedPath}>Saved {fileName(savedPath)}</span>
            )}
            <Button variant="secondary" onClick={openDialog} disabled={isEmpty || open} title={isEmpty ? 'Add a section before exporting' : 'Export PDF (Ctrl+P)'}>
                Export PDF
            </Button>
            {open && profile && (
                <ExportDialog
                    profileId={profile.id}
                    sectionIds={tailored ? tailored.sections.filter(section => section.included).map(section => section.sectionId) : null}
                    lineIds={tailored
                        ? tailored.lines.filter(line => line.included && !fit.trimmedIds.includes(line.lineId)).map(line => line.lineId)
                        : null}
                    onSaved={setSavedPath}
                    onClose={() => setOpen(false)}
                />
            )}
        </div>
    );
};

export default ExportButton;
