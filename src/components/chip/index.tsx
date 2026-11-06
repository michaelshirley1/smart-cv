import React from 'react';
import { ChipProps } from './model';
import Icon from '../icon';

import './styles.scss';

const Chip: React.FC<ChipProps> = ({ label, colour = '', onRemove }) => (
    <span className={`chip chip--${colour || 'default'}`}>
        <span className="chip-label">{label}</span>
        {onRemove && (
            <button type="button" className="chip-remove" aria-label={`Remove ${label}`} title={`Remove ${label}`} onClick={onRemove}>
                <Icon name="close" />
            </button>
        )}
    </span>
);

export default Chip;
