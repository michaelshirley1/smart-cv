import React, { useState } from 'react';
import { TemplateDialogKind, TemplatePickerProps } from './model';
import Icon from '../../../../components/icon';
import Button from '../../../../components/button';
import NameDialog from '../../../../components/name-dialog';
import ConfirmDialog from '../../../../components/confirm-dialog';

import './styles.scss';

const TemplatePicker: React.FC<TemplatePickerProps> = ({ editor }) => {
    const { templates, current, notice, pickTemplate, saveAs, rename, remove } = editor;
    const [dialog, setDialog] = useState<TemplateDialogKind>(null);
    const close = () => setDialog(null);

    return (
        <div className="template-picker">
            <ul className="template-picker-list" role="radiogroup" aria-label="Template">
                {templates.map(template => {
                    const active = template.name === current?.name;
                    return (
                        <li key={template.name}>
                            <button
                                type="button"
                                role="radio"
                                aria-checked={active}
                                className={`template-picker-option${active ? ' template-picker-option--active' : ''}`}
                                onClick={() => pickTemplate(template.name)}
                            >
                                <span className="template-picker-name">{template.name}</span>
                                {template.isBuiltIn && <span className="template-picker-badge">Built-in</span>}
                                {active && <Icon name="check" className="template-picker-check" />}
                            </button>
                        </li>
                    );
                })}
            </ul>

            {notice && <p className="template-picker-notice">{notice}</p>}
            {!notice && current?.isBuiltIn && (
                <p className="template-picker-hint">Built-in templates stay unchanged. Your first style change saves a copy.</p>
            )}

            <div className="template-picker-actions">
                <Button variant="secondary" onClick={() => setDialog('save-as')}>Save as new</Button>
                {current && !current.isBuiltIn && (
                    <>
                        <Button variant="ghost" onClick={() => setDialog('rename')}>Rename</Button>
                        <Button variant="ghost" className="template-picker-delete" onClick={() => setDialog('delete')}>Delete</Button>
                    </>
                )}
            </div>

            {dialog === 'save-as' && (
                <NameDialog
                    title="Save as new template"
                    label="Template name"
                    placeholder="e.g. Tokyo applications"
                    confirmLabel="Save"
                    initialName=""
                    maxLength={40}
                    onSubmit={saveAs}
                    onClose={close}
                />
            )}
            {dialog === 'rename' && current && (
                <NameDialog
                    title="Rename template"
                    label="Template name"
                    confirmLabel="Rename"
                    initialName={current.name}
                    maxLength={40}
                    onSubmit={rename}
                    onClose={close}
                />
            )}
            {dialog === 'delete' && current && (
                <ConfirmDialog
                    title="Delete template"
                    message={<>Delete <strong>{current.name}</strong>? Profiles using it switch back to the default template.</>}
                    confirmLabel="Delete"
                    onConfirm={remove}
                    onClose={close}
                />
            )}
        </div>
    );
};

export default TemplatePicker;
