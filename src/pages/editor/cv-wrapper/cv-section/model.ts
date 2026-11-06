import { Section } from '../../model';
import { CvEdits, CvSelection } from '../model';

export interface CvSectionProps {
    section: Section;
    isPlaceholder: boolean;
    showPlaceholders: boolean;
    selection?: CvSelection;
    edits?: CvEdits;
}
