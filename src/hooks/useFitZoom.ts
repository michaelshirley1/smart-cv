import { RefObject, useLayoutEffect, useState } from 'react';

const minZoom = 30;
const maxZoom = 100;

export function useFitZoom(container: RefObject<HTMLElement | null>, content: RefObject<HTMLElement | null>) {
    const [zoom, setZoom] = useState(maxZoom);

    useLayoutEffect(() => {
        const containerNode = container.current;
        const contentNode = content.current;
        if (!containerNode || !contentNode) return;

        const update = () => {
            const styles = getComputedStyle(containerNode);
            const available = containerNode.clientWidth - parseFloat(styles.paddingLeft) - parseFloat(styles.paddingRight);
            const natural = contentNode.offsetWidth;
            if (natural <= 0) return;
            setZoom(Math.max(minZoom, Math.min(maxZoom, Math.floor((available / natural) * 100))));
        };

        const observer = new ResizeObserver(update);
        observer.observe(containerNode);
        observer.observe(contentNode);
        update();

        return () => observer.disconnect();
    }, []);

    return zoom;
}
