import React from 'react';
import { ContactField, ContactItem, CvHeaderProps } from './model';
import { ProfileDetail } from '../../model';
import { placeholderContact } from '../lorem';
import { toProfileRequest } from '../../api';
import EditableText from '../../../../components/editable-text';

import './styles.scss';

const webAddress = (value: string) => (/^https?:\/\//i.test(value) ? value : `https://${value}`);

const contactItems = (profile: ProfileDetail, showPlaceholders: boolean): ContactItem[] => {
    const items: ContactItem[] = [
        { field: 'email', value: profile.email, placeholder: '', href: `mailto:${profile.email}` },
        { field: 'phone', value: profile.phone, placeholder: '' },
        { field: 'location', value: profile.location, placeholder: '' },
        ...profile.links.map((link, index): ContactItem => ({ field: index, value: link, placeholder: '', href: webAddress(link) })),
    ];
    const filled = items.filter(item => item.value);

    if (filled.length > 0 || !showPlaceholders) return filled;

    return [
        { field: 'email', value: '', placeholder: placeholderContact.email },
        { field: 'phone', value: '', placeholder: placeholderContact.phone },
        { field: 0, value: '', placeholder: placeholderContact.link },
    ];
};

const CvHeader: React.FC<CvHeaderProps> = ({ profile, showPlaceholders, onSave }) => {
    const saveName = onSave
        ? (fullName: string) => onSave({ ...toProfileRequest(profile), fullName })
        : undefined;

    const saveHeadline = onSave
        ? (headline: string) => onSave({ ...toProfileRequest(profile), headline })
        : undefined;

    const saveContact = (field: ContactField) => onSave
        ? (value: string) => {
            const request = toProfileRequest(profile);
            if (typeof field === 'number') {
                const links = [...request.links];
                links[field] = value;
                onSave({ ...request, links: links.filter(link => link) });
            } else {
                onSave({ ...request, [field]: value });
            }
        }
        : undefined;

    const showName = profile.fullName || showPlaceholders;

    return (
        <header className="cv-header">
            {showName && (
                <h1 className="cv-header-name">
                    <EditableText value={profile.fullName} placeholder={placeholderContact.name} onSave={saveName} />
                </h1>
            )}
            {profile.headline && (
                <p className="cv-header-headline">
                    <EditableText value={profile.headline} onSave={saveHeadline} />
                </p>
            )}
            <ul className="cv-header-contact">
                {contactItems(profile, showPlaceholders).map(item => (
                    <li key={item.field} className="cv-header-contact-item">
                        {item.href && !onSave ? (
                            <a className="cv-header-contact-link" href={item.href}>{item.value}</a>
                        ) : (
                            <EditableText
                                className={item.href ? 'cv-header-contact-link' : undefined}
                                value={item.value}
                                placeholder={item.placeholder}
                                onSave={saveContact(item.field)}
                            />
                        )}
                    </li>
                ))}
            </ul>
        </header>
    );
};

export default CvHeader;
