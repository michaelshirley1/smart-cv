import { SelectOption } from '../../../components/select/model';

export interface StylePanelProps {}

export type StyleControl =
    | { kind: 'length'; key: string; label: string; max?: number }
    | { kind: 'number'; key: string; label: string; min: number; max: number; step: number }
    | { kind: 'colour'; key: string; label: string }
    | { kind: 'font'; key: string; label: string }
    | { kind: 'choice'; key: string; label: string; options: SelectOption<string>[] };

export interface StyleGroupDefinition {
    title: string;
    controls: StyleControl[];
}

export interface PageSize {
    value: string;
    label: string;
    width: string;
    height: string;
}

export const lengthOptions: SelectOption<string>[] = [
    { value: '0', label: 'No limit' },
    { value: '1', label: '1 page' },
    { value: '2', label: '2 pages' },
    { value: '3', label: '3 pages' },
    { value: '4', label: '4 pages' },
    { value: '5', label: '5 pages' },
];

export const pageSizes: PageSize[] = [
    { value: 'letter', label: 'US Letter', width: '8.5in', height: '11in' },
    { value: 'a4', label: 'A4', width: '210mm', height: '297mm' },
];

const weights = [
    { value: '400', label: 'Regular' },
    { value: '600', label: 'Semibold' },
    { value: '700', label: 'Bold' },
];

const rules = [
    { value: 'none', label: 'None' },
    { value: '0.5pt solid currentColor', label: 'Hairline' },
    { value: '0.75pt solid currentColor', label: 'Thin' },
    { value: '1pt solid currentColor', label: 'Medium' },
    { value: '1.5pt solid currentColor', label: 'Thick' },
];

export const styleGroups: StyleGroupDefinition[] = [
    {
        title: 'Fonts',
        controls: [
            { kind: 'font', key: 'body-font', label: 'Body font' },
            { kind: 'font', key: 'heading-font', label: 'Heading font' },
            { kind: 'length', key: 'font-size', label: 'Body size', max: 30 },
            { kind: 'number', key: 'line-height', label: 'Line height', min: 0.8, max: 3, step: 0.05 },
            { kind: 'length', key: 'name-size', label: 'Name size', max: 72 },
            { kind: 'choice', key: 'name-weight', label: 'Name weight', options: weights },
            { kind: 'length', key: 'heading-size', label: 'Heading size', max: 40 },
            { kind: 'choice', key: 'heading-weight', label: 'Heading weight', options: weights },
            { kind: 'length', key: 'contact-size', label: 'Contact size', max: 30 },
            { kind: 'choice', key: 'entry-heading-weight', label: 'Organisation weight', options: weights },
        ],
    },
    {
        title: 'Colours',
        controls: [
            { kind: 'colour', key: 'text-colour', label: 'Text' },
            { kind: 'colour', key: 'accent-colour', label: 'Headings' },
            { kind: 'colour', key: 'muted-colour', label: 'Muted' },
            { kind: 'colour', key: 'link-colour', label: 'Links' },
            {
                kind: 'choice',
                key: 'link-decoration',
                label: 'Link style',
                options: [{ value: 'underline', label: 'Underlined' }, { value: 'none', label: 'Plain' }],
            },
        ],
    },
    {
        title: 'Layout',
        controls: [
            {
                kind: 'choice',
                key: 'name-align',
                label: 'Header alignment',
                options: [{ value: 'left', label: 'Left' }, { value: 'center', label: 'Centre' }, { value: 'right', label: 'Right' }],
            },
            {
                kind: 'choice',
                key: 'contact-separator',
                label: 'Contact separator',
                options: [
                    { value: '"\\2756"', label: '❖ Diamond' },
                    { value: '"\\2022"', label: '• Dot' },
                    { value: '"\\00B7"', label: '· Middle dot' },
                    { value: '"|"', label: '| Bar' },
                    { value: '"/"', label: '/ Slash' },
                ],
            },
            { kind: 'choice', key: 'header-rule', label: 'Rule under header', options: rules },
            { kind: 'choice', key: 'heading-rule', label: 'Rule under headings', options: rules },
            {
                kind: 'choice',
                key: 'heading-case',
                label: 'Heading case',
                options: [{ value: 'uppercase', label: 'UPPERCASE' }, { value: 'none', label: 'As typed' }, { value: 'capitalize', label: 'Title Case' }],
            },
            {
                kind: 'choice',
                key: 'summary-title-display',
                label: 'Summary heading',
                options: [{ value: 'none', label: 'Hidden' }, { value: 'block', label: 'Shown' }],
            },
            {
                kind: 'choice',
                key: 'entry-subheading-style',
                label: 'Role style',
                options: [{ value: 'normal', label: 'Normal' }, { value: 'italic', label: 'Italic' }],
            },
            {
                kind: 'choice',
                key: 'bullet',
                label: 'Bullet',
                options: [
                    { value: '"\\25AA"', label: '▪ Square' },
                    { value: '"\\2022"', label: '• Dot' },
                    { value: '"\\2013"', label: '– Dash' },
                    { value: '"\\203A"', label: '› Arrow' },
                    { value: '""', label: 'None' },
                ],
            },
        ],
    },
    {
        title: 'Spacing',
        controls: [
            { kind: 'length', key: 'page-margin-top', label: 'Top margin' },
            { kind: 'length', key: 'page-margin-bottom', label: 'Bottom margin' },
            { kind: 'length', key: 'page-margin-left', label: 'Left margin' },
            { kind: 'length', key: 'page-margin-right', label: 'Right margin' },
            { kind: 'length', key: 'section-spacing', label: 'Between sections' },
            { kind: 'length', key: 'entry-spacing', label: 'Between entries' },
            { kind: 'length', key: 'bullet-indent', label: 'Bullet indent' },
        ],
    },
];
