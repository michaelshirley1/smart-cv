import React, { useState } from 'react';
import { AddSectionProps } from './model';
import { SectionKind } from '../../model';
import { sectionKindOptions } from '../model';
import TextField from '../../../../components/text-field';
import Select from '../../../../components/select';
import Button from '../../../../components/button';

import './styles.scss';

const AddSection: React.FC<AddSectionProps> = ({ onAdd }) => {
    const [title, setTitle] = useState('');
    const [kind, setKind] = useState<SectionKind>('experience');
    const [saving, setSaving] = useState(false);
    const trimmed = title.trim();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!trimmed || saving) return;
        setSaving(true);
        await onAdd(trimmed, kind);
        setTitle('');
        setSaving(false);
    };

    return (
        <form className="add-section" onSubmit={handleSubmit}>
            <TextField
                className="add-section-title"
                label="New section"
                placeholder="e.g. Work Experience"
                value={title}
                maxLength={100}
                onChange={setTitle}
            />
            <Select className="add-section-kind" label="Type" value={kind} options={sectionKindOptions} onChange={setKind} />
            <Button type="submit" variant="secondary" className="add-section-submit" disabled={!trimmed || saving}>Add</Button>
        </form>
    );
};

export default AddSection;
