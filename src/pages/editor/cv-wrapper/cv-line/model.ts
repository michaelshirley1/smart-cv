import { Line, LineRequest } from '../../model';
import { CvLineVariant } from '../model';

export interface CvLineProps {
    line: Line;
    variant: CvLineVariant;
    isPlaceholder: boolean;
    onSave?: (id: number, request: LineRequest) => void;
}
