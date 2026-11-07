import React, { useState } from 'react';
import { TopBarProps, viewOptions } from './model';
import { useTailoring } from '../tailoring-context';
import ProfileSwitcher from './profile-switcher';
import SettingsDialog from './settings-dialog';
import ExportButton from './export-button';
import HistoryButtons from './history-buttons';
import TagSelector from './tag-selector';
import SegmentedControl from '../../../components/segmented-control';
import IconButton from '../../../components/icon-button';

import './styles.scss';

const TopBar: React.FC<TopBarProps> = () => {
    const { result, view, setView } = useTailoring();
    const [settingsOpen, setSettingsOpen] = useState(false);

    return (
        <header className="top-bar">
            <div className="top-bar-brand">
                <img className="top-bar-logo" src="/favicon.svg" alt="" />
                <span className="top-bar-title">Smart CV</span>
            </div>
            <ProfileSwitcher />
            <HistoryButtons />
            <TagSelector />
            <div className="top-bar-actions">
                {result && <SegmentedControl label="CV view" value={view} options={viewOptions} onChange={setView} />}
                <ExportButton />
                <IconButton icon="settings" label="Settings" onClick={() => setSettingsOpen(true)} />
            </div>
            {settingsOpen && <SettingsDialog onClose={() => setSettingsOpen(false)} />}
        </header>
    );
};

export default TopBar;
