import React, { createContext, useContext, useState } from 'react';
import { ActiveLineContextValue, ActiveLineProviderProps } from './model';

const ActiveLineContext = createContext<ActiveLineContextValue | null>(null);

export const useActiveLine = (): ActiveLineContextValue => {
    const context = useContext(ActiveLineContext);
    if (!context) throw new Error('useActiveLine must be used inside ActiveLineProvider.');
    return context;
};

export const ActiveLineProvider: React.FC<ActiveLineProviderProps> = ({ children }) => {
    const [activeLineId, setActiveLineId] = useState<number | null>(null);

    return (
        <ActiveLineContext.Provider value={{ activeLineId, setActiveLineId }}>
            {children}
        </ActiveLineContext.Provider>
    );
};
