import React, { createContext, useContext, useEffect, useRef, useState } from 'react';
import { emptyFit, FitState, TailoringContextValue, TailoringProviderProps, TailoringSource } from './model';
import { lineStatusFor, sectionStatusFor } from './status';
import { CvView, TailorResult } from '../model';
import { useProfileContext } from '../profile-context';
import * as api from '../api';

const retailorDelay = 250;

const TailoringContext = createContext<TailoringContextValue | null>(null);

export const useTailoring = (): TailoringContextValue => {
    const context = useContext(TailoringContext);
    if (!context) throw new Error('useTailoring must be used inside TailoringProvider.');
    return context;
};

const sameFit = (a: FitState, b: FitState) =>
    a.key === b.key && a.fits === b.fits && a.trimmedIds.join(',') === b.trimmedIds.join(',');

export const TailoringProvider: React.FC<TailoringProviderProps> = ({ children }) => {
    const { profile, settings, attempt } = useProfileContext();
    const [draft, setDraft] = useState('');
    const [applied, setApplied] = useState<string | null>(null);
    const [chosenTagIds, setChosenTagIds] = useState<number[]>([]);
    const [result, setResult] = useState<TailorResult | null>(null);
    const [loading, setLoading] = useState(false);
    const [view, setView] = useState<CvView>('all');
    const [fit, setFitState] = useState<FitState>(emptyFit);
    const [pages, setPagesState] = useState(1);
    const request = useRef(0);
    const loadedProfileId = useRef<number | null>(null);

    const contentKey = profile ? JSON.stringify([profile.id, profile.sections, profile.tags]) : '';
    const matchingKey = settings ? JSON.stringify([settings.matchingMode, settings.ollamaUrl, settings.ollamaModel]) : '';
    const tagIds = chosenTagIds.filter(id => profile?.tags.some(tag => tag.id === id && !tag.isAlwaysOn));

    const sourceFor = (ids: number[], description: string | null): TailoringSource | null =>
        ids.length > 0 ? { kind: 'tags', tagIds: ids } : description ? { kind: 'description', text: description } : null;

    const source = sourceFor(tagIds, applied);

    const run = async (next: TailoringSource) => {
        if (!profile) return;
        const id = ++request.current;
        setLoading(true);
        await attempt(async () => {
            const outcome = await api.tailor(profile.id, next.kind === 'tags' ? { tagIds: next.tagIds } : { jobDescription: next.text });
            if (id === request.current) setResult(outcome);
        }, 'Could not tailor the CV.');
        if (id === request.current) setLoading(false);
    };

    const apply = (next: TailoringSource | null) => {
        if (!next) {
            request.current++;
            setResult(null);
            setLoading(false);
            setView('all');
            setFitState(emptyFit);
            return;
        }
        setView('tailored');
        run(next);
    };

    useEffect(() => {
        if (!profile) return;

        if (loadedProfileId.current !== profile.id) {
            loadedProfileId.current = profile.id;
            const savedDescription = profile.jobDescription.trim();
            setDraft(savedDescription);
            setApplied(savedDescription || null);
            setChosenTagIds(profile.tailorTagIds);
            setFitState(emptyFit);
            apply(sourceFor(profile.tailorTagIds, savedDescription || null));
            return;
        }

        if (!source) return;
        const timer = window.setTimeout(() => run(source), retailorDelay);
        return () => window.clearTimeout(timer);
    }, [contentKey, matchingKey]);

    const saveDescription = (description: string) => {
        if (profile) attempt(() => api.saveJobDescription(profile.id, description).then(() => undefined), 'Could not save the job description.');
    };

    const saveTags = (ids: number[]) => {
        if (profile) attempt(() => api.saveTailorTags(profile.id, ids).then(() => undefined), 'Could not save the chosen tags.');
    };

    const setTagIds = (ids: number[]) => {
        setChosenTagIds(ids);
        saveTags(ids);
        apply(sourceFor(ids, applied));
    };

    const tailor = async () => {
        const description = draft.trim();
        if (!description) return;
        setApplied(description);
        saveDescription(description);
        if (tagIds.length > 0) {
            setChosenTagIds([]);
            saveTags([]);
        }
        apply({ kind: 'description', text: description });
    };

    const clear = () => {
        setApplied(null);
        saveDescription('');
        apply(sourceFor(tagIds, null));
    };

    const isTailored = view === 'tailored' && result !== null;

    const value: TailoringContextValue = {
        draft,
        setDraft,
        applied,
        tagIds,
        setTagIds,
        source,
        result,
        loading,
        view,
        setView,
        tailor,
        clear,
        fit,
        setFit: next => setFitState(current => (sameFit(current, next) ? current : next)),
        pages,
        setPages: next => setPagesState(current => (Math.abs(current - next) < 0.01 ? current : next)),
        isTailored,
        lineStatus: line => (isTailored && result ? lineStatusFor(result, fit.trimmedIds, line) : null),
        sectionStatus: section => (isTailored && result ? sectionStatusFor(result, section) : null),
    };

    return <TailoringContext.Provider value={value}>{children}</TailoringContext.Provider>;
};
