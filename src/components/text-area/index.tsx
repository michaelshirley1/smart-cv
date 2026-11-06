import React, { useId } from 'react';
import { TextAreaProps } from './model';

import './styles.scss';

const TextArea: React.FC<TextAreaProps> = ({ label, value, onChange, className, rows = 6, ...rest }) => {
    const id = useId();

    return (
        <div className={`text-area${className ? ` ${className}` : ''}`}>
            <label className="text-area-label" htmlFor={id}>{label}</label>
            <textarea
                id={id}
                className="text-area-input"
                rows={rows}
                value={value}
                onChange={e => onChange(e.target.value)}
                {...rest}
            />
        </div>
    );
};

export default TextArea;
