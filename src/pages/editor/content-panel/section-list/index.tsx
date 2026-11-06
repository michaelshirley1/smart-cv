import React, { useState } from 'react';
import { SectionListProps } from './model';
import { SectionKind } from '../../model';
import { toTagOptions } from '../model';
import { useContentActions } from '../actions';
import { useReorder } from '../../../../hooks/useReorder';
import SectionCard from '../section-card';
import AddSection from '../add-section';

import './styles.scss';

const SectionList: React.FC<SectionListProps> = ({ profile }) => {
    const actions = useContentActions();
    const [expandedIds, setExpandedIds] = useState<number[]>([]);
    const ids = profile.sections.map(section => section.id);
    const reorder = useReorder('section', ids, actions.reorderSections);
    const tagOptions = toTagOptions(profile.tags);
    const alwaysOn = profile.tags.find(tag => tag.isAlwaysOn);

    const toggle = (id: number) =>
        setExpandedIds(current => (current.includes(id) ? current.filter(x => x !== id) : [...current, id]));

    const add = async (title: string, kind: SectionKind) => {
        const updated = await actions.createSection(title, kind);
        const created = updated?.sections.at(-1);
        if (created) setExpandedIds(current => [...current, created.id]);
    };

    return (
        <div className="section-list">
            <h3 className="section-list-title">Sections</h3>
            {profile.sections.length === 0 && (
                <p className="section-list-empty">The CV shows a sample until you add your first section.</p>
            )}
            <ul className="section-list-items">
                {profile.sections.map(section => {
                    const drop = reorder.dropPosition(section.id);
                    return (
                        <li
                            key={section.id}
                            className={`section-list-item${drop ? ` section-list-item--drop-${drop}` : ''}`}
                            {...reorder.itemProps(section.id)}
                        >
                            <SectionCard
                                section={section}
                                tagOptions={tagOptions}
                                alwaysOn={alwaysOn}
                                expanded={expandedIds.includes(section.id)}
                                onToggle={() => toggle(section.id)}
                                handleProps={reorder.handleProps(section.id)}
                            />
                        </li>
                    );
                })}
            </ul>
            <AddSection onAdd={add} />
        </div>
    );
};

export default SectionList;
