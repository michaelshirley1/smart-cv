import { useProfileContext } from '../profile-context';
import { Entry, EntryRequest, Line, LineRequest, normalPriority, ProfileDetail, ProfileRequest, Section, SectionKind, SectionRequest, Tag, TagRequest } from '../model';
import * as api from '../api';

export const useContentActions = () => {
    const { profile, mutate } = useProfileContext();
    const profileId = profile?.id ?? 0;

    return {
        saveProfile: (request: ProfileRequest) =>
            mutate(() => api.updateProfile(profileId, request), 'Could not save your details.'),

        createSection: (title: string, kind: SectionKind) =>
            mutate(() => api.createSection({ profileId, title, kind, maxLines: 5, isHidden: false, priority: normalPriority }), 'Could not add the section.'),
        saveSection: (section: Section, changes: Partial<SectionRequest>) =>
            mutate(() => api.updateSection(section.id, { ...api.toSectionRequest(section), ...changes }), 'Could not save the section.'),
        deleteSection: (id: number) =>
            mutate(() => api.deleteSection(id), 'Could not delete the section.'),
        reorderSections: (ids: number[]) =>
            mutate(() => api.reorderSections(profileId, ids), 'Could not reorder the sections.'),
        setSectionTags: (id: number, tagIds: number[]) =>
            mutate(() => api.setSectionTags(id, tagIds), 'Could not save the section tags.'),

        createEntry: (sectionId: number) =>
            mutate(() => api.createEntry({ sectionId, heading: '', subheading: '', location: '', dateRange: '' }), 'Could not add the entry.'),
        saveEntry: (entry: Entry, changes: Partial<EntryRequest>) =>
            mutate(() => api.updateEntry(entry.id, { ...api.toEntryRequest(entry), ...changes }), 'Could not save the entry.'),
        deleteEntry: (id: number) =>
            mutate(() => api.deleteEntry(id), 'Could not delete the entry.'),
        reorderEntries: (sectionId: number, ids: number[]) =>
            mutate(() => api.reorderEntries(sectionId, ids), 'Could not reorder the entries.'),

        createLine: (entryId: number, text: string) =>
            mutate(() => api.createLine({ entryId, text, override: 'none', priority: normalPriority, tagIds: [] }), 'Could not add the line.'),
        saveLine: (line: Line, changes: Partial<LineRequest>) =>
            mutate(() => api.updateLine(line.id, { ...api.toLineRequest(line), ...changes }), 'Could not save the line.'),
        deleteLine: (id: number) =>
            mutate(() => api.deleteLine(id), 'Could not delete the line.'),
        reorderLines: (entryId: number, ids: number[]) =>
            mutate(() => api.reorderLines(entryId, ids), 'Could not reorder the lines.'),
        setLineTags: (id: number, tagIds: number[]) =>
            mutate(() => api.setLineTags(id, tagIds), 'Could not save the line tags.'),

        createTag: async (name: string): Promise<number | null> => {
            const updated: ProfileDetail | null = await mutate(
                () => api.createTag({ profileId, name, keywords: [], colour: '' }),
                'Could not create the tag.',
            );
            return updated?.tags.find(tag => tag.name.toLowerCase() === name.toLowerCase())?.id ?? null;
        },
        saveTag: (tag: Tag, changes: Partial<TagRequest>) =>
            mutate(() => api.updateTag(tag.id, { ...api.toTagRequest(tag), ...changes }), 'Could not save the tag.'),
        deleteTag: (id: number) =>
            mutate(() => api.deleteTag(id), 'Could not delete the tag.'),
    };
};

export type ContentActions = ReturnType<typeof useContentActions>;
