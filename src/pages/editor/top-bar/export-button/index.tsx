import React, { useEffect, useState } from 'react';
import { ExportButtonProps, fileName } from './model';
import { useProfileContext } from '../../profile-context';
import { useTailoring } from '../../tailoring-context';
import * as api from '../../api';
import { useShortcut, withCommand } from '../../../../hooks/useShortcut';
import Button from '../../../../components/button';

import './styles.scss';

const noticeDuration = 6000;

const ExportButton: React.FC<ExportButtonProps> = () => {
    const { profile, attempt } = useProfileContext();
    const { result, isTailored, fit } = useTailoring();
    const [exporting, setExporting] = useState(false);
    const [savedPath, setSavedPath] = useState<string | null>(null);

    useEffect(() => {
        if (!savedPath) return;
        const timer = window.setTimeout(() => setSavedPath(null), noticeDuration);
        return () => window.clearTimeout(timer);
    }, [savedPath]);

    const exportPdf = async () => {
        if (!profile || exporting) return;
        setExporting(true);
        setSavedPath(null);
        const tailored = isTailored && result;
        await attempt(async () => {
            const outcome = await api.exportPdf({
                profileId: profile.id,
                sectionIds: tailored ? result.sections.filter(section => section.included).map(section => section.sectionId) : null,
                lineIds: tailored
                    ? result.lines.filter(line => line.included && !fit.trimmedIds.includes(line.lineId)).map(line => line.lineId)
                    : null,
            });
            if (outcome.saved && outcome.path) setSavedPath(outcome.path);
        }, 'Could not export the PDF.');
        setExporting(false);
    };

    useShortcut(e => withCommand(e) && e.key.toLowerCase() === 'p', exportPdf, true);

    return (
        <div className="export-button">
            {savedPath && (
                <span className="export-button-saved" role="status" title={savedPath}>Saved {fileName(savedPath)}</span>
            )}
            <Button variant="secondary" onClick={exportPdf} disabled={!profile || exporting} title="Export PDF (Ctrl+P)">
                {exporting ? 'Exporting…' : 'Export PDF'}
            </Button>
        </div>
    );
};

export default ExportButton;
