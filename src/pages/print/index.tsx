import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { PrintJob, PrintPageProps } from './model';
import { getPrintJob } from './api';
import { CvSelection } from '../editor/cv-wrapper/model';
import CvWrapper from '../editor/cv-wrapper';

import './styles.scss';

const PrintPage: React.FC<PrintPageProps> = () => {
    const { jobId = '' } = useParams();
    const [job, setJob] = useState<PrintJob | null>(null);
    const [error, setError] = useState(false);
    const [stylesLoaded, setStylesLoaded] = useState(false);
    const [ready, setReady] = useState(false);

    useEffect(() => {
        getPrintJob(jobId).then(setJob).catch(() => setError(true));
    }, [jobId]);

    useEffect(() => {
        if (!stylesLoaded) return;
        document.fonts.ready.then(() => setReady(true));
    }, [stylesLoaded]);

    if (error) return <p className="print-page-error">This export has expired.</p>;
    if (!job) return null;

    const selection: CvSelection | undefined = job.sectionIds && job.lineIds
        ? { sectionIds: new Set(job.sectionIds), lineIds: new Set(job.lineIds) }
        : undefined;

    return (
        <div className="print-page" data-print-ready={ready || undefined}>
            <link
                rel="stylesheet"
                href={`/templates/${job.profile.templateName}.css`}
                onLoad={() => setStylesLoaded(true)}
                onError={() => setStylesLoaded(true)}
            />
            <CvWrapper profile={job.profile} variant="print" showPlaceholders={false} selection={selection} />
        </div>
    );
};

export default PrintPage;
