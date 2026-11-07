import React from 'react';
import { lineSummary, LineRowProps } from './model';
import { priorityOptions, withAlwaysOn } from '../model';
import { formatInline } from '../../cv-wrapper/format';
import { useContentActions } from '../actions';
import { useTailoring } from '../../tailoring-context';
import { useActiveLine } from '../active-line-context';
import DragHandle from '../../../../components/drag-handle';
import EditableText from '../../../../components/editable-text';
import IconButton from '../../../../components/icon-button';
import TagInput from '../../../../components/tag-input';
import Select from '../../../../components/select';

import './styles.scss';

const LineRow: React.FC<LineRowProps> = ({ line, tagOptions, alwaysOn, handleProps }) => {
    const actions = useContentActions();
    const status = useTailoring().lineStatus(line);
    const { activeLineId, setActiveLineId } = useActiveLine();
    const isActive = activeLineId === line.id;
    const isAlwaysOn = alwaysOn !== undefined && line.tagIds.includes(alwaysOn.id);
    const isHidden = line.override === 'hidden';
    const tagIds = line.tagIds.filter(id => id !== alwaysOn?.id);

    const classes = [
        'line-row',
        isActive ? 'line-row--active' : '',
        isHidden ? 'line-row--hidden' : '',
        isAlwaysOn ? 'line-row--always-on' : '',
        status ? `line-row--${status.included ? 'in' : 'out'}` : '',
    ].filter(Boolean).join(' ');

    return (
        <div className={classes} onFocusCapture={() => setActiveLineId(line.id)} onMouseDown={() => setActiveLineId(line.id)}>
            <DragHandle label="Line" {...handleProps} />
            <div className="line-row-main">
                <EditableText
                    className="line-row-text"
                    value={line.text}
                    multiline
                    format={formatInline}
                    onSave={text => (text ? actions.saveLine(line, { text }) : actions.deleteLine(line.id))}
                />
                {status && <span className="line-row-status">{status.label}</span>}
                {isActive ? (
                    <div className="line-row-details">
                        <TagInput
                            tags={tagOptions}
                            selectedIds={tagIds}
                            onChange={ids => actions.setLineTags(line.id, withAlwaysOn(ids, alwaysOn?.id, isAlwaysOn))}
                            onCreate={actions.createTag}
                        />
                        <Select
                            className="line-row-priority"
                            label="Priority"
                            value={String(line.priority)}
                            options={priorityOptions}
                            onChange={value => actions.saveLine(line, { priority: Number(value) })}
                        />
                    </div>
                ) : (
                    <span className="line-row-summary">{lineSummary(tagIds.length, line.priority)}</span>
                )}
            </div>
            <div className="line-row-actions">
                <IconButton
                    icon="pin"
                    className="icon-button--small"
                    active={isAlwaysOn}
                    label={isAlwaysOn ? 'Always on. Click to let tailoring decide.' : 'Always show this line'}
                    onClick={() => actions.setLineTags(line.id, withAlwaysOn(line.tagIds, alwaysOn?.id, !isAlwaysOn))}
                />
                <IconButton
                    icon={isHidden ? 'eye-off' : 'eye'}
                    className="icon-button--small"
                    active={isHidden}
                    label={isHidden ? 'Hidden from the CV. Click to show.' : 'Hide from the CV'}
                    onClick={() => actions.saveLine(line, { override: isHidden ? 'none' : 'hidden' })}
                />
                <IconButton icon="trash" className="icon-button--small" label="Delete line" onClick={() => actions.deleteLine(line.id)} />
            </div>
        </div>
    );
};

export default LineRow;
