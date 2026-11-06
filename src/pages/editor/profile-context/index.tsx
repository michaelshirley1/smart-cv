import React, { createContext, useContext, useEffect, useRef, useState } from 'react';
import { ProfileContextValue, ProfileProviderProps } from './model';
import { AppSettings, Profile, ProfileDetail } from '../model';
import * as api from '../api';
import { getErrorMessage } from '../../../api/errors';

const defaultProfileName = 'My CV';

const ProfileContext = createContext<ProfileContextValue | null>(null);

export const useProfileContext = (): ProfileContextValue => {
    const context = useContext(ProfileContext);
    if (!context) throw new Error('useProfileContext must be used inside ProfileProvider.');
    return context;
};

export const ProfileProvider: React.FC<ProfileProviderProps> = ({ children }) => {
    const [profiles, setProfiles] = useState<Profile[]>([]);
    const [profile, setProfile] = useState<ProfileDetail | null>(null);
    const [settings, setSettings] = useState<AppSettings | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const started = useRef(false);

    const run = async (action: () => Promise<void>, fallback: string) => {
        setError(null);
        try {
            await action();
        } catch (e) {
            setError(getErrorMessage(e, fallback));
        }
    };

    const remember = async (profileId: number, current: AppSettings | null = settings) => {
        if (!current || current.lastProfileId === profileId) return;
        setSettings(await api.updateSettings({ ...current, lastProfileId: profileId }));
    };

    const open = async (detail: ProfileDetail, current: AppSettings | null = settings) => {
        setProfile(detail);
        setProfiles(await api.getProfiles());
        await remember(detail.id, current);
    };

    const createDefault = () => api.createProfile(api.newProfileRequest(defaultProfileName));

    useEffect(() => {
        if (started.current) return;
        started.current = true;

        run(async () => {
            const [existing, saved] = await Promise.all([api.getProfiles(), api.getSettings()]);
            setSettings(saved);

            if (existing.length === 0) {
                await open(await createDefault(), saved);
                return;
            }

            const id = existing.some(p => p.id === saved.lastProfileId) ? saved.lastProfileId! : existing[0].id;
            await open(await api.getProfile(id), saved);
        }, 'Could not load your profiles.').finally(() => setLoading(false));
    }, []);

    const selectProfile = (id: number) =>
        run(async () => open(await api.getProfile(id)), 'Could not open that profile.');

    const createProfile = (name: string) =>
        run(async () => open(await api.createProfile(api.newProfileRequest(name))), 'Could not create the profile.');

    const renameProfile = (name: string) =>
        run(async () => {
            if (!profile) return;
            await open(await api.updateProfile(profile.id, { ...api.toProfileRequest(profile), name }));
        }, 'Could not rename the profile.');

    const duplicateProfile = () =>
        run(async () => {
            if (!profile) return;
            await open(await api.duplicateProfile(profile.id));
        }, 'Could not duplicate the profile.');

    const deleteProfile = () =>
        run(async () => {
            if (!profile) return;
            await api.deleteProfile(profile.id);
            const remaining = await api.getProfiles();
            await open(remaining.length > 0 ? await api.getProfile(remaining[0].id) : await createDefault());
        }, 'Could not delete the profile.');

    const value: ProfileContextValue = {
        profiles,
        profile,
        loading,
        error,
        clearError: () => setError(null),
        selectProfile,
        createProfile,
        renameProfile,
        duplicateProfile,
        deleteProfile,
        mutate: (action, fallback) => run(async () => setProfile(await action()), fallback),
    };

    return <ProfileContext.Provider value={value}>{children}</ProfileContext.Provider>;
};
