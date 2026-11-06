import React from 'react';
import { StylePanelProps } from './model';
import { useProfileContext } from '../profile-context';

import './styles.scss';

const StylePanel: React.FC<StylePanelProps> = () => {
    const { profile } = useProfileContext();

    return (
        <div className="style-panel">
            <dl className="style-panel-summary">
                <dt className="style-panel-summary-label">Template</dt>
                <dd className="style-panel-summary-value">{profile?.templateName ?? '—'}</dd>
                <dt className="style-panel-summary-label">Target length</dt>
                <dd className="style-panel-summary-value">{profile ? `${profile.targetPages} page${profile.targetPages === 1 ? '' : 's'}` : '—'}</dd>
            </dl>
        </div>
    );
};

export default StylePanel;
