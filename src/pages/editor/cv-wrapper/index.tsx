import React from 'react';
import { CvWrapperProps } from './model';
import { placeholderSections } from './lorem';
import CvHeader from './cv-header';
import CvSection from './cv-section';

import './styles.scss';

const CvWrapper: React.FC<CvWrapperProps> = ({ profile, variant, showPlaceholders, edits }) => {
    const usePlaceholderSections = profile.sections.length === 0 && showPlaceholders;
    const sections = usePlaceholderSections ? placeholderSections : profile.sections.filter(section => !section.isHidden);

    return (
        <div className={`cv-wrapper cv-wrapper--${variant}`}>
            <article className="cv-page">
                <CvHeader profile={profile} showPlaceholders={showPlaceholders} onSave={edits?.saveProfile} />
                {sections.map(section => (
                    <CvSection
                        key={section.id}
                        section={section}
                        isPlaceholder={usePlaceholderSections}
                        showPlaceholders={showPlaceholders}
                        edits={edits}
                    />
                ))}
            </article>
        </div>
    );
};

export default CvWrapper;
