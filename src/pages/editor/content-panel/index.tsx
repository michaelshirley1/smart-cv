import React from 'react';
import { ContentPanelProps } from './model';
import { useProfileContext } from '../profile-context';
import JobDescription from './job-description';
import ProfileDetails from './profile-details';
import SectionList from './section-list';
import TagManager from './tag-manager';

import './styles.scss';

const ContentPanel: React.FC<ContentPanelProps> = () => {
    const { profile } = useProfileContext();
    if (!profile) return null;

    return (
        <div className="content-panel">
            <JobDescription tags={profile.tags} />
            <ProfileDetails profile={profile} />
            <SectionList profile={profile} />
            <TagManager profile={profile} />
        </div>
    );
};

export default ContentPanel;
