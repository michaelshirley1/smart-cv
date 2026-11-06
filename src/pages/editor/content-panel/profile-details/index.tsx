import React, { useEffect, useRef, useState } from 'react';
import { detailFields, mergeSaved, ProfileDetailsProps, sameRequest } from './model';
import { ProfileRequest } from '../../model';
import { toProfileRequest } from '../../api';
import { useContentActions } from '../actions';
import Disclosure from '../../../../components/disclosure';
import TextField from '../../../../components/text-field';
import IconButton from '../../../../components/icon-button';
import Button from '../../../../components/button';

import './styles.scss';

const ProfileDetails: React.FC<ProfileDetailsProps> = ({ profile }) => {
    const actions = useContentActions();
    const saved = toProfileRequest(profile);
    const [draft, setDraft] = useState<ProfileRequest>(saved);
    const previous = useRef<ProfileRequest>(saved);
    const savedKey = JSON.stringify(saved);

    useEffect(() => {
        setDraft(current => mergeSaved(current, previous.current, saved));
        previous.current = saved;
    }, [savedKey]);

    const save = (next: ProfileRequest) => {
        const cleaned = { ...next, links: next.links.map(link => link.trim()).filter(link => link) };
        if (!sameRequest(cleaned, saved)) actions.saveProfile(cleaned);
    };

    const setLink = (index: number, value: string) =>
        setDraft({ ...draft, links: draft.links.map((link, i) => (i === index ? value : link)) });

    return (
        <Disclosure title="Details" summary={profile.fullName || undefined}>
            <div className="profile-details">
                {detailFields.map(({ field, label, type, placeholder }) => (
                    <TextField
                        key={field}
                        label={label}
                        type={type}
                        placeholder={placeholder}
                        value={draft[field]}
                        onChange={value => setDraft({ ...draft, [field]: value })}
                        onBlur={() => save(draft)}
                    />
                ))}
                <div className="profile-details-links">
                    {draft.links.map((link, index) => (
                        <div key={index} className="profile-details-link">
                            <TextField
                                className="profile-details-link-field"
                                label={`Link ${index + 1}`}
                                placeholder="github.com/you"
                                value={link}
                                onChange={value => setLink(index, value)}
                                onBlur={() => save(draft)}
                            />
                            <IconButton
                                icon="trash"
                                label={`Remove link ${index + 1}`}
                                className="profile-details-link-remove"
                                onClick={() => save({ ...draft, links: draft.links.filter((_, i) => i !== index) })}
                            />
                        </div>
                    ))}
                    <Button variant="ghost" className="profile-details-add-link" onClick={() => setDraft({ ...draft, links: [...draft.links, ''] })}>
                        Add link
                    </Button>
                </div>
            </div>
        </Disclosure>
    );
};

export default ProfileDetails;
