import React from 'react';
import { DragHandleProps } from './model';
import Icon from '../icon';

import './styles.scss';

const DragHandle: React.FC<DragHandleProps> = ({ label, ...handleProps }) => (
    <button
        type="button"
        className="drag-handle"
        aria-label={`${label}. Drag, or use Alt and the arrow keys, to reorder.`}
        title="Drag to reorder (Alt + ↑ / ↓)"
        {...handleProps}
    >
        <Icon name="grip" />
    </button>
);

export default DragHandle;
