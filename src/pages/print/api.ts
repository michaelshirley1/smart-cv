import client from '../../api/client';
import { PrintJob } from './model';

export const getPrintJob = (id: string) =>
    client.get<PrintJob>(`/export/job/${encodeURIComponent(id)}`).then(r => r.data);
