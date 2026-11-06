import { CvView } from '../model';
import { SegmentedOption } from '../../../components/segmented-control/model';

export interface TopBarProps {}

export const viewOptions: SegmentedOption<CvView>[] = [
    { value: 'all', label: 'All lines' },
    { value: 'tailored', label: 'Tailored' },
];
