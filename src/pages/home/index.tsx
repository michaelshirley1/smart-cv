import React, { useEffect, useState } from 'react';
import { HomePageProps, Item } from './model';
import { getSavedCv } from './api';

import './styles.scss';

export const HomePage: React.FC<HomePageProps> = () => {
    const [cv, setCv] = useState<Item | null>(null);

    useEffect(() => {
        getSavedCv(1).then(setCv);
    }, []);

    return (
        <div className="home-page">
            <h1>Smart CV</h1>
            {cv && (
                <div className="home-page-cv">
                    <span className="home-page-cv-name">{cv.name}</span>
                    <span className="home-page-cv-description">{cv.description}</span>
                </div>
            )}
        </div>
    );
};
