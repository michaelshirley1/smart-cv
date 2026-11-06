import { ProfileDetail, ProfileRequest } from '../../model';

export interface CvHeaderProps {
    profile: ProfileDetail;
    showPlaceholders: boolean;
    onSave?: (request: ProfileRequest) => void;
}

export type ContactField = 'email' | 'phone' | 'location' | number;

export interface ContactItem {
    field: ContactField;
    value: string;
    placeholder: string;
    href?: string;
}
