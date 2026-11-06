import React from 'react';
import { EditorPageProps } from './model';
import { ProfileProvider, useProfileContext } from './profile-context';
import TopBar from './top-bar';
import EditorLayout from './editor-layout';
import ErrorBanner from '../../components/error-banner';

import './styles.scss';

const EditorContent: React.FC = () => {
    const { error, clearError } = useProfileContext();

    return (
        <div className="editor-page">
            <TopBar />
            {error && <ErrorBanner message={error} onDismiss={clearError} />}
            <EditorLayout />
        </div>
    );
};

const EditorPage: React.FC<EditorPageProps> = () => (
    <ProfileProvider>
        <EditorContent />
    </ProfileProvider>
);

export default EditorPage;
