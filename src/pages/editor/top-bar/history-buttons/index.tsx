import React from 'react';
import { HistoryButtonsProps, isRedo, isUndo } from './model';
import { useProfileContext } from '../../profile-context';
import { useShortcut } from '../../../../hooks/useShortcut';
import IconButton from '../../../../components/icon-button';

import './styles.scss';

const HistoryButtons: React.FC<HistoryButtonsProps> = () => {
    const { canUndo, canRedo, undo, redo } = useProfileContext();

    useShortcut(isUndo, undo);
    useShortcut(isRedo, redo);

    return (
        <div className="history-buttons">
            <IconButton icon="undo" label="Undo (Ctrl+Z)" disabled={!canUndo} onClick={undo} />
            <IconButton icon="redo" label="Redo (Ctrl+Shift+Z)" disabled={!canRedo} onClick={redo} />
        </div>
    );
};

export default HistoryButtons;
