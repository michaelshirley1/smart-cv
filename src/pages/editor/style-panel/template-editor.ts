import { useEffect, useRef, useState } from 'react';
import { useProfileContext } from '../profile-context';
import { TemplateSummary, TemplateValues } from '../model';
import * as api from '../api';

const saveDelay = 400;

const uniqueName = (base: string, taken: TemplateSummary[]) => {
    const names = new Set(taken.map(template => template.name));
    let name = base;
    for (let suffix = 2; names.has(name); suffix++) name = `${base}-${suffix}`;
    return name;
};

export const useTemplateEditor = () => {
    const { profile, mutate, attempt, reloadProfile, refreshTemplate } = useProfileContext();
    const [templates, setTemplates] = useState<TemplateSummary[]>([]);
    const [values, setValues] = useState<TemplateValues | null>(null);
    const [fonts, setFonts] = useState<string[]>([]);
    const [notice, setNotice] = useState<string | null>(null);
    const copies = useRef(new Map<string, string>());
    const keepValuesFor = useRef<string | null>(null);
    const timer = useRef<number | null>(null);
    const queue = useRef<Promise<void>>(Promise.resolve());

    const templateName = profile?.templateName ?? null;
    const current = templates.find(template => template.name === templateName) ?? null;

    useEffect(() => {
        attempt(async () => setTemplates(await api.getTemplates()), 'Could not load the templates.');
        attempt(async () => setFonts(await api.getFonts()), 'Could not load your fonts.');
    }, []);

    useEffect(() => {
        if (!templateName) return;
        if (keepValuesFor.current === templateName) {
            keepValuesFor.current = null;
            return;
        }
        setValues(null);
        setNotice(null);
        attempt(async () => setValues((await api.getTemplate(templateName)).values), 'Could not load the template.');
    }, [templateName]);

    const switchTemplate = async (name: string) => {
        if (!profile || name === profile.templateName) return;
        await mutate(() => api.updateProfile(profile.id, { ...api.toProfileRequest(profile), templateName: name }), 'Could not switch template.');
    };

    const persist = (name: string, next: TemplateValues) => {
        queue.current = queue.current.then(() => attempt(async () => {
            const destination = copies.current.get(name) ?? name;
            const all = await api.getTemplates();
            const isBuiltIn = all.find(template => template.name === destination)?.isBuiltIn ?? false;

            if (isBuiltIn) {
                const created = await api.createTemplate({ name: uniqueName(`${destination}-custom`, all), values: next });
                copies.current.set(name, created.name);
                keepValuesFor.current = created.name;
                setTemplates(await api.getTemplates());
                await switchTemplate(created.name);
                setNotice(`${destination} is built in, so your changes were saved as a new template, ${created.name}.`);
            } else {
                await api.updateTemplate(destination, { name: destination, values: next });
            }
            refreshTemplate();
        }, 'Could not save the style change.'));
    };

    const change = (changes: TemplateValues) => {
        if (!values || !templateName) return;
        const next = { ...values, ...changes };
        if (JSON.stringify(next) === JSON.stringify(values)) return;
        setValues(next);
        if (timer.current !== null) window.clearTimeout(timer.current);
        const name = templateName;
        timer.current = window.setTimeout(() => {
            timer.current = null;
            persist(name, next);
        }, saveDelay);
    };

    const pickTemplate = (name: string) => {
        copies.current.clear();
        return switchTemplate(name);
    };

    const saveAs = (name: string) => attempt(async () => {
        if (!values) return;
        const created = await api.createTemplate({ name, values });
        setTemplates(await api.getTemplates());
        await pickTemplate(created.name);
        refreshTemplate();
    }, 'Could not save the template.');

    const rename = (name: string) => attempt(async () => {
        if (!values || !current || current.isBuiltIn) return;
        await api.updateTemplate(current.name, { name, values });
        setTemplates(await api.getTemplates());
        await reloadProfile();
        refreshTemplate();
    }, 'Could not rename the template.');

    const remove = () => attempt(async () => {
        if (!current || current.isBuiltIn) return;
        await api.deleteTemplate(current.name);
        copies.current.clear();
        setTemplates(await api.getTemplates());
        await reloadProfile();
        refreshTemplate();
    }, 'Could not delete the template.');

    return { templates, current, values, fonts, notice, change, pickTemplate, saveAs, rename, remove };
};

export type TemplateEditor = ReturnType<typeof useTemplateEditor>;
