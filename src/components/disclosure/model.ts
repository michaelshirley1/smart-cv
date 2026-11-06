import React from 'react';

export interface DisclosureProps {
    title: string;
    summary?: string;
    defaultOpen?: boolean;
    children: React.ReactNode;
}
