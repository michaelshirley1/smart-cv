import React from 'react';
import { LineRowProps } from './model';
import { withAlwaysOn } from '../model';
import { formatInline } from '../../cv-wrapper/format';
import { useContentActions } from '../actions';
import DragHandle from '../../../../components/drag-handle';
import EditableText from '../../../../components/editable-text';
import IconButton from '../../../../components/icon-button';
import TagInput from '../../../../components/tag-input';

import './styles.scss';

const LineRow: React.FC<LineRowProps> = ({ line, tagOptions, alwaysOn, handleProps }) => {
    const actions = useContentActions();
    const isAlwaysOn = alwaysOn !== undefined && line.tagIds.includes(alwaysOn.id);
    const isHidden = line.override === 'hidden';

    return (
        <div className={`line-row${isHidden ? ' line-row--hidden' : ''}${isAlwaysOn ? ' line-row--always-on' : ''}`}>
            <DragHandle label="Line" {...handleProps} />
            <div className="line-row-main">
                <EditableText
                    className="line-row-text"
                    value={line.text}
                    multiline
                    format={formatInline}
                    onSave={text => (text ? actions.saveLine(line, { text }) : actions.deleteLine(line.id))}
                />
                <TagInput
                    tags={tagOptions}
                    selectedIds={line.tagIds.filter(id => id !== alwaysOn?.id)}
                    onChange={ids => actions.setLineTags(line.id, withAlwaysOn(ids, alwaysOn?.id, isAlwaysOn))}
                    onCreate={actions.createTag}
                />
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
