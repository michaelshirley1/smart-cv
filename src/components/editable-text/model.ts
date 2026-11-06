import React from 'react';

export interface EditableTextProps {
    value: string;
    placeholder?: string;
    multiline?: boolean;
    className?: string;
    format?: (value: string) => React.ReactNode;
    onSave?: (value: string) => void;
}
