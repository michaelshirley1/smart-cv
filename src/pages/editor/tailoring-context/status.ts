import { Line, LineDecision, Section, TailorResult } from '../model';
import { LineStatus } from './model';

const lineLabels: Record<LineDecision, string | null> = {
    alwaysOn: 'Always on',
    matched: 'Matched',
    untagged: 'Included, untagged',
    fallback: 'Kept as the entry\'s best line',
    unmatched: 'Not relevant to this job',
    overLimit: 'Over the max lines',
    hidden: null,
    sectionExcluded: 'Section left out',
};

const scoredByModel: LineDecision[] = ['matched', 'unmatched', 'overLimit', 'fallback'];

export const lineStatusFor =(result: TailorResult, trimmedIds: number[], line: Line): LineStatus | null => {
    const decision = result.lines.find(candidate => candidate.lineId === line.id);
    if (!decision) return null;
    if (trimmedIds.includes(line.id)) return { label: 'Trimmed to fit the page', included: false };

    const label = lineLabels[decision.decision];
    if (label === null) return null;

    if (result.mode === 'ollama' && scoredByModel.includes(decision.decision)) {
        const text = decision.decision === 'matched' ? 'Relevant' : label;
        return { label: `${text} (${decision.score}/10)`, included: decision.included };
    }

    if (decision.decision !== 'matched') return { label, included: decision.included };

    const tags = result.matchedTags.filter(tag => line.tagIds.includes(tag.tagId)).map(tag => tag.name);
    return { label: `${label}: ${tags.join(', ')}`, included: true };
};

export const sectionStatusFor = (result: TailorResult, section: Section): LineStatus | null => {
    const decision = result.sections.find(candidate => candidate.sectionId === section.id);
    if (!decision) return null;
    if (decision.decision === 'unmatched') return { label: 'Left out', included: false };
    if (decision.decision === 'matched') return { label: 'Matched', included: true };
    return null;
};
