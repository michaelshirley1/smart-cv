import React, { useEffect, useState } from 'react';
import { limitText, lineCountLabel, parseLimit, SectionCardProps } from './model';
import { priorityOptions, sectionKindOptions, withAlwaysOn } from '../model';
import { hasEntryHeader } from '../../cv-wrapper/model';
import { useContentActions } from '../actions';
import { useTailoring } from '../../tailoring-context';
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
    const status = useTailoring().sectionStatus(section);
    const [confirmDelete, setConfirmDelete] = useState(false);
    const [maxLines, setMaxLines] = useState(limitText(section.maxLines));
    const [maxTotal, setMaxTotal] = useState(limitText(section.maxTotal));
    const entryIds = section.entries.map(entry => entry.id);
    const reorder = useReorder(`entry-${section.id}`, entryIds, ids => actions.reorderEntries(section.id, ids));
    const showEntryHeaders = hasEntryHeader(section.kind);
    const isAlwaysOn = alwaysOn !== undefined && section.tagIds.includes(alwaysOn.id);
    const bodyId = `section-${section.id}-body`;

    useEffect(() => setMaxLines(limitText(section.maxLines)), [section.maxLines]);
    useEffect(() => setMaxTotal(limitText(section.maxTotal)), [section.maxTotal]);

    const saveLimits = () => {
        const perEntry = parseLimit(maxLines, 50) ?? section.maxLines;
        const inSection = parseLimit(maxTotal, 200) ?? section.maxTotal;
        setMaxLines(limitText(perEntry));
        setMaxTotal(limitText(inSection));
        if (perEntry !== section.maxLines || inSection !== section.maxTotal) {
            actions.saveSection(section, { maxLines: perEntry, maxTotal: inSection });
        }
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
                {status && (
                    <span className={`section-card-status section-card-status--${status.included ? 'in' : 'out'}`}>{status.label}</span>
                )}
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
                            className="section-card-limit"
                            label="Max per entry"
                            type="number"
                            min={1}
                            max={50}
                            placeholder="No limit"
                            value={maxLines}
                            onChange={setMaxLines}
                            onBlur={saveLimits}
                        />
                        <TextField
                            className="section-card-limit"
                            label="Max in section"
                            type="number"
                            min={1}
                            max={200}
                            placeholder="No limit"
                            value={maxTotal}
                            onChange={setMaxTotal}
                            onBlur={saveLimits}
                        />
                        <Select
                            className="section-card-priority"
                            label="Priority"
                            value={String(section.priority)}
                            options={priorityOptions}
                            onChange={value => actions.saveSection(section, { priority: Number(value) })}
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
