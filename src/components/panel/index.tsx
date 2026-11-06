import React from 'react';
import { PanelProps } from './model';
import Icon from '../icon';
import IconButton from '../icon-button';

import './styles.scss';

const Panel: React.FC<PanelProps> = ({ side, title, icon, isFull, onToggleFull, className, children }) => (
    <aside className={`panel panel--${side}${isFull ? ' panel--full' : ''}${className ? ` ${className}` : ''}`} aria-label={title}>
        <header className="panel-header">
            <Icon name={icon} className="panel-header-icon" />
            <h2 className="panel-header-title">{title}</h2>
            <IconButton
                icon={isFull ? 'collapse' : 'expand'}
                label={isFull ? `Collapse ${title.toLowerCase()} panel` : `Expand ${title.toLowerCase()} panel`}
                className="panel-header-toggle"
                onClick={onToggleFull}
            />
        </header>
        <div className="panel-body">{children}</div>
    </aside>
);

export default Panel;
