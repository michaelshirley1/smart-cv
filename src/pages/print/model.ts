import { ProfileDetail } from '../editor/model';

export interface PrintPageProps {}

export interface PrintJob {
    id: string;
    profile: ProfileDetail;
    sectionIds: number[] | null;
    lineIds: number[] | null;
}
