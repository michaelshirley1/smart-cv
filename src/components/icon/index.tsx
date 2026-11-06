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
    grip: 'M9 6h.01M9 12h.01M9 18h.01M15 6h.01M15 12h.01M15 18h.01',
    trash: 'M3 6h18M8 6V4h8v2M19 6l-1 14H6L5 6',
    eye: 'M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12zM12 15a3 3 0 100-6 3 3 0 000 6z',
    'eye-off': 'M3 3l18 18M10.6 5.1A10 10 0 0112 5c6.5 0 10 7 10 7a17 17 0 01-3.2 4.2M6.6 6.6A17 17 0 002 12s3.5 7 10 7a9.7 9.7 0 005.4-1.6M9.9 9.9a3 3 0 004.2 4.2',
    pin: 'M12 17v5M5 17h14l-2-4V6h1V3H6v3h1v7l-2 4z',
    'chevron-right': 'M9 6l6 6-6 6',
    tag: 'M20.6 13.4l-7.2 7.2a2 2 0 01-2.8 0L3 13V3h10l7.6 7.6a2 2 0 010 2.8zM7.5 7.5h.01',
};

const Icon: React.FC<IconProps> = ({ name, className }) => (
    <svg className={`icon icon--${name}${className ? ` ${className}` : ''}`} viewBox="0 0 24 24" aria-hidden="true" focusable="false">
        <path d={paths[name]} />
    </svg>
);

export default Icon;
