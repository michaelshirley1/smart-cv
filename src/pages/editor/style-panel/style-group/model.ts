import { TemplateValues } from '../../model';
import { StyleGroupDefinition } from '../model';

export interface StyleGroupProps {
    definition: StyleGroupDefinition;
    values: TemplateValues;
    fonts: string[];
    onChange: (key: string, value: string) => void;
}
