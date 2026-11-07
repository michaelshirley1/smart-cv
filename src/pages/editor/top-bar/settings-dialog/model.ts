import { MatchingMode } from '../../model';
import { SegmentedOption } from '../../../../components/segmented-control/model';

export interface SettingsDialogProps {
    onClose: () => void;
}

export const matchingOptions: SegmentedOption<MatchingMode>[] = [
    { value: 'keyword', label: 'Keywords' },
    { value: 'ollama', label: 'Ollama' },
];

export const matchingHints: Record<MatchingMode, string> = {
    keyword: 'Matches your tags and keywords against the job description. Fast and predictable.',
    ollama: 'A local AI model reads the job description and scores each line by meaning. Nothing leaves your computer. If Ollama is unavailable, keyword matching is used.',
};

export const suggestedModel = 'llama3.2';
