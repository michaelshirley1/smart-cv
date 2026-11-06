import React from 'react';
import { SectionOrderProps } from './model';
import { useContentActions } from '../../content-panel/actions';
import { useReorder } from '../../../../hooks/useReorder';
import Disclosure from '../../../../components/disclosure';
import DragHandle from '../../../../components/drag-handle';
import IconButton from '../../../../components/icon-button';

import './styles.scss';

const SectionOrder: React.FC<SectionOrderProps> = ({ profile }) => {
    const actions = useContentActions();
    const reorder = useReorder('section-order', profile.sections.map(section => section.id), actions.reorderSections);

    return (
        <Disclosure title="Section order" summary={String(profile.sections.length)}>
            {profile.sections.length === 0 ? (
                <p className="section-order-empty">Add sections in the Content panel.</p>
            ) : (
                <ul className="section-order">
                    {profile.sections.map(section => {
                        const drop = reorder.dropPosition(section.id);
                        return (
                            <li
                                key={section.id}
                                className={`section-order-item${section.isHidden ? ' section-order-item--hidden' : ''}${drop ? ` section-order-item--drop-${drop}` : ''}`}
                                {...reorder.itemProps(section.id)}
                            >
                                <DragHandle label={`Section ${section.title}`} {...reorder.handleProps(section.id)} />
                                <span className="section-order-title">{section.title}</span>
                                <IconButton
                                    icon={section.isHidden ? 'eye-off' : 'eye'}
                                    className="icon-button--small"
                                    active={section.isHidden}
                                    label={section.isHidden ? `Show ${section.title}` : `Hide ${section.title}`}
                                    onClick={() => actions.saveSection(section, { isHidden: !section.isHidden })}
                                />
                            </li>
                        );
                    })}
                </ul>
            )}
        </Disclosure>
    );
};

export default SectionOrder;
