import React, { useEffect, useState } from 'react';
import { busyLabel, JobDescriptionProps, modeLabel } from './model';
import { useTailoring } from '../../tailoring-context';
import { useProfileContext } from '../../profile-context';
import * as api from '../../api';
import Disclosure from '../../../../components/disclosure';
import TextArea from '../../../../components/text-area';
import Button from '../../../../components/button';
import Chip from '../../../../components/chip';

import './styles.scss';

const JobDescription: React.FC<JobDescriptionProps> = ({ tags }) => {
    const { draft, setDraft, applied, result, loading, tailor, clear, source } = useTailoring();
    const usingTags = source?.kind === 'tags';
    const { settings } = useProfileContext();
    const [ollamaModels, setOllamaModels] = useState(0);
    const changed = applied !== null && draft.trim() !== applied;
    const colourOf = (tagId: number) => tags.find(tag => tag.id === tagId)?.colour ?? '';
    const usingKeywords = settings?.matchingMode !== 'ollama';
    const canTailor = draft.trim().length > 0 && !loading && (applied === null || changed || usingTags);

    useEffect(() => {
        if (!settings || !usingKeywords) return;
        api.getOllamaStatus().then(status => setOllamaModels(status.reachable ? status.models.length : 0)).catch(() => setOllamaModels(0));
    }, [settings?.matchingMode, settings?.ollamaUrl]);

    return (
        <Disclosure title="Job description" summary={applied ? 'Applied' : undefined} defaultOpen>
            <TextArea
                label="Paste the job description"
                value={draft}
                rows={7}
                placeholder="Paste the full advert here. Your tags and keywords are matched against it."
                onChange={setDraft}
                onKeyDown={e => {
                    if ((e.ctrlKey || e.metaKey) && e.key === 'Enter' && canTailor) {
                        e.preventDefault();
                        tailor();
                    }
                }}
            />
            <div className="job-description-actions">
                <Button onClick={tailor} disabled={!canTailor} title="Tailor (Ctrl+Enter)">
                    {loading ? busyLabel(settings) : applied === null ? 'Tailor CV' : 'Update'}
                </Button>
                {applied !== null && <Button variant="ghost" onClick={clear}>Clear</Button>}
                <span className="job-description-mode">{modeLabel(result, settings)}</span>
            </div>
            {usingKeywords && ollamaModels > 0 && !result && (
                <p className="job-description-hint">
                    Ollama is running with {ollamaModels} model{ollamaModels === 1 ? '' : 's'}. Turn it on in Settings for matching by meaning.
                </p>
            )}
            {usingTags && (
                <p className="job-description-hint">
                    The CV is tailored to the tags chosen in the top bar. Tailoring by a job description will replace them.
                </p>
            )}
            {result?.notice && <p className="job-description-notice" role="status">{result.notice}</p>}
            {result && !usingTags && (
                <div className="job-description-result">
                    <span className="job-description-result-label">Matched tags</span>
                    {result.matchedTags.length === 0 ? (
                        <p className="job-description-empty">
                            {result.mode === 'ollama'
                                ? 'No tags matched by keyword. Lines were scored by Ollama.'
                                : 'Nothing matched. Add keywords to your tags in the Tags section below.'}
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
