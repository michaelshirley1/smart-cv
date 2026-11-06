import React from 'react';
import { CvEntryProps } from './model';
import { hasEntryHeader, lineVariantFor } from '../model';
import { EntryRequest } from '../../model';
import { placeholderEntry, placeholderLines } from '../lorem';
import { toEntryRequest } from '../../api';
import CvLine from '../cv-line';
import EditableText from '../../../../components/editable-text';

import './styles.scss';

type EntryField = keyof EntryRequest;

const CvEntry: React.FC<CvEntryProps> = ({ entry, kind, isPlaceholder, showPlaceholders, selection, edits }) => {
    const variant = lineVariantFor(kind);
    const visibleLines = entry.lines.filter(line => line.override !== 'hidden' && (!selection || selection.lineIds.has(line.id)));
    const usePlaceholderLines = visibleLines.length === 0 && showPlaceholders && !isPlaceholder;
    const lines = usePlaceholderLines ? placeholderLines(entry.id, variant) : visibleLines;
    const editable = edits && !isPlaceholder;

    const saveField = (field: EntryField) => editable
        ? (value: string) => edits.saveEntry(entry.id, { ...toEntryRequest(entry), [field]: value })
        : undefined;

    const field = (name: EntryField, className: string, placeholder = '') =>
        entry[name] || (showPlaceholders && placeholder)
            ? <EditableText className={className} value={entry[name]} placeholder={placeholder} onSave={saveField(name)} />
            : null;

    const LinesElement = variant === 'bullet' ? 'ul' : 'div';

    return (
        <div className={`cv-entry${isPlaceholder ? ' cv-entry--placeholder' : ''}`}>
            {hasEntryHeader(kind) && (
                <div className="cv-entry-header">
                    <div className="cv-entry-row">
                        {field('heading', 'cv-entry-heading', placeholderEntry.heading)}
                        {field('dateRange', 'cv-entry-date', placeholderEntry.dateRange)}
                    </div>
                    <div className="cv-entry-row">
                        {field('subheading', 'cv-entry-subheading', placeholderEntry.subheading)}
                        {field('location', 'cv-entry-location')}
                    </div>
                </div>
            )}
            {lines.length > 0 && (
                <LinesElement className={`cv-entry-lines cv-entry-lines--${variant}`}>
                    {lines.map(line => (
                        <CvLine
                            key={line.id}
                            line={line}
                            variant={variant}
                            isPlaceholder={isPlaceholder || usePlaceholderLines}
                            onSave={edits?.saveLine}
                        />
                    ))}
                </LinesElement>
            )}
        </div>
    );
};

export default CvEntry;
