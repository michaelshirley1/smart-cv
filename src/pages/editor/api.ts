import client from '../../api/client';
import { AppSettings } from './model';

export const getSettings = () =>
    client.get<AppSettings>('/settings').then(r => r.data);

export const updateSettings = (settings: AppSettings) =>
    client.put<AppSettings>('/settings', settings).then(r => r.data);
