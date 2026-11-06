import { Section, Tag } from '../../model';
import { TagOption } from '../../../../components/tag-input/model';
import { ReorderHandleProps } from '../../../../hooks/useReorder';

export interface SectionCardProps {
    section: Section;
    tagOptions: TagOption[];
    alwaysOn?: Tag;
    expanded: boolean;
    onToggle: () => void;
    handleProps: ReorderHandleProps;
}

export const lineCountLabel = (section: Section) => {
    const count = section.entries.reduce((total, entry) => total + entry.lines.length, 0);
    return `${count} line${count === 1 ? '' : 's'}`;
};
