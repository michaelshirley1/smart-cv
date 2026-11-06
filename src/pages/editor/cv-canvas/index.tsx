import React, { useRef, useState } from 'react';
import { CvCanvasProps } from './model';
import { useProfileContext } from '../profile-context';
import { useFitZoom } from '../../../hooks/useFitZoom';
import { CvEdits } from '../cv-wrapper/model';
import * as api from '../api';
import CvWrapper from '../cv-wrapper';

import './styles.scss';

const CvCanvas: React.FC<CvCanvasProps> = () => {
    const { profile, mutate } = useProfileContext();
    const [loadedTemplate, setLoadedTemplate] = useState<string | null>(null);
    const canvas = useRef<HTMLDivElement>(null);
    const probe = useRef<HTMLDivElement>(null);
    const zoom = useFitZoom(canvas, probe);

    const templateName = profile?.templateName;
    const ready = profile !== null && loadedTemplate === templateName;

    const edits: CvEdits | undefined = profile ? {
        saveProfile: request => mutate(() => api.updateProfile(profile.id, request), 'Could not save your details.'),
        saveSection: (id, request) => mutate(() => api.updateSection(id, request), 'Could not save the section.'),
        saveEntry: (id, request) => mutate(() => api.updateEntry(id, request), 'Could not save the entry.'),
        saveLine: (id, request) => mutate(() => api.updateLine(id, request), 'Could not save the line.'),
    } : undefined;

    return (
        <div className="cv-canvas" ref={canvas}>
            {templateName && (
                <link
                    rel="stylesheet"
                    href={`/templates/${templateName}.css`}
                    onLoad={() => setLoadedTemplate(templateName)}
                    onError={() => setLoadedTemplate(templateName)}
                />
            )}
            <div className="cv-wrapper cv-canvas-probe" ref={probe} aria-hidden="true" />
            <div className={`cv-canvas-scaler cv-canvas-scaler--zoom-${zoom}${ready ? '' : ' cv-canvas-scaler--loading'}`}>
                {profile && <CvWrapper profile={profile} variant="editor" showPlaceholders edits={edits} />}
            </div>
        </div>
    );
};

export default CvCanvas;
