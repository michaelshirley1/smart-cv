import client from '../../api/client';
import { AppSettings, Profile, ProfileDetail, ProfileRequest } from './model';

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
