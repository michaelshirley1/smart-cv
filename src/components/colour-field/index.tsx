import React, { useId } from 'react';
import { ColourFieldProps, isHexColour } from './model';
import TextField from '../text-field';

import './styles.scss';

const ColourField: React.FC<ColourFieldProps> = ({ label, value, onChange, className }) => {
    const id = useId();

    if (!isHexColour(value)) {
        return <TextField className={className} label={label} value={value} onChange={onChange} />;
    }

    return (
        <div className={`colour-field${className ? ` ${className}` : ''}`}>
            <label className="colour-field-label" htmlFor={id}>{label}</label>
            <div className="colour-field-control">
                <input id={id} type="color" className="colour-field-input" value={value} onChange={e => onChange(e.target.value)} />
                <span className="colour-field-value">{value.toLowerCase()}</span>
            </div>
        </div>
    );
};

export default ColourField;
