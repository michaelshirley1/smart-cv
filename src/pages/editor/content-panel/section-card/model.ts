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

export const limitText = (value: number) => (value > 0 ? String(value) : '');

export const parseLimit = (text: string, max: number): number | null => {
    if (text.trim() === '') return 0;
    const value = Number.parseInt(text, 10);
    return Number.isInteger(value) && value >= 1 && value <= max ? value : null;
};

export const lineCountLabel = (section: Section) => {
    const count = section.entries.reduce((total, entry) => total + entry.lines.length, 0);
    return `${count} line${count === 1 ? '' : 's'}`;
};
