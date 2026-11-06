import React from 'react';
import { StyleGroupProps } from './model';
import { StyleControl } from '../model';
import Disclosure from '../../../../components/disclosure';
import ColourField from '../../../../components/colour-field';
import FontField from '../../../../components/font-field';
import LengthField from '../../../../components/length-field';
import NumberField from '../../../../components/number-field';
import Select from '../../../../components/select';

import './styles.scss';

const StyleGroup: React.FC<StyleGroupProps> = ({ definition, values, fonts, onChange }) => {
    const renderControl = (control: StyleControl) => {
        const value = values[control.key] ?? '';
        const set = (next: string) => onChange(control.key, next);
        const className = `style-group-control style-group-control--${control.kind}`;

        switch (control.kind) {
            case 'font':
                return <FontField key={control.key} className={className} label={control.label} value={value} fonts={fonts} onChange={set} />;
            case 'colour':
                return <ColourField key={control.key} className={className} label={control.label} value={value} onChange={set} />;
            case 'length':
                return <LengthField key={control.key} className={className} label={control.label} value={value} max={control.max} onChange={set} />;
            case 'number':
                return (
                    <NumberField
                        key={control.key}
                        className={className}
                        label={control.label}
                        value={Number.parseFloat(value) || control.min}
                        min={control.min}
                        max={control.max}
                        step={control.step}
                        onChange={next => set(String(next))}
                    />
                );
            case 'choice':
                return <Select key={control.key} className={className} label={control.label} value={value} options={control.options} onChange={set} />;
        }
    };

    return (
        <Disclosure title={definition.title}>
            <div className="style-group">
                {definition.controls.filter(control => control.key in values).map(renderControl)}
            </div>
        </Disclosure>
    );
};

export default StyleGroup;
