import React, { useState } from 'react';
import { TagManagerProps } from './model';
import { useContentActions } from '../actions';
import TagRow from './tag-row';
import Disclosure from '../../../../components/disclosure';
import TextField from '../../../../components/text-field';
import Button from '../../../../components/button';

import './styles.scss';

const TagManager: React.FC<TagManagerProps> = ({ profile }) => {
    const actions = useContentActions();
    const [name, setName] = useState('');
    const trimmed = name.trim();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!trimmed) return;
        if (await actions.createTag(trimmed) !== null) setName('');
    };

    return (
        <Disclosure title="Tags" summary={String(profile.tags.length)}>
            <p className="tag-manager-hint">
                A tag matches a job description when its name or any of its keywords appear in it.
            </p>
            <ul className="tag-manager-list">
                {profile.tags.map(tag => <TagRow key={tag.id} tag={tag} />)}
            </ul>
            <form className="tag-manager-add" onSubmit={handleSubmit}>
                <TextField
                    className="tag-manager-add-name"
                    label="New tag"
                    placeholder="e.g. Frontend"
                    value={name}
                    maxLength={50}
                    onChange={setName}
                />
                <Button type="submit" variant="secondary" disabled={!trimmed}>Add</Button>
            </form>
        </Disclosure>
    );
};

export default TagManager;
