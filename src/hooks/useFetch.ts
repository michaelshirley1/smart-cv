import { useEffect, useState } from 'react';

export function useFetch<T>(fetchFn: () => Promise<T>, initialValue: T, errorMessage = 'Failed to load data.') {
    const [data, setData] = useState<T>(initialValue);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        fetchFn()
            .then(setData)
            .catch(() => setError(errorMessage))
            .finally(() => setLoading(false));
    }, []);

    return { data, setData, loading, error };
}
