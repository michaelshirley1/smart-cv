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
    priority: number;
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
    maxTotal: number;
    isHidden: boolean;
    priority: number;
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
    jobDescription: string;
    createdAt: string;
    updatedAt: string;
}

export interface ProfileDetail extends Profile {
    tags: Tag[];
    sections: Section[];
    tailorTagIds: number[];
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

export interface SectionRequest {
    title: string;
    kind: SectionKind;
    maxLines: number;
    maxTotal: number;
    isHidden: boolean;
    priority: number;
}

export interface EntryRequest {
    heading: string;
    subheading: string;
    location: string;
    dateRange: string;
}

export interface LineRequest {
    text: string;
    override: LineOverride;
    priority: number;
}

export interface CreateSectionRequest extends SectionRequest {
    profileId: number;
}

export interface CreateEntryRequest extends EntryRequest {
    sectionId: number;
}

export interface CreateLineRequest extends LineRequest {
    entryId: number;
    tagIds: number[];
}

export const normalPriority = 3;

export type TagColour = '' | 'blue' | 'green' | 'amber' | 'red' | 'purple' | 'teal' | 'pink';

export interface TagRequest {
    name: string;
    keywords: string[];
    colour: string;
}

export interface CreateTagRequest extends TagRequest {
    profileId: number;
}

export type TemplateValues = Record<string, string>;

export interface TemplateSummary {
    name: string;
    isBuiltIn: boolean;
}

export interface Template extends TemplateSummary {
    values: TemplateValues;
}

export interface TemplateRequest {
    name: string;
    values: TemplateValues;
}

export type SectionDecision = 'untagged' | 'matched' | 'alwaysOn' | 'unmatched' | 'hidden';

export type LineDecision = 'alwaysOn' | 'matched' | 'untagged' | 'fallback' | 'unmatched' | 'overLimit' | 'hidden' | 'sectionExcluded';

export interface TagMatch {
    tagId: number;
    name: string;
    score: number;
    terms: string[];
}

export interface SectionResult {
    sectionId: number;
    included: boolean;
    decision: SectionDecision;
}

export interface LineResult {
    lineId: number;
    entryId: number;
    sectionId: number;
    score: number;
    priority: number;
    included: boolean;
    decision: LineDecision;
}

export type TailorSource = 'description' | 'tags';

export interface TailorRequest {
    jobDescription?: string;
    tagIds?: number[];
}

export interface TailorResult {
    source: TailorSource;
    mode: MatchingMode;
    model: string | null;
    notice: string | null;
    matchedTags: TagMatch[];
    sections: SectionResult[];
    lines: LineResult[];
    trimOrder: number[];
}

export type CvView = 'all' | 'tailored';

export interface OllamaStatus {
    reachable: boolean;
    models: string[];
    error: string | null;
}

export interface ExportRequest {
    profileId: number;
    sectionIds: number[] | null;
    lineIds: number[] | null;
    folder?: string;
    fileName?: string;
    overwrite?: boolean;
}

export interface ExportResult {
    saved: boolean;
    path: string | null;
    exists: boolean;
}

export interface ExportLocation {
    folder: string;
    fileName: string;
    canBrowse: boolean;
}

export interface AppSettings {
    matchingMode: MatchingMode;
    ollamaUrl: string;
    ollamaModel: string;
    lastProfileId: number | null;
}

export interface AppInfo {
    version: string;
    dataFolder: string;
}
