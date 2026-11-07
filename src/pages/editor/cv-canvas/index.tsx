import React, { useLayoutEffect, useRef, useState } from 'react';
import { CvCanvasProps, linesToTrim, pageLabel } from './model';
import { useProfileContext } from '../profile-context';
import { useTailoring } from '../tailoring-context';
import { useFitZoom } from '../../../hooks/useFitZoom';
import { CvEdits, CvSelection } from '../cv-wrapper/model';
import * as api from '../api';
import CvWrapper from '../cv-wrapper';

import './styles.scss';

const CvCanvas: React.FC<CvCanvasProps> = () => {
    const { profile, mutate, templateRevision } = useProfileContext();
    const { result, isTailored, fit, setFit, pages, setPages } = useTailoring();
    const [loadedHref, setLoadedHref] = useState<string | null>(null);
    const canvas = useRef<HTMLDivElement>(null);
    const probe = useRef<HTMLDivElement>(null);
    const scaler = useRef<HTMLDivElement>(null);
    const zoom = useFitZoom(canvas, probe);

    const href = profile ? `/templates/${profile.templateName}.css?v=${templateRevision}` : null;
    const ready = profile !== null && loadedHref !== null;
    const stylesheets = [...new Set([loadedHref, href])].filter((value): value is string => value !== null);
    const targetPages = profile?.targetPages ?? 1;

    const fitKey = isTailored && result ? JSON.stringify([result, targetPages, href]) : '';
    const trimmedIds = isTailored && fit.key === fitKey ? fit.trimmedIds : [];

    const selection: CvSelection | undefined = isTailored && result ? {
        sectionIds: new Set(result.sections.filter(section => section.included).map(section => section.sectionId)),
        lineIds: new Set(result.lines.filter(line => line.included && !trimmedIds.includes(line.lineId)).map(line => line.lineId)),
    } : undefined;

    useLayoutEffect(() => {
        const page = scaler.current?.querySelector<HTMLElement>('.cv-page');
        const reference = probe.current;
        if (!ready || !page || !reference || reference.offsetWidth === 0) return;

        const pageHeight = page.offsetWidth * (reference.offsetHeight / reference.offsetWidth);
        if (isTailored && result && fit.key !== fitKey) {
            const overflow = targetPages === 0 ? 0 : page.offsetHeight - pageHeight * targetPages;
            const { trimmed, fits } = linesToTrim(page, result.trimOrder, overflow);
            setFit({ key: fitKey, trimmedIds: trimmed, fits });
            return;
        }
        setPages(page.offsetHeight / pageHeight);
    });

    const edits: CvEdits | undefined = profile ? {
        saveProfile: request => mutate(() => api.updateProfile(profile.id, request), 'Could not save your details.'),
        saveSection: (id, request) => mutate(() => api.updateSection(id, request), 'Could not save the section.'),
        saveEntry: (id, request) => mutate(() => api.updateEntry(id, request), 'Could not save the entry.'),
        saveLine: (id, request) => mutate(() => api.updateLine(id, request), 'Could not save the line.'),
    } : undefined;

    const target = targetPages === 0 ? 'no page limit' : `target ${targetPages} page${targetPages === 1 ? '' : 's'}`;
    const status = !isTailored
        ? `All lines · ${pageLabel(pages)} · ${target}`
        : !fit.fits
            ? `Tailored · ${pageLabel(pages)} · still over the ${target} with every optional line trimmed`
            : trimmedIds.length > 0
                ? `Tailored · ${pageLabel(pages)} · ${trimmedIds.length} line${trimmedIds.length === 1 ? '' : 's'} trimmed to fit`
                : `Tailored · ${pageLabel(pages)} · ${target}`;

    return (
        <div className="cv-canvas" ref={canvas}>
            {stylesheets.map(stylesheet => (
                <link
                    key={stylesheet}
                    rel="stylesheet"
                    href={stylesheet}
                    onLoad={() => stylesheet === href && setLoadedHref(stylesheet)}
                    onError={() => stylesheet === href && setLoadedHref(stylesheet)}
                />
            ))}
            <div className="cv-wrapper cv-canvas-probe" ref={probe} aria-hidden="true" />
            {profile && ready && (
                <p className={`cv-canvas-status${isTailored && !fit.fits ? ' cv-canvas-status--over' : ''}`} role="status">{status}</p>
            )}
            <div ref={scaler} className={`cv-canvas-scaler cv-canvas-scaler--zoom-${zoom}${ready ? '' : ' cv-canvas-scaler--loading'}`}>
                {profile && (
                    <CvWrapper
                        profile={profile}
                        variant="editor"
                        showPlaceholders={!isTailored}
                        selection={selection}
                        edits={edits}
                    />
                )}
            </div>
        </div>
    );
};

export default CvCanvas;
