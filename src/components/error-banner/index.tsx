import React from 'react';
import { ErrorBannerProps } from './model';
import IconButton from '../icon-button';

import './styles.scss';

const ErrorBanner: React.FC<ErrorBannerProps> = ({ message, onDismiss }) => (
    <div className="error-banner" role="alert">
        <span className="error-banner-message">{message}</span>
        {onDismiss && <IconButton icon="close" label="Dismiss" className="error-banner-dismiss" onClick={onDismiss} />}
    </div>
);

export default ErrorBanner;
