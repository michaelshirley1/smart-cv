import React from 'react';
import { TopBarProps } from './model';
import ProfileSwitcher from './profile-switcher';

import './styles.scss';

const TopBar: React.FC<TopBarProps> = () => (
    <header className="top-bar">
        <span className="top-bar-title">Smart CV</span>
        <ProfileSwitcher />
        <div className="top-bar-actions" />
    </header>
);

export default TopBar;
