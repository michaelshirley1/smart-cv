import React, { useId } from 'react';
import { TextFieldProps } from './model';

import './styles.scss';

const TextField: React.FC<TextFieldProps> = ({ label, value, onChange, className, type = 'text', ...rest }) => {
    const id = useId();

    return (
        <div className={`text-field${className ? ` ${className}` : ''}`}>
            <label className="text-field-label" htmlFor={id}>{label}</label>
            <input
                id={id}
                type={type}
                className="text-field-input"
                value={value}
                onChange={e => onChange(e.target.value)}
                {...rest}
            />
        </div>
    );
};

export default TextField;
