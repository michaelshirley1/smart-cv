import React, { createContext, useContext, useEffect, useRef, useState } from 'react';
import { emptyFit, FitState, TailoringContextValue, TailoringProviderProps } from './model';
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
    const { profile, attempt } = useProfileContext();
    const [draft, setDraft] = useState('');
    const [applied, setApplied] = useState<string | null>(null);
    const [result, setResult] = useState<TailorResult | null>(null);
    const [loading, setLoading] = useState(false);
    const [view, setView] = useState<CvView>('all');
    const [fit, setFitState] = useState<FitState>(emptyFit);
    const [pages, setPagesState] = useState(1);
    const request = useRef(0);

    const contentKey = profile ? JSON.stringify([profile.id, profile.sections, profile.tags]) : '';

    const run = async (description: string) => {
        if (!profile) return;
        const id = ++request.current;
        setLoading(true);
        await attempt(async () => {
            const next = await api.tailor(profile.id, description);
            if (id === request.current) setResult(next);
        }, 'Could not tailor the CV.');
        if (id === request.current) setLoading(false);
    };

    useEffect(() => {
        if (applied === null) return;
        const timer = window.setTimeout(() => run(applied), retailorDelay);
        return () => window.clearTimeout(timer);
    }, [contentKey]);

    const tailor = async () => {
        const description = draft.trim();
        if (!description) return;
        setApplied(description);
        await run(description);
        setView('tailored');
    };

    const clear = () => {
        request.current++;
        setApplied(null);
        setResult(null);
        setLoading(false);
        setView('all');
        setFitState(emptyFit);
    };

    const isTailored = view === 'tailored' && result !== null;

    const value: TailoringContextValue = {
        draft,
        setDraft,
        applied,
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
