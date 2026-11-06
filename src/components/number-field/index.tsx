import React, { useEffect, useId, useState } from 'react';
import { NumberFieldProps } from './model';

import './styles.scss';

const NumberField: React.FC<NumberFieldProps> = ({ label, value, onChange, min, max, step = 1, unit, className }) => {
    const id = useId();
    const [text, setText] = useState(String(value));

    useEffect(() => {
        if (Number.parseFloat(text) !== value) setText(String(value));
    }, [value]);

    const handleChange = (next: string) => {
        setText(next);
        const parsed = Number.parseFloat(next);
        const inRange = !Number.isNaN(parsed) && (min === undefined || parsed >= min) && (max === undefined || parsed <= max);
        if (inRange && parsed !== value) onChange(parsed);
    };

    return (
        <div className={`number-field${className ? ` ${className}` : ''}`}>
            <label className="number-field-label" htmlFor={id}>{label}</label>
            <div className="number-field-control">
                <input
                    id={id}
                    type="number"
                    className="number-field-input"
                    value={text}
                    min={min}
                    max={max}
                    step={step}
                    onChange={e => handleChange(e.target.value)}
                    onBlur={() => setText(String(value))}
                />
                {unit && <span className="number-field-unit">{unit}</span>}
            </div>
        </div>
    );
};

export default NumberField;
