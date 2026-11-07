import axios from 'axios';

interface ApiError {
    detail?: string;
    title?: string;
    errors?: Record<string, string[]>;
}

export function getErrorMessage(error: unknown, fallback: string): string {
    if (!axios.isAxiosError<ApiError | string>(error)) return fallback;

    const data = error.response?.data;
    if (typeof data === 'string') return data.trim() || fallback;
    if (data?.detail) return data.detail;

    const firstValidationError = data?.errors ? Object.values(data.errors).flat()[0] : undefined;
    return firstValidationError ?? data?.title ?? fallback;
}
