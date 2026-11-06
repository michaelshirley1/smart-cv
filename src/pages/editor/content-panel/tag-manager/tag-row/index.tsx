import React, { useState } from 'react';
import { parseKeywords, placeLabel, TagRowProps, usageLabel } from './model';
import { colourClass, tagColours } from '../../model';
import { useContentActions } from '../../actions';
import EditableText from '../../../../../components/editable-text';
import IconButton from '../../../../../components/icon-button';
import Icon from '../../../../../components/icon';
import ConfirmDialog from '../../../../../components/confirm-dialog';

import './styles.scss';

const TagRow: React.FC<TagRowProps> = ({ tag }) => {
    const actions = useContentActions();
    const [confirmDelete, setConfirmDelete] = useState(false);

    if (tag.isAlwaysOn) {
        return (
            <li className="tag-row tag-row--always-on">
                <div className="tag-row-header">
                    <Icon name="pin" className="tag-row-pin" />
                    <span className="tag-row-name">{tag.name}</span>
                    <span className="tag-row-usage">{usageLabel(tag.usageCount)}</span>
                </div>
                <p className="tag-row-description">Lines and sections with the pin turned on are always shown.</p>
            </li>
        );
    }

    const remove = () => (tag.usageCount > 0 ? setConfirmDelete(true) : actions.deleteTag(tag.id));

    return (
        <li className="tag-row">
            <div className="tag-row-header">
                <span className={`tag-row-swatch tag-row-swatch--${colourClass(tag.colour)}`} />
                <EditableText
                    className="tag-row-name"
                    value={tag.name}
                    onSave={name => name && actions.saveTag(tag, { name })}
                />
                <span className="tag-row-usage">{usageLabel(tag.usageCount)}</span>
                <IconButton icon="trash" className="icon-button--small" label={`Delete ${tag.name}`} onClick={remove} />
            </div>
            <EditableText
                className="tag-row-keywords"
                value={tag.keywords.join(', ')}
                placeholder="Add keywords, separated by commas"
                onSave={value => actions.saveTag(tag, { keywords: parseKeywords(value) })}
            />
            <div className="tag-row-colours" role="radiogroup" aria-label={`${tag.name} colour`}>
                {tagColours.map(colour => (
                    <button
                        key={colourClass(colour)}
                        type="button"
                        role="radio"
                        aria-checked={tag.colour === colour}
                        aria-label={colour || 'grey'}
                        title={colour || 'grey'}
                        className={`tag-row-colour tag-row-swatch--${colourClass(colour)}${tag.colour === colour ? ' tag-row-colour--selected' : ''}`}
                        onClick={() => tag.colour !== colour && actions.saveTag(tag, { colour })}
                    />
                ))}
            </div>
            {confirmDelete && (
                <ConfirmDialog
                    title="Delete tag"
                    message={<>Delete <strong>{tag.name}</strong>? It will be removed from {placeLabel(tag.usageCount)}.</>}
                    confirmLabel="Delete"
                    onConfirm={() => actions.deleteTag(tag.id)}
                    onClose={() => setConfirmDelete(false)}
                />
            )}
        </li>
    );
};

export default TagRow;
