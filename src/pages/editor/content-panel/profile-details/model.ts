import { ProfileDetail, ProfileRequest } from '../../model';

export interface ProfileDetailsProps {
    profile: ProfileDetail;
}

export type DetailField = 'fullName' | 'headline' | 'email' | 'phone' | 'location';

export const detailFields: { field: DetailField; label: string; type: string; placeholder: string }[] = [
    { field: 'fullName', label: 'Full name', type: 'text', placeholder: 'Your Name' },
    { field: 'headline', label: 'Headline', type: 'text', placeholder: 'Optional, e.g. Full Stack Engineer' },
    { field: 'email', label: 'Email', type: 'email', placeholder: 'you@example.com' },
    { field: 'phone', label: 'Phone', type: 'tel', placeholder: '+00 000 000 000' },
    { field: 'location', label: 'Location', type: 'text', placeholder: 'Optional' },
];

export const sameRequest = (a: ProfileRequest, b: ProfileRequest) => JSON.stringify(a) === JSON.stringify(b);

export const mergeSaved = (draft: ProfileRequest, previous: ProfileRequest, saved: ProfileRequest): ProfileRequest => {
    const merged = { ...saved };
    for (const field of detailFields.map(detail => detail.field)) {
        if (draft[field] !== previous[field]) merged[field] = draft[field];
    }
    if (JSON.stringify(draft.links) !== JSON.stringify(previous.links)) merged.links = draft.links;
    return merged;
};
