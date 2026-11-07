import { Tag } from '../../model';

export interface TagSelectorProps {}

export const selectionSummary = (tagIds: number[], tags: Tag[]) => {
    const names = tagIds.map(id => tags.find(tag => tag.id === id)?.name).filter((name): name is string => !!name);
    if (names.length === 0) return 'None';
    return names.length <= 2 ? names.join(' › ') : `${names.slice(0, 2).join(' › ')} +${names.length - 2}`;
};
