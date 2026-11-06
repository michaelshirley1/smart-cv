import React from 'react';
import { CvCanvasProps } from './model';
import { useProfileContext } from '../profile-context';

import './styles.scss';

const CvCanvas: React.FC<CvCanvasProps> = () => {
    const { profile } = useProfileContext();

    return (
        <div className="cv-canvas">
            <div className="cv-canvas-page">
                <span className="cv-canvas-page-name">{profile?.fullName || profile?.name || ''}</span>
            </div>
        </div>
    );
};

export default CvCanvas;
