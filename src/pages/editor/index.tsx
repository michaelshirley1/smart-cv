import React from 'react';
import { AppSettings, EditorPageProps } from './model';
import { getSettings } from './api';
import { useFetch } from '../../hooks/useFetch';

import './styles.scss';

const EditorPage: React.FC<EditorPageProps> = () => {
    const { data: settings, loading, error } = useFetch<AppSettings | null>(getSettings, null, 'Could not reach the Smart CV service.');

    return (
        <div className="editor-page">
            <header className="editor-top-bar">
                <span className="editor-top-bar-title">Smart CV</span>
                <span className="editor-top-bar-status">
                    {loading && 'Connecting'}
                    {error}
                    {settings && `Matching: ${settings.matchingMode}`}
                </span>
            </header>
            <main className="editor-body" />
        </div>
    );
};

export default EditorPage;
