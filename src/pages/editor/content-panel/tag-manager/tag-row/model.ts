import { Tag } from '../../../model';

export interface TagRowProps {
    tag: Tag;
}

export const usageLabel = (count: number) => `${count} use${count === 1 ? '' : 's'}`;

export const placeLabel = (count: number) => `${count} place${count === 1 ? '' : 's'}`;

export const parseKeywords = (value: string) =>
    value.split(',').map(keyword => keyword.trim()).filter(keyword => keyword);
