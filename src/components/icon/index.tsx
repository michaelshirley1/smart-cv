import React from 'react';
import { IconName, IconProps } from './model';

import './styles.scss';

const paths: Record<IconName, string> = {
    'chevron-down': 'M6 9l6 6 6-6',
    check: 'M5 12l5 5 9-11',
    expand: 'M15 3h6v6M9 21H3v-6M21 3l-7 7M3 21l7-7',
    collapse: 'M4 14h6v6M20 10h-6V4M14 10l7-7M3 21l7-7',
    plus: 'M12 5v14M5 12h14',
    close: 'M6 6l12 12M18 6L6 18',
    style: 'M4 21v-7M4 10V3M12 21v-9M12 8V3M20 21v-5M20 12V3M1 14h6M9 8h6M17 16h6',
    content: 'M8 6h13M8 12h13M8 18h13M3 6h.01M3 12h.01M3 18h.01',
};

const Icon: React.FC<IconProps> = ({ name, className }) => (
    <svg className={`icon${className ? ` ${className}` : ''}`} viewBox="0 0 24 24" aria-hidden="true" focusable="false">
        <path d={paths[name]} />
    </svg>
);

export default Icon;
