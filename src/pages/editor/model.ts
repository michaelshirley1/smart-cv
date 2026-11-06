export interface EditorPageProps {}

export type PanelMode = 'rest' | 'left-full' | 'right-full';

export type SectionKind = 'summary' | 'experience' | 'education' | 'projects' | 'skills' | 'custom';

export type LineOverride = 'none' | 'pinned' | 'hidden';

export type MatchingMode = 'keyword' | 'ollama';

export interface Line {
    id: number;
    entryId: number;
    text: string;
    sortOrder: number;
    override: LineOverride;
    tagIds: number[];
}

export interface Entry {
    id: number;
    sectionId: number;
    heading: string;
    subheading: string;
    location: string;
    dateRange: string;
    sortOrder: number;
    lines: Line[];
}

export interface Section {
    id: number;
    profileId: number;
    title: string;
    kind: SectionKind;
    sortOrder: number;
    maxLines: number;
    isHidden: boolean;
    tagIds: number[];
    entries: Entry[];
}

export interface Tag {
    id: number;
    profileId: number;
    name: string;
    keywords: string[];
    colour: string;
    isAlwaysOn: boolean;
    usageCount: number;
}

export interface Profile {
    id: number;
    name: string;
    templateName: string;
    targetPages: number;
    fullName: string;
    headline: string;
    email: string;
    phone: string;
    location: string;
    links: string[];
    createdAt: string;
    updatedAt: string;
}

export interface ProfileDetail extends Profile {
    tags: Tag[];
    sections: Section[];
}

export interface ProfileRequest {
    name: string;
    templateName: string;
    targetPages: number;
    fullName: string;
    headline: string;
    email: string;
    phone: string;
    location: string;
    links: string[];
}

export interface AppSettings {
    matchingMode: MatchingMode;
    ollamaUrl: string;
    ollamaModel: string;
    lastProfileId: number | null;
}
