import React, { useId } from 'react';
import { SelectProps } from './model';

import './styles.scss';

const Select = <T extends string>({ label, value, options, onChange, className }: SelectProps<T>) => {
    const id = useId();
    const choices = options.some(option => option.value === value) ? options : [{ value, label: 'Custom' }, ...options];

    return (
        <div className={`select${className ? ` ${className}` : ''}`}>
            <label className="select-label" htmlFor={id}>{label}</label>
            <select id={id} className="select-input" value={value} onChange={e => onChange(e.target.value as T)}>
                {choices.map(option => (
                    <option key={option.value} value={option.value}>{option.label}</option>
                ))}
            </select>
        </div>
    );
};

export default Select;
