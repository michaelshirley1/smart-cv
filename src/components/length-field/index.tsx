import React from 'react';
import { defaultStep, LengthFieldProps, parseLength } from './model';
import NumberField from '../number-field';
import TextField from '../text-field';

const LengthField: React.FC<LengthFieldProps> = ({ label, value, onChange, step, max, className }) => {
    const length = parseLength(value);

    if (!length) {
        return <TextField className={className} label={label} value={value} onChange={onChange} />;
    }

    return (
        <NumberField
            className={className}
            label={label}
            value={length.amount}
            unit={length.unit}
            min={0}
            max={max}
            step={step ?? defaultStep(length.unit)}
            onChange={amount => onChange(`${Number(amount.toFixed(3))}${length.unit}`)}
        />
    );
};

export default LengthField;
