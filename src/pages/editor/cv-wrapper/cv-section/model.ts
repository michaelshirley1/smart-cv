import { Section } from '../../model';
import { CvEdits } from '../model';

export interface CvSectionProps {
    section: Section;
    isPlaceholder: boolean;
    showPlaceholders: boolean;
    edits?: CvEdits;
}
