import { SectionKind, Tag, TagColour } from '../model';
import { SelectOption } from '../../../components/select/model';
import { TagOption } from '../../../components/tag-input/model';

export interface ContentPanelProps {}

export const sectionKindOptions: SelectOption<SectionKind>[] = [
    { value: 'summary', label: 'Summary' },
    { value: 'experience', label: 'Experience' },
    { value: 'education', label: 'Education' },
    { value: 'projects', label: 'Projects' },
    { value: 'skills', label: 'Skills' },
    { value: 'custom', label: 'List' },
];

export const priorityOptions: SelectOption<string>[] = [
    { value: '5', label: 'Highest' },
    { value: '4', label: 'High' },
    { value: '3', label: 'Normal' },
    { value: '2', label: 'Low' },
    { value: '1', label: 'Lowest' },
];

export const priorityLabel = (priority: number) =>
    priorityOptions.find(option => option.value === String(priority))?.label ?? 'Normal';

export const tagColours: TagColour[] = ['', 'blue', 'green', 'amber', 'red', 'purple', 'teal', 'pink'];

export const colourClass = (colour: string) => colour || 'default';

export const toTagOptions = (tags: Tag[]): TagOption[] =>
    tags.filter(tag => !tag.isAlwaysOn).map(tag => ({ id: tag.id, name: tag.name, colour: tag.colour }));

export const withAlwaysOn = (tagIds: number[], alwaysOnId: number | undefined, on: boolean) => {
    const others = tagIds.filter(id => id !== alwaysOnId);
    return on && alwaysOnId !== undefined ? [...others, alwaysOnId] : others;
};
