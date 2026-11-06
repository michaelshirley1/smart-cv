import axios from 'axios';

interface ApiError {
    message?: string;
    title?: string;
    errors?: Record<string, string[]>;
}

export function getErrorMessage(error: unknown, fallback: string): string {
    if (!axios.isAxiosError<ApiError>(error)) return fallback;

    const data = error.response?.data;
    if (data?.message) return data.message;

    const firstValidationError = data?.errors ? Object.values(data.errors).flat()[0] : undefined;
    return firstValidationError ?? data?.title ?? fallback;
}
