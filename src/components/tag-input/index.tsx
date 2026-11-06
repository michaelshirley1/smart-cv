import React, { useState } from 'react';
import { TagInputProps, TagOption } from './model';
import Chip from '../chip';

import './styles.scss';

type Suggestion = { kind: 'tag'; tag: TagOption } | { kind: 'create'; name: string };

const TagInput: React.FC<TagInputProps> = ({ tags, selectedIds, onChange, onCreate, placeholder = 'Add tag' }) => {
    const [query, setQuery] = useState('');
    const [open, setOpen] = useState(false);
    const [highlighted, setHighlighted] = useState(0);

    const selected = selectedIds
        .map(id => tags.find(tag => tag.id === id))
        .filter((tag): tag is TagOption => tag !== undefined);

    const trimmed = query.trim();
    const search = trimmed.toLowerCase();
    const matches = tags.filter(tag => !selectedIds.includes(tag.id) && tag.name.toLowerCase().includes(search));
    const exists = tags.some(tag => tag.name.toLowerCase() === search);
    const suggestions: Suggestion[] = [
        ...matches.map(tag => ({ kind: 'tag' as const, tag })),
        ...(trimmed && !exists ? [{ kind: 'create' as const, name: trimmed }] : []),
    ];

    const reset = () => {
        setQuery('');
        setHighlighted(0);
    };

    const choose = async (suggestion: Suggestion) => {
        reset();
        if (suggestion.kind === 'tag') {
            onChange([...selectedIds, suggestion.tag.id]);
            return;
        }
        const id = await onCreate(suggestion.name);
        if (id !== null) onChange([...selectedIds, id]);
    };

    const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === 'ArrowDown') {
            e.preventDefault();
            setOpen(true);
            setHighlighted(current => Math.min(current + 1, suggestions.length - 1));
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            setHighlighted(current => Math.max(current - 1, 0));
        } else if (e.key === 'Enter' || e.key === ',') {
            e.preventDefault();
            const suggestion = suggestions[highlighted] ?? suggestions[0];
            if (trimmed && suggestion) choose(suggestion);
        } else if (e.key === 'Escape') {
            e.stopPropagation();
            reset();
            setOpen(false);
        } else if (e.key === 'Backspace' && !query && selectedIds.length > 0) {
            onChange(selectedIds.slice(0, -1));
        }
    };

    return (
        <div className="tag-input">
            {selected.map(tag => (
                <Chip
                    key={tag.id}
                    label={tag.name}
                    colour={tag.colour}
                    onRemove={() => onChange(selectedIds.filter(id => id !== tag.id))}
                />
            ))}
            <div className="tag-input-search">
                <input
                    className="tag-input-field"
                    value={query}
                    placeholder={placeholder}
                    aria-label={placeholder}
                    onChange={e => {
                        setQuery(e.target.value);
                        setHighlighted(0);
                        setOpen(true);
                    }}
                    onFocus={() => setOpen(true)}
                    onBlur={() => setOpen(false)}
                    onKeyDown={handleKeyDown}
                />
                {open && suggestions.length > 0 && (
                    <ul className="tag-input-options" role="listbox">
                        {suggestions.map((suggestion, index) => (
                            <li
                                key={suggestion.kind === 'tag' ? suggestion.tag.id : 'create'}
                                role="option"
                                aria-selected={index === highlighted}
                                className={`tag-input-option${index === highlighted ? ' tag-input-option--highlighted' : ''}`}
                                onMouseDown={e => {
                                    e.preventDefault();
                                    choose(suggestion);
                                }}
                                onMouseEnter={() => setHighlighted(index)}
                            >
                                {suggestion.kind === 'tag'
                                    ? <Chip label={suggestion.tag.name} colour={suggestion.tag.colour} />
                                    : <span className="tag-input-create">Create “{suggestion.name}”</span>}
                            </li>
                        ))}
                    </ul>
                )}
            </div>
        </div>
    );
};

export default TagInput;
