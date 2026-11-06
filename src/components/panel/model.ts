import React from 'react';
import { IconName } from '../icon/model';

export interface PanelProps {
    side: 'left' | 'right';
    title: string;
    icon: IconName;
    isFull: boolean;
    onToggleFull: () => void;
    className?: string;
    children: React.ReactNode;
}
