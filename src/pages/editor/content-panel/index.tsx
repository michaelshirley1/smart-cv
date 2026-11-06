import React from 'react';
import { ContentPanelProps } from './model';
import { useProfileContext } from '../profile-context';

import './styles.scss';

const ContentPanel: React.FC<ContentPanelProps> = () => {
    const { profile } = useProfileContext();
    const sections = profile?.sections ?? [];

    return (
        <div className="content-panel">
            {sections.length === 0 ? (
                <p className="content-panel-empty">No sections yet.</p>
            ) : (
                <ul className="content-panel-sections">
                    {sections.map(section => (
                        <li key={section.id} className="content-panel-section">{section.title}</li>
                    ))}
                </ul>
            )}
        </div>
    );
};

export default ContentPanel;
