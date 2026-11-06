import { SectionKind } from '../../model';

export interface AddSectionProps {
    onAdd: (title: string, kind: SectionKind) => Promise<void>;
}
