import React from 'react';
import { IconButtonProps } from './model';
import Icon from '../icon';

import './styles.scss';

const IconButton: React.FC<IconButtonProps> = ({ icon, label, className, type = 'button', ...rest }) => (
    <button
        type={type}
        className={`icon-button${className ? ` ${className}` : ''}`}
        aria-label={label}
        title={label}
        {...rest}
    >
        <Icon name={icon} />
    </button>
);

export default IconButton;
