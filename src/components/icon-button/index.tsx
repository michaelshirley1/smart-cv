import React from 'react';
import { IconButtonProps } from './model';
import Icon from '../icon';

import './styles.scss';

const IconButton: React.FC<IconButtonProps> = ({ icon, label, active, className, type = 'button', ...rest }) => (
    <button
        type={type}
        className={`icon-button${active ? ' icon-button--active' : ''}${className ? ` ${className}` : ''}`}
        aria-pressed={active}
        aria-label={label}
        title={label}
        {...rest}
    >
        <Icon name={icon} />
    </button>
);

export default IconButton;
