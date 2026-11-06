import React, { useEffect, useId, useState } from 'react';
import { FontFieldProps, primaryFamily, withFamily } from './model';
import TextField from '../text-field';

const FontField: React.FC<FontFieldProps> = ({ label, value, fonts, onChange, className }) => {
    const listId = useId();
    const current = primaryFamily(value);
    const [text, setText] = useState(current);

    useEffect(() => setText(current), [current]);

    const commit = (next: string) => {
        const family = next.trim();
        if (family && family !== current) onChange(withFamily(value, family));
        else setText(current);
    };

    const handleChange = (next: string) => {
        setText(next);
        if (fonts.includes(next)) commit(next);
    };

    return (
        <>
            <TextField
                className={className}
                label={label}
                value={text}
                list={listId}
                spellCheck={false}
                onChange={handleChange}
                onBlur={() => commit(text)}
                onKeyDown={e => e.key === 'Enter' && commit(text)}
            />
            <datalist id={listId}>
                {fonts.map(font => <option key={font} value={font} />)}
            </datalist>
        </>
    );
};

export default FontField;
