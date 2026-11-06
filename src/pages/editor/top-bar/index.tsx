import React from 'react';
import { TopBarProps, viewOptions } from './model';
import { useTailoring } from '../tailoring-context';
import ProfileSwitcher from './profile-switcher';
import SegmentedControl from '../../../components/segmented-control';

import './styles.scss';

const TopBar: React.FC<TopBarProps> = () => {
    const { result, view, setView } = useTailoring();

    return (
        <header className="top-bar">
            <span className="top-bar-title">Smart CV</span>
            <ProfileSwitcher />
            <div className="top-bar-actions">
                {result && <SegmentedControl label="CV view" value={view} options={viewOptions} onChange={setView} />}
            </div>
        </header>
    );
};

export default TopBar;
