import React from 'react';
import { JobDescriptionProps } from './model';
import { useTailoring } from '../../tailoring-context';
import Disclosure from '../../../../components/disclosure';
import TextArea from '../../../../components/text-area';
import Button from '../../../../components/button';
import Chip from '../../../../components/chip';

import './styles.scss';

const JobDescription: React.FC<JobDescriptionProps> = ({ tags }) => {
    const { draft, setDraft, applied, result, loading, tailor, clear } = useTailoring();
    const changed = applied !== null && draft.trim() !== applied;
    const colourOf = (tagId: number) => tags.find(tag => tag.id === tagId)?.colour ?? '';

    return (
        <Disclosure title="Job description" summary={applied ? 'Applied' : undefined} defaultOpen>
            <TextArea
                label="Paste the job description"
                value={draft}
                rows={7}
                placeholder="Paste the full advert here. Your tags and keywords are matched against it."
                onChange={setDraft}
            />
            <div className="job-description-actions">
                <Button onClick={tailor} disabled={!draft.trim() || loading || (applied !== null && !changed)}>
                    {applied === null ? 'Tailor CV' : 'Update'}
                </Button>
                {applied !== null && <Button variant="ghost" onClick={clear}>Clear</Button>}
                <span className="job-description-mode">Keyword matching</span>
            </div>
            {result && (
                <div className="job-description-result">
                    <span className="job-description-result-label">Matched tags</span>
                    {result.matchedTags.length === 0 ? (
                        <p className="job-description-empty">
                            Nothing matched. Add keywords to your tags in the Tags section below.
                        </p>
                    ) : (
                        <div className="job-description-matches">
                            {result.matchedTags.map(match => (
                                <span key={match.tagId} title={`Found: ${match.terms.join(', ')}`}>
                                    <Chip label={`${match.name} · ${match.score}`} colour={colourOf(match.tagId)} />
                                </span>
                            ))}
                        </div>
                    )}
                </div>
            )}
        </Disclosure>
    );
};

export default JobDescription;
