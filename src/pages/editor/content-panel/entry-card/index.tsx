import React, { useState } from 'react';
import { entryFields, EntryCardProps } from './model';
import { useContentActions } from '../actions';
import { useReorder } from '../../../../hooks/useReorder';
import { useActiveLine } from '../active-line-context';
import LineRow from '../line-row';
import DragHandle from '../../../../components/drag-handle';
import EditableText from '../../../../components/editable-text';
import IconButton from '../../../../components/icon-button';
import ConfirmDialog from '../../../../components/confirm-dialog';

import './styles.scss';

const EntryCard: React.FC<EntryCardProps> = ({ entry, showHeader, tagOptions, alwaysOn, handleProps }) => {
    const actions = useContentActions();
    const { setActiveLineId } = useActiveLine();
    const [confirmDelete, setConfirmDelete] = useState(false);
    const lineIds = entry.lines.map(line => line.id);
    const reorder = useReorder(`line-${entry.id}`, lineIds, ids => actions.reorderLines(entry.id, ids));
    const name = entry.heading || 'this entry';

    const addLine = async (text: string) => {
        if (!text) return;
        const updated = await actions.createLine(entry.id, text);
        const lines = updated?.sections.flatMap(section => section.entries).find(candidate => candidate.id === entry.id)?.lines ?? [];
        const created = lines.find(line => !lineIds.includes(line.id));
        if (created) setActiveLineId(created.id);
    };

    const remove = () => (entry.lines.length > 0 ? setConfirmDelete(true) : actions.deleteEntry(entry.id));

    return (
        <div className={`entry-card${showHeader ? ' entry-card--with-header' : ''}`}>
            {showHeader && (
                <div className="entry-card-header">
                    {handleProps && <DragHandle label={`Entry ${name}`} {...handleProps} />}
                    <div className="entry-card-fields">
                        {entryFields.map(({ field, placeholder }) => (
                            <EditableText
                                key={field}
                                className={`entry-card-field entry-card-field--${field}`}
                                value={entry[field]}
                                placeholder={placeholder}
                                onSave={value => actions.saveEntry(entry, { [field]: value })}
                            />
                        ))}
                    </div>
                    <IconButton icon="trash" className="icon-button--small" label={`Delete ${name}`} onClick={remove} />
                </div>
            )}

            <ul className="entry-card-lines">
                {entry.lines.map(line => {
                    const drop = reorder.dropPosition(line.id);
                    return (
                        <li
                            key={line.id}
                            className={`entry-card-line${drop ? ` entry-card-line--drop-${drop}` : ''}`}
                            {...reorder.itemProps(line.id)}
                        >
                            <LineRow line={line} tagOptions={tagOptions} alwaysOn={alwaysOn} handleProps={reorder.handleProps(line.id)} />
                        </li>
                    );
                })}
            </ul>

            <EditableText
                className="entry-card-add-line"
                value=""
                placeholder="+ Add line"
                multiline
                onSave={addLine}
            />

            {confirmDelete && (
                <ConfirmDialog
                    title="Delete entry"
                    message={<>Delete <strong>{name}</strong> and its {entry.lines.length} line{entry.lines.length === 1 ? '' : 's'}? This can't be undone.</>}
                    confirmLabel="Delete"
                    onConfirm={() => actions.deleteEntry(entry.id)}
                    onClose={() => setConfirmDelete(false)}
                />
            )}
        </div>
    );
};

export default EntryCard;
