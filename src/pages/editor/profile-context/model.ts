import React from 'react';
import { Profile, ProfileDetail } from '../model';

export interface ProfileProviderProps {
    children: React.ReactNode;
}

export interface ProfileContextValue {
    profiles: Profile[];
    profile: ProfileDetail | null;
    loading: boolean;
    error: string | null;
    clearError: () => void;
    selectProfile: (id: number) => Promise<void>;
    createProfile: (name: string) => Promise<void>;
    renameProfile: (name: string) => Promise<void>;
    duplicateProfile: () => Promise<void>;
    deleteProfile: () => Promise<void>;
    applyProfile: (profile: ProfileDetail) => void;
}
