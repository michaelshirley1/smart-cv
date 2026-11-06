import React, { useEffect, useState } from 'react';
import { lineCountLabel, SectionCardProps } from './model';
import { sectionKindOptions, withAlwaysOn } from '../model';
import { hasEntryHeader } from '../../cv-wrapper/model';
import { useContentActions } from '../actions';
import { useReorder } from '../../../../hooks/useReorder';
import EntryCard from '../entry-card';
import DragHandle from '../../../../components/drag-handle';
import EditableText from '../../../../components/editable-text';
import Icon from '../../../../components/icon';
import IconButton from '../../../../components/icon-button';
import Select from '../../../../components/select';
import TextField from '../../../../components/text-field';
import TagInput from '../../../../components/tag-input';
import Button from '../../../../components/button';
import ConfirmDialog from '../../../../components/confirm-dialog';

import './styles.scss';

const SectionCard: React.FC<SectionCardProps> = ({ section, tagOptions, alwaysOn, expanded, onToggle, handleProps }) => {
    const actions = useContentActions();
    const [confirmDelete, setConfirmDelete] = useState(false);
    const [maxLines, setMaxLines] = useState(String(section.maxLines));
    const entryIds = section.entries.map(entry => entry.id);
    const reorder = useReorder(`entry-${section.id}`, entryIds, ids => actions.reorderEntries(section.id, ids));
    const showEntryHeaders = hasEntryHeader(section.kind);
    const isAlwaysOn = alwaysOn !== undefined && section.tagIds.includes(alwaysOn.id);
    const bodyId = `section-${section.id}-body`;

    useEffect(() => setMaxLines(String(section.maxLines)), [section.maxLines]);

    const saveMaxLines = () => {
        const value = Number.parseInt(maxLines, 10);
        if (value >= 1 && value <= 50 && value !== section.maxLines) actions.saveSection(section, { maxLines: value });
        else setMaxLines(String(section.maxLines));
    };

    return (
        <div className={`section-card${section.isHidden ? ' section-card--hidden' : ''}`}>
            <div className="section-card-header">
                <DragHandle label={`Section ${section.title}`} {...handleProps} />
                <button
                    type="button"
                    className={`section-card-toggle${expanded ? ' section-card-toggle--expanded' : ''}`}
                    aria-expanded={expanded}
                    aria-controls={bodyId}
                    aria-label={expanded ? `Collapse ${section.title}` : `Expand ${section.title}`}
                    onClick={onToggle}
                >
                    <Icon name="chevron-right" />
                </button>
                <EditableText
                    className="section-card-title"
                    value={section.title}
                    onSave={title => title && actions.saveSection(section, { title })}
                />
                <span className="section-card-count">{lineCountLabel(section)}</span>
                <div className="section-card-actions">
                    <IconButton
                        icon="pin"
                        className="icon-button--small"
                        active={isAlwaysOn}
                        label={isAlwaysOn ? 'Always on. Click to let tailoring decide.' : 'Always show this section'}
                        onClick={() => actions.setSectionTags(section.id, withAlwaysOn(section.tagIds, alwaysOn?.id, !isAlwaysOn))}
                    />
                    <IconButton
                        icon={section.isHidden ? 'eye-off' : 'eye'}
                        className="icon-button--small"
                        active={section.isHidden}
                        label={section.isHidden ? 'Hidden from the CV. Click to show.' : 'Hide from the CV'}
                        onClick={() => actions.saveSection(section, { isHidden: !section.isHidden })}
                    />
                    <IconButton icon="trash" className="icon-button--small" label={`Delete ${section.title}`} onClick={() => setConfirmDelete(true)} />
                </div>
            </div>

            {expanded && (
                <div className="section-card-body" id={bodyId}>
                    <div className="section-card-settings">
                        <Select
                            className="section-card-kind"
                            label="Type"
                            value={section.kind}
                            options={sectionKindOptions}
                            onChange={kind => actions.saveSection(section, { kind })}
                        />
                        <TextField
                            className="section-card-max-lines"
                            label="Max lines"
                            type="number"
                            min={1}
                            max={50}
                            value={maxLines}
                            onChange={setMaxLines}
                            onBlur={saveMaxLines}
                        />
                    </div>
                    <div className="section-card-tags">
                        <span className="section-card-tags-label">Tags</span>
                        <TagInput
                            tags={tagOptions}
                            selectedIds={section.tagIds.filter(id => id !== alwaysOn?.id)}
                            onChange={ids => actions.setSectionTags(section.id, withAlwaysOn(ids, alwaysOn?.id, isAlwaysOn))}
                            onCreate={actions.createTag}
                        />
                    </div>
                    <ul className="section-card-entries">
                        {section.entries.map(entry => {
                            const drop = reorder.dropPosition(entry.id);
                            return (
                                <li
                                    key={entry.id}
                                    className={`section-card-entry${drop ? ` section-card-entry--drop-${drop}` : ''}`}
                                    {...reorder.itemProps(entry.id)}
                                >
                                    <EntryCard
                                        entry={entry}
                                        showHeader={showEntryHeaders}
                                        tagOptions={tagOptions}
                                        alwaysOn={alwaysOn}
                                        handleProps={showEntryHeaders && section.entries.length > 1 ? reorder.handleProps(entry.id) : undefined}
                                    />
                                </li>
                            );
                        })}
                    </ul>
                    {showEntryHeaders && (
                        <Button variant="ghost" className="section-card-add-entry" onClick={() => actions.createEntry(section.id)}>
                            Add entry
                        </Button>
                    )}
                </div>
            )}

            {confirmDelete && (
                <ConfirmDialog
                    title="Delete section"
                    message={<>Delete <strong>{section.title}</strong> and its {lineCountLabel(section)}? This can't be undone.</>}
                    confirmLabel="Delete"
                    onConfirm={() => actions.deleteSection(section.id)}
                    onClose={() => setConfirmDelete(false)}
                />
            )}
        </div>
    );
};

export default SectionCard;
