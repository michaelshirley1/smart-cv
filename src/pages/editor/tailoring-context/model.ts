import React from 'react';
import { CvView, Line, Section, TailorResult } from '../model';

export interface TailoringProviderProps {
    children: React.ReactNode;
}

export interface FitState {
    key: string;
    trimmedIds: number[];
    fits: boolean;
}

export interface LineStatus {
    label: string;
    included: boolean;
}

export type TailoringSource = { kind: 'tags'; tagIds: number[] } | { kind: 'description'; text: string };

export interface TailoringContextValue {
    draft: string;
    setDraft: (value: string) => void;
    applied: string | null;
    tagIds: number[];
    setTagIds: (tagIds: number[]) => void;
    source: TailoringSource | null;
    result: TailorResult | null;
    loading: boolean;
    view: CvView;
    setView: (view: CvView) => void;
    tailor: () => Promise<void>;
    clear: () => void;
    fit: FitState;
    setFit: (fit: FitState) => void;
    pages: number;
    setPages: (pages: number) => void;
    isTailored: boolean;
    lineStatus: (line: Line) => LineStatus | null;
    sectionStatus: (section: Section) => LineStatus | null;
}

export const emptyFit: FitState = { key: '', trimmedIds: [], fits: true };
