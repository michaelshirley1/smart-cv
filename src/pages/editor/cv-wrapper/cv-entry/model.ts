import { Entry, SectionKind } from '../../model';
import { CvEdits } from '../model';

export interface CvEntryProps {
    entry: Entry;
    kind: SectionKind;
    isPlaceholder: boolean;
    showPlaceholders: boolean;
    edits?: CvEdits;
}
