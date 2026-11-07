import React from 'react';

export interface ActiveLineProviderProps {
    children: React.ReactNode;
}

export interface ActiveLineContextValue {
    activeLineId: number | null;
    setActiveLineId: (id: number | null) => void;
}
