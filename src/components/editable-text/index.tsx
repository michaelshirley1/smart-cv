import React, { useLayoutEffect, useRef, useState } from 'react';
import { EditableTextProps } from './model';

import './styles.scss';

const EditableText: React.FC<EditableTextProps> = ({ value, placeholder = '', multiline = false, className, format, onSave }) => {
    const [editing, setEditing] = useState(false);
    const element = useRef<HTMLSpanElement>(null);

    useLayoutEffect(() => {
        const node = element.current;
        if (!editing || !node) return;

        node.textContent = value;
        node.focus();
        const selection = window.getSelection();
        selection?.selectAllChildren(node);
        selection?.collapseToEnd();
    }, [editing]);

    const display = value ? (format ? format(value) : value) : placeholder;
    const classes = [
        'editable-text',
        onSave ? 'editable-text--interactive' : '',
        editing ? 'editable-text--editing' : '',
        !value && !editing ? 'editable-text--placeholder' : '',
        className ?? '',
    ].filter(Boolean).join(' ');

    if (!onSave) return <span className={classes}>{display}</span>;

    const finish = (save: boolean) => {
        const node = element.current;
        const next = (node?.textContent ?? '').replace(/\s*\n\s*/g, multiline ? '\n' : ' ').trim();
        if (node) node.textContent = '';
        setEditing(false);
        if (save && next !== value) onSave(next);
    };

    const handleKeyDown = (e: React.KeyboardEvent<HTMLSpanElement>) => {
        if (!editing) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                setEditing(true);
            }
            return;
        }

        if (e.key === 'Escape') {
            e.preventDefault();
            e.stopPropagation();
            finish(false);
        } else if (e.key === 'Enter' && (!multiline || !e.shiftKey)) {
            e.preventDefault();
            finish(true);
        }
    };

    return (
        <span
            ref={element}
            className={classes}
            role={editing ? 'textbox' : 'button'}
            tabIndex={0}
            contentEditable={editing ? 'plaintext-only' : false}
            suppressContentEditableWarning
            aria-label={placeholder || undefined}
            onClick={e => {
                e.preventDefault();
                if (!editing) setEditing(true);
            }}
            onKeyDown={handleKeyDown}
            onBlur={() => editing && finish(true)}
        >
            {editing ? null : display}
        </span>
    );
};

export default EditableText;
