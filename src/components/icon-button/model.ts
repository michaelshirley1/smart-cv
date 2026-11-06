import React from 'react';
import { IconName } from '../icon/model';

export interface IconButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
    icon: IconName;
    label: string;
    active?: boolean;
}
