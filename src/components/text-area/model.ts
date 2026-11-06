import React from 'react';

export interface TextAreaProps extends Omit<React.TextareaHTMLAttributes<HTMLTextAreaElement>, 'onChange'> {
    label: string;
    value: string;
    onChange: (value: string) => void;
}
