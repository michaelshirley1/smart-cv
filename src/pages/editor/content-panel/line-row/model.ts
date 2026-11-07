import { Line, normalPriority, Tag } from '../../model';
import { TagOption } from '../../../../components/tag-input/model';
import { ReorderHandleProps } from '../../../../hooks/useReorder';
import { priorityLabel } from '../model';

export interface LineRowProps {
    line: Line;
    tagOptions: TagOption[];
    alwaysOn?: Tag;
    handleProps: ReorderHandleProps;
}

export const lineSummary = (tagCount: number, priority: number) => {
    const parts = [tagCount === 0 ? 'No tags' : `${tagCount} tag${tagCount === 1 ? '' : 's'}`];
    if (priority !== normalPriority) parts.push(`${priorityLabel(priority)} priority`);
    return parts.join(' · ');
};
