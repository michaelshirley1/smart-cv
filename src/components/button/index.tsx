import React from 'react';
import { ButtonProps } from './model';

import './styles.scss';

const Button: React.FC<ButtonProps> = ({ variant = 'primary', className, type = 'button', children, ...rest }) => (
    <button
        type={type}
        className={`button button--${variant}${className ? ` ${className}` : ''}`}
        {...rest}
    >
        {children}
    </button>
);

export default Button;
