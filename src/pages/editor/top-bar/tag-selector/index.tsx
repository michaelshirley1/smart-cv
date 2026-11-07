import React, { useEffect, useRef, useState } from 'react';
import { selectionSummary, TagSelectorProps } from './model';
import { useProfileContext } from '../../profile-context';
import { useTailoring } from '../../tailoring-context';
import { useReorder } from '../../../../hooks/useReorder';
import Icon from '../../../../components/icon';
import Chip from '../../../../components/chip';
import DragHandle from '../../../../components/drag-handle';
import IconButton from '../../../../components/icon-button';
import Select from '../../../../components/select';
import Button from '../../../../components/button';

import './styles.scss';

const TagSelector: React.FC<TagSelectorProps> = () => {
    const { profile } = useProfileContext();
    const { tagIds, setTagIds } = useTailoring();
    const [open, setOpen] = useState(false);
    const container = useRef<HTMLDivElement>(null);
    const reorder = useReorder('tailor-tags', tagIds, setTagIds);

    useEffect(() => {
        if (!open) return;

        const handleMouseDown = (e: MouseEvent) => {
            if (!container.current?.contains(e.target as Node)) setOpen(false);
        };
        const handleKeyDown = (e: KeyboardEvent) => {
            if (e.key === 'Escape') setOpen(false);
        };

        document.addEventListener('mousedown', handleMouseDown);
        document.addEventListener('keydown', handleKeyDown);
        return () => {
            document.removeEventListener('mousedown', handleMouseDown);
            document.removeEventListener('keydown', handleKeyDown);
        };
    }, [open]);

    if (!profile) return null;

    const tags = profile.tags.filter(tag => !tag.isAlwaysOn);
    const remaining = tags.filter(tag => !tagIds.includes(tag.id));
    const active = tagIds.length > 0;

    return (
        <div className="tag-selector" ref={container}>
            <button
                type="button"
                className={`tag-selector-trigger${active ? ' tag-selector-trigger--active' : ''}`}
                aria-haspopup="dialog"
                aria-expanded={open}
                onClick={() => setOpen(current => !current)}
            >
                <Icon name="tag" />
                <span className="tag-selector-label">Tags</span>
                <span className="tag-selector-summary">{selectionSummary(tagIds, tags)}</span>
                <Icon name="chevron-down" />
            </button>

            {open && (
                <div className="tag-selector-panel" role="dialog" aria-label="Tailor to tags">
                    <p className="tag-selector-title">Tailor to tags</p>
                    <p className="tag-selector-hint">
                        {tags.length === 0
                            ? 'Create tags in the Content panel first.'
                            : 'Choose tags to tailor the CV without a job description. Drag to set importance: the first tag counts most.'}
                    </p>

                    {active && (
                        <ol className="tag-selector-list">
                            {tagIds.map((id, index) => {
                                const tag = tags.find(candidate => candidate.id === id);
                                if (!tag) return null;
                                const drop = reorder.dropPosition(id);
                                return (
                                    <li
                                        key={id}
                                        className={`tag-selector-item${drop ? ` tag-selector-item--drop-${drop}` : ''}`}
                                        {...reorder.itemProps(id)}
                                    >
                                        <DragHandle label={`Tag ${tag.name}`} {...reorder.handleProps(id)} />
                                        <span className="tag-selector-rank">{index + 1}</span>
                                        <Chip label={tag.name} colour={tag.colour} />
                                        <IconButton
                                            icon="close"
                                            className="icon-button--small tag-selector-remove"
                                            label={`Remove ${tag.name}`}
                                            onClick={() => setTagIds(tagIds.filter(candidate => candidate !== id))}
                                        />
                                    </li>
                                );
                            })}
                        </ol>
                    )}

                    {remaining.length > 0 && (
                        <Select
                            label="Add a tag"
                            value=""
                            options={[{ value: '', label: 'Choose…' }, ...remaining.map(tag => ({ value: String(tag.id), label: tag.name }))]}
                            onChange={value => value && setTagIds([...tagIds, Number(value)])}
                        />
                    )}

                    {active && (
                        <Button variant="ghost" className="tag-selector-clear" onClick={() => setTagIds([])}>Clear tags</Button>
                    )}
                </div>
            )}
        </div>
    );
};

export default TagSelector;
