import client from '../../api/client';
import {
    AppSettings,
    CreateEntryRequest,
    CreateLineRequest,
    CreateSectionRequest,
    CreateTagRequest,
    Entry,
    EntryRequest,
    ExportRequest,
    ExportResult,
    Line,
    LineRequest,
    OllamaStatus,
    Profile,
    ProfileDetail,
    ProfileRequest,
    Section,
    SectionRequest,
    Tag,
    TagRequest,
    TailorResult,
    Template,
    TemplateRequest,
    TemplateSummary,
} from './model';

export const getProfiles = () =>
    client.get<Profile[]>('/profile').then(r => r.data);

export const getProfile = (id: number) =>
    client.get<ProfileDetail>(`/profile/${id}`).then(r => r.data);

export const createProfile = (request: ProfileRequest) =>
    client.post<ProfileDetail>('/profile', request).then(r => r.data);

export const updateProfile = (id: number, request: ProfileRequest) =>
    client.put<ProfileDetail>(`/profile/${id}`, request).then(r => r.data);

export const duplicateProfile = (id: number) =>
    client.post<ProfileDetail>(`/profile/${id}/duplicate`).then(r => r.data);

export const restoreProfile = (id: number, snapshot: ProfileDetail) =>
    client.post<ProfileDetail>(`/profile/${id}/restore`, snapshot).then(r => r.data);

export const saveJobDescription = (id: number, jobDescription: string) =>
    client.put(`/profile/${id}/job-description`, { jobDescription });

export const deleteProfile = (id: number) =>
    client.delete(`/profile/${id}`);

export const createSection = (request: CreateSectionRequest) =>
    client.post<ProfileDetail>('/section', request).then(r => r.data);

export const updateSection = (id: number, request: SectionRequest) =>
    client.put<ProfileDetail>(`/section/${id}`, request).then(r => r.data);

export const deleteSection = (id: number) =>
    client.delete<ProfileDetail>(`/section/${id}`).then(r => r.data);

export const reorderSections = (profileId: number, ids: number[]) =>
    client.put<ProfileDetail>('/section/reorder', { parentId: profileId, ids }).then(r => r.data);

export const setSectionTags = (id: number, tagIds: number[]) =>
    client.put<ProfileDetail>(`/section/${id}/tags`, { tagIds }).then(r => r.data);

export const createEntry = (request: CreateEntryRequest) =>
    client.post<ProfileDetail>('/entry', request).then(r => r.data);

export const updateEntry = (id: number, request: EntryRequest) =>
    client.put<ProfileDetail>(`/entry/${id}`, request).then(r => r.data);

export const deleteEntry = (id: number) =>
    client.delete<ProfileDetail>(`/entry/${id}`).then(r => r.data);

export const reorderEntries = (sectionId: number, ids: number[]) =>
    client.put<ProfileDetail>('/entry/reorder', { parentId: sectionId, ids }).then(r => r.data);

export const createLine = (request: CreateLineRequest) =>
    client.post<ProfileDetail>('/line', request).then(r => r.data);

export const updateLine = (id: number, request: LineRequest) =>
    client.put<ProfileDetail>(`/line/${id}`, request).then(r => r.data);

export const deleteLine = (id: number) =>
    client.delete<ProfileDetail>(`/line/${id}`).then(r => r.data);

export const reorderLines = (entryId: number, ids: number[]) =>
    client.put<ProfileDetail>('/line/reorder', { parentId: entryId, ids }).then(r => r.data);

export const setLineTags = (id: number, tagIds: number[]) =>
    client.put<ProfileDetail>(`/line/${id}/tags`, { tagIds }).then(r => r.data);

export const createTag = (request: CreateTagRequest) =>
    client.post<ProfileDetail>('/tag', request).then(r => r.data);

export const updateTag = (id: number, request: TagRequest) =>
    client.put<ProfileDetail>(`/tag/${id}`, request).then(r => r.data);

export const deleteTag = (id: number) =>
    client.delete<ProfileDetail>(`/tag/${id}`).then(r => r.data);

export const getTemplates = () =>
    client.get<TemplateSummary[]>('/template').then(r => r.data);

export const getTemplate = (name: string) =>
    client.get<Template>(`/template/${encodeURIComponent(name)}`).then(r => r.data);

export const createTemplate = (request: TemplateRequest) =>
    client.post<Template>('/template', request).then(r => r.data);

export const updateTemplate = (name: string, request: TemplateRequest) =>
    client.put<Template>(`/template/${encodeURIComponent(name)}`, request).then(r => r.data);

export const deleteTemplate = (name: string) =>
    client.delete(`/template/${encodeURIComponent(name)}`);

export const getFonts = () =>
    client.get<string[]>('/font').then(r => r.data);

export const tailor = (profileId: number, jobDescription: string) =>
    client.post<TailorResult>('/tailor', { profileId, jobDescription }).then(r => r.data);

export const exportPdf = (request: ExportRequest) =>
    client.post<ExportResult>('/export/pdf', request).then(r => r.data);

export const getSettings = () =>
    client.get<AppSettings>('/settings').then(r => r.data);

export const updateSettings = (settings: AppSettings) =>
    client.put<AppSettings>('/settings', settings).then(r => r.data);

export const getOllamaStatus = (url?: string) =>
    client.get<OllamaStatus>('/settings/ollama', { params: url ? { url } : undefined }).then(r => r.data);

export const toProfileRequest = (profile: Profile): ProfileRequest => ({
    name: profile.name,
    templateName: profile.templateName,
    targetPages: profile.targetPages,
    fullName: profile.fullName,
    headline: profile.headline,
    email: profile.email,
    phone: profile.phone,
    location: profile.location,
    links: profile.links,
});

export const toSectionRequest = (section: Section): SectionRequest => ({
    title: section.title,
    kind: section.kind,
    maxLines: section.maxLines,
    isHidden: section.isHidden,
});

export const toEntryRequest = (entry: Entry): EntryRequest => ({
    heading: entry.heading,
    subheading: entry.subheading,
    location: entry.location,
    dateRange: entry.dateRange,
});

export const toLineRequest = (line: Line): LineRequest => ({
    text: line.text,
    override: line.override,
});

export const toTagRequest = (tag: Tag): TagRequest => ({
    name: tag.name,
    keywords: tag.keywords,
    colour: tag.colour,
});

export const newProfileRequest = (name: string): ProfileRequest => ({
    name,
    templateName: '',
    targetPages: 1,
    fullName: '',
    headline: '',
    email: '',
    phone: '',
    location: '',
    links: [],
});
