import React from 'react';
import { SegmentedControlProps } from './model';

import './styles.scss';

const SegmentedControl = <T extends string>({ label, value, options, onChange }: SegmentedControlProps<T>) => (
    <div className="segmented-control" role="radiogroup" aria-label={label}>
        {options.map(option => (
            <button
                key={option.value}
                type="button"
                role="radio"
                aria-checked={option.value === value}
                disabled={option.disabled}
                className={`segmented-control-option${option.value === value ? ' segmented-control-option--active' : ''}`}
                onClick={() => onChange(option.value)}
            >
                {option.label}
            </button>
        ))}
    </div>
);

export default SegmentedControl;
