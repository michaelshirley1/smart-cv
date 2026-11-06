import { Entry, EntryRequest, Tag } from '../../model';
import { TagOption } from '../../../../components/tag-input/model';
import { ReorderHandleProps } from '../../../../hooks/useReorder';

export interface EntryCardProps {
    entry: Entry;
    showHeader: boolean;
    tagOptions: TagOption[];
    alwaysOn?: Tag;
    handleProps?: ReorderHandleProps;
}

export const entryFields: { field: keyof EntryRequest; placeholder: string }[] = [
    { field: 'heading', placeholder: 'Organisation' },
    { field: 'dateRange', placeholder: 'Dates' },
    { field: 'subheading', placeholder: 'Role' },
    { field: 'location', placeholder: 'Location' },
];
