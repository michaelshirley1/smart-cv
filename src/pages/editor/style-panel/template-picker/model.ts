import { TemplateEditor } from '../template-editor';

export interface TemplatePickerProps {
    editor: TemplateEditor;
}

export type TemplateDialogKind = 'save-as' | 'rename' | 'delete' | null;
