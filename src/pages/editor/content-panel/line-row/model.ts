import { Line, Tag } from '../../model';
import { TagOption } from '../../../../components/tag-input/model';
import { ReorderHandleProps } from '../../../../hooks/useReorder';

export interface LineRowProps {
    line: Line;
    tagOptions: TagOption[];
    alwaysOn?: Tag;
    handleProps: ReorderHandleProps;
}
