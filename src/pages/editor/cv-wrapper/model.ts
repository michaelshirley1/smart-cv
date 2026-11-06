import { EntryRequest, LineRequest, ProfileDetail, ProfileRequest, SectionKind, SectionRequest } from '../model';

export type CvVariant = 'editor' | 'print';

export type CvLineVariant = 'bullet' | 'paragraph' | 'plain';

export interface CvEdits {
    saveProfile: (request: ProfileRequest) => void;
    saveSection: (id: number, request: SectionRequest) => void;
    saveEntry: (id: number, request: EntryRequest) => void;
    saveLine: (id: number, request: LineRequest) => void;
}

export interface CvWrapperProps {
    profile: ProfileDetail;
    variant: CvVariant;
    showPlaceholders: boolean;
    edits?: CvEdits;
}

export const lineVariantFor = (kind: SectionKind): CvLineVariant => {
    if (kind === 'summary') return 'paragraph';
    if (kind === 'skills' || kind === 'custom') return 'plain';
    return 'bullet';
};

export const hasEntryHeader = (kind: SectionKind) =>
    kind === 'experience' || kind === 'education' || kind === 'projects';
