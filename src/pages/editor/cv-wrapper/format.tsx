import React from 'react';

const boldPattern = /(\*\*[^*]+\*\*)/g;

export const formatInline = (text: string): React.ReactNode =>
    text
        .split(boldPattern)
        .filter(part => part.length > 0)
        .map((part, index) =>
            part.length > 4 && part.startsWith('**') && part.endsWith('**')
                ? <strong key={index} className="cv-strong">{part.slice(2, -2)}</strong>
                : <React.Fragment key={index}>{part}</React.Fragment>,
        );
