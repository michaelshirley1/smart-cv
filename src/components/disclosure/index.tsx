import React from 'react';
import { DisclosureProps } from './model';
import Icon from '../icon';

import './styles.scss';

const Disclosure: React.FC<DisclosureProps> = ({ title, summary, defaultOpen = false, children }) => (
    <details className="disclosure" open={defaultOpen}>
        <summary className="disclosure-header">
            <Icon name="chevron-right" className="disclosure-chevron" />
            <span className="disclosure-title">{title}</span>
            {summary && <span className="disclosure-summary">{summary}</span>}
        </summary>
        <div className="disclosure-body">{children}</div>
    </details>
);

export default Disclosure;
