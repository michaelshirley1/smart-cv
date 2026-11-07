import React from 'react';
import { AppSettings, Profile, ProfileDetail } from '../model';

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
    importProfile: (file: File) => Promise<void>;
    mutate: (action: () => Promise<ProfileDetail>, fallback: string) => Promise<ProfileDetail | null>;
    attempt: (action: () => Promise<void>, fallback: string) => Promise<void>;
    reloadProfile: () => Promise<void>;
    settings: AppSettings | null;
    saveSettings: (settings: AppSettings) => Promise<void>;
    canUndo: boolean;
    canRedo: boolean;
    undo: () => Promise<void>;
    redo: () => Promise<void>;
    templateRevision: number;
    refreshTemplate: () => void;
}
