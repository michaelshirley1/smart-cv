import { Entry, Line, normalPriority, Section, SectionKind } from '../model';
import { CvLineVariant } from './model';

const sentences = [
    'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.',
    'Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.',
    'Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.',
    'Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.',
];

const skills = [
    '**Lorem:** ipsum, dolor, sit amet, consectetur, adipiscing',
    '**Elit:** sed do, eiusmod, tempor, incididunt, labore',
];

let nextId = -1;
const placeholderId = () => nextId--;

const line = (entryId: number, text: string, sortOrder: number): Line => ({
    id: placeholderId(),
    entryId,
    text,
    sortOrder,
    override: 'none',
    priority: normalPriority,
    tagIds: [],
});

const entry = (sectionId: number, heading: string, subheading: string, dateRange: string, texts: string[]): Entry => {
    const id = placeholderId();
    return {
        id,
        sectionId,
        heading,
        subheading,
        location: '',
        dateRange,
        sortOrder: 0,
        lines: texts.map((text, index) => line(id, text, index)),
    };
};

const section = (title: string, kind: SectionKind, build: (id: number) => Entry[]): Section => {
    const id = placeholderId();
    return {
        id,
        profileId: 0,
        title,
        kind,
        sortOrder: 0,
        maxLines: 5,
        maxTotal: 0,
        isHidden: false,
        priority: normalPriority,
        tagIds: [],
        entries: build(id),
    };
};

export const placeholderSections: Section[] = [
    section('Summary', 'summary', id => [entry(id, '', '', '', [sentences[0] + ' ' + sentences[1]])]),
    section('Work Experience', 'experience', id => [
        entry(id, 'Company Name', 'Job Title', 'Jan 2022 – Present', sentences.slice(0, 3)),
        entry(id, 'Previous Company', 'Job Title', 'Mar 2019 – Dec 2021', sentences.slice(1, 3)),
    ]),
    section('Skills', 'skills', id => [entry(id, '', '', '', skills)]),
    section('Education', 'education', id => [entry(id, 'University Name', 'Degree', 'Graduated 2018', [sentences[3]])]),
];

export const placeholderLines = (entryId: number, variant: CvLineVariant): Line[] => {
    const texts = variant === 'bullet' ? sentences.slice(0, 2) : variant === 'plain' ? skills.slice(0, 1) : sentences.slice(0, 1);
    return texts.map((text, index) => ({ ...line(entryId, text, index), id: -(entryId * 10 + index + 1) }));
};

export const placeholderContact = {
    name: 'Your Name',
    email: 'you@example.com',
    phone: '+00 000 000 000',
    link: 'linkedin.com/in/you',
};

export const placeholderEntry = {
    heading: 'Organisation',
    subheading: 'Role',
    dateRange: 'Start – End',
};
