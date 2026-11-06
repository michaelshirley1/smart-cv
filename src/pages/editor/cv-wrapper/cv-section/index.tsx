import React from 'react';
import { CvSectionProps } from './model';
import { toSectionRequest } from '../../api';
import CvEntry from '../cv-entry';
import EditableText from '../../../../components/editable-text';

import './styles.scss';

const CvSection: React.FC<CvSectionProps> = ({ section, isPlaceholder, showPlaceholders, edits }) => {
    const saveTitle = edits && !isPlaceholder
        ? (title: string) => title && edits.saveSection(section.id, { ...toSectionRequest(section), title })
        : undefined;

    return (
        <section className={`cv-section cv-section--${section.kind}${isPlaceholder ? ' cv-section--placeholder' : ''}`}>
            <h2 className="cv-section-title">
                <EditableText value={section.title} onSave={saveTitle} />
            </h2>
            <div className="cv-section-entries">
                {section.entries.map(entry => (
                    <CvEntry
                        key={entry.id}
                        entry={entry}
                        kind={section.kind}
                        isPlaceholder={isPlaceholder}
                        showPlaceholders={showPlaceholders}
                        edits={edits}
                    />
                ))}
            </div>
        </section>
    );
};

export default CvSection;
