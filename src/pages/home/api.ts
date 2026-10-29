import client from '../../api/client';
import { Item } from './model';

export async function getSavedCv(id: number): Promise<Item> {
    const { data } = await client.get<Item>(`/api/cv/${id}`);
    return data;
}
