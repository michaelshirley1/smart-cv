import React, { useEffect, useState } from 'react';
import { EditorLayoutProps } from './model';
import { PanelMode } from '../model';
import Panel from '../../../components/panel';
import StylePanel from '../style-panel';
import ContentPanel from '../content-panel';
import CvCanvas from '../cv-canvas';

import './styles.scss';

const EditorLayout: React.FC<EditorLayoutProps> = () => {
    const [panelMode, setPanelMode] = useState<PanelMode>('rest');

    useEffect(() => {
        const handleKeyDown = (e: KeyboardEvent) => {
            if (e.key === 'Escape') setPanelMode('rest');
        };
        window.addEventListener('keydown', handleKeyDown);
        return () => window.removeEventListener('keydown', handleKeyDown);
    }, []);

    const toggle = (mode: PanelMode) => setPanelMode(current => (current === mode ? 'rest' : mode));

    return (
        <div className={`editor-layout editor-layout--${panelMode}`}>
            <Panel
                side="left"
                title="Style"
                icon="style"
                className="editor-layout-left"
                isFull={panelMode === 'left-full'}
                onToggleFull={() => toggle('left-full')}
            >
                <StylePanel />
            </Panel>
            <main className="editor-layout-centre">
                <CvCanvas />
            </main>
            <Panel
                side="right"
                title="Content"
                icon="content"
                className="editor-layout-right"
                isFull={panelMode === 'right-full'}
                onToggleFull={() => toggle('right-full')}
            >
                <ContentPanel />
            </Panel>
        </div>
    );
};

export default EditorLayout;
