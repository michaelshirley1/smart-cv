import React from 'react';
import { pageSizes, StylePanelProps, styleGroups } from './model';
import { useProfileContext } from '../profile-context';
import { useTemplateEditor } from './template-editor';
import { useContentActions } from '../content-panel/actions';
import { toProfileRequest } from '../api';
import TemplatePicker from './template-picker';
import StyleGroup from './style-group';
import SectionOrder from './section-order';
import Disclosure from '../../../components/disclosure';
import NumberField from '../../../components/number-field';
import Select from '../../../components/select';

import './styles.scss';

const StylePanel: React.FC<StylePanelProps> = () => {
    const { profile } = useProfileContext();
    const editor = useTemplateEditor();
    const actions = useContentActions();
    if (!profile) return null;

    const { values } = editor;
    const pageSize = pageSizes.find(size => size.width === values?.['page-width'] && size.height === values?.['page-height']);

    const setPageSize = (value: string) => {
        const size = pageSizes.find(option => option.value === value);
        if (size) editor.change({ 'page-width': size.width, 'page-height': size.height });
    };

    return (
        <div className="style-panel">
            <Disclosure title="Template" summary={profile.templateName} defaultOpen>
                <TemplatePicker editor={editor} />
            </Disclosure>

            <Disclosure title="Page" summary={pageSize?.label}>
                <div className="style-panel-grid">
                    {values && (
                        <Select
                            label="Page size"
                            value={pageSize?.value ?? 'custom'}
                            options={pageSizes.map(size => ({ value: size.value, label: size.label }))}
                            onChange={setPageSize}
                        />
                    )}
                    <NumberField
                        label="Target pages"
                        value={profile.targetPages}
                        min={1}
                        max={5}
                        onChange={targetPages => actions.saveProfile({ ...toProfileRequest(profile), targetPages })}
                    />
                </div>
            </Disclosure>

            {values
                ? styleGroups.map(group => (
                    <StyleGroup
                        key={group.title}
                        definition={group}
                        values={values}
                        fonts={editor.fonts}
                        onChange={(key, value) => editor.change({ [key]: value })}
                    />
                ))
                : <p className="style-panel-loading">Loading template…</p>}

            <SectionOrder profile={profile} />
        </div>
    );
};

export default StylePanel;
