import { Entry, SectionKind } from '../../model';
import { CvEdits, CvSelection } from '../model';

export interface CvEntryProps {
    entry: Entry;
    kind: SectionKind;
    isPlaceholder: boolean;
    showPlaceholders: boolean;
    selection?: CvSelection;
    edits?: CvEdits;
}
