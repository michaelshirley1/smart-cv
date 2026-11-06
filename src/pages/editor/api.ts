import client from '../../api/client';
import { AppSettings, Entry, EntryRequest, Line, LineRequest, Profile, ProfileDetail, ProfileRequest, Section, SectionRequest } from './model';

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

export const deleteProfile = (id: number) =>
    client.delete(`/profile/${id}`);

export const updateSection = (id: number, request: SectionRequest) =>
    client.put<ProfileDetail>(`/section/${id}`, request).then(r => r.data);

export const updateEntry = (id: number, request: EntryRequest) =>
    client.put<ProfileDetail>(`/entry/${id}`, request).then(r => r.data);

export const updateLine = (id: number, request: LineRequest) =>
    client.put<ProfileDetail>(`/line/${id}`, request).then(r => r.data);

export const getSettings = () =>
    client.get<AppSettings>('/settings').then(r => r.data);

export const updateSettings = (settings: AppSettings) =>
    client.put<AppSettings>('/settings', settings).then(r => r.data);

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
