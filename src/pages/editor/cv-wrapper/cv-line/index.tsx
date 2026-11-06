import React from 'react';
import { CvLineProps } from './model';
import { formatInline } from '../format';
import { toLineRequest } from '../../api';
import EditableText from '../../../../components/editable-text';

import './styles.scss';

const CvLine: React.FC<CvLineProps> = ({ line, variant, isPlaceholder, onSave }) => {
    const Element = variant === 'bullet' ? 'li' : 'p';
    const save = onSave && !isPlaceholder
        ? (text: string) => text && onSave(line.id, { ...toLineRequest(line), text })
        : undefined;

    return (
        <Element className={`cv-line cv-line--${variant}${isPlaceholder ? ' cv-line--placeholder' : ''}`} data-line-id={line.id}>
            <EditableText value={line.text} format={formatInline} multiline onSave={save} />
        </Element>
    );
};

export default CvLine;
