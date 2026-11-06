import React from 'react';
import { CvSectionProps } from './model';
import { toSectionRequest } from '../../api';
import CvEntry from '../cv-entry';
import EditableText from '../../../../components/editable-text';

import './styles.scss';

const CvSection: React.FC<CvSectionProps> = ({ section, isPlaceholder, showPlaceholders, selection, edits }) => {
    const saveTitle = edits && !isPlaceholder
        ? (title: string) => title && edits.saveSection(section.id, { ...toSectionRequest(section), title })
        : undefined;

    const entries = selection
        ? section.entries.filter(entry => entry.lines.length === 0 || entry.lines.some(line => selection.lineIds.has(line.id)))
        : section.entries;

    return (
        <section className={`cv-section cv-section--${section.kind}${isPlaceholder ? ' cv-section--placeholder' : ''}`}>
            <h2 className="cv-section-title">
                <EditableText value={section.title} onSave={saveTitle} />
            </h2>
            <div className="cv-section-entries">
                {entries.map(entry => (
                    <CvEntry
                        key={entry.id}
                        entry={entry}
                        kind={section.kind}
                        isPlaceholder={isPlaceholder}
                        showPlaceholders={showPlaceholders}
                        selection={selection}
                        edits={edits}
                    />
                ))}
            </div>
        </section>
    );
};

export default CvSection;
