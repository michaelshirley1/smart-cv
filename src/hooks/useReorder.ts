import React, { useState } from 'react';

export type DropPosition = 'before' | 'after';

interface DropTarget {
    id: number;
    position: DropPosition;
}

const positionFor = (e: React.DragEvent<HTMLElement>): DropPosition => {
    const rect = e.currentTarget.getBoundingClientRect();
    return e.clientY < rect.top + rect.height / 2 ? 'before' : 'after';
};

export function useReorder(scope: string, ids: number[], onReorder: (ids: number[]) => void) {
    const type = `application/x-smart-cv-${scope}`;
    const [dragging, setDragging] = useState<number | null>(null);
    const [target, setTarget] = useState<DropTarget | null>(null);

    const commit = (next: number[]) => {
        if (next.join(',') !== ids.join(',')) onReorder(next);
    };

    const moveTo = (id: number, targetId: number, position: DropPosition) => {
        const remaining = ids.filter(current => current !== id);
        const index = remaining.indexOf(targetId) + (position === 'after' ? 1 : 0);
        commit([...remaining.slice(0, index), id, ...remaining.slice(index)]);
    };

    const clear = () => {
        setDragging(null);
        setTarget(null);
    };

    const handleProps = (id: number) => ({
        draggable: true,
        onDragStart: (e: React.DragEvent<HTMLElement>) => {
            e.stopPropagation();
            e.dataTransfer.setData(type, String(id));
            e.dataTransfer.effectAllowed = 'move';
            const item = e.currentTarget.closest('[data-reorder-item]');
            if (item) e.dataTransfer.setDragImage(item, 12, 12);
            setDragging(id);
        },
        onDragEnd: clear,
        onKeyDown: (e: React.KeyboardEvent<HTMLElement>) => {
            if (!e.altKey) return;
            const index = ids.indexOf(id);
            if (e.key === 'ArrowUp' && index > 0) {
                e.preventDefault();
                moveTo(id, ids[index - 1], 'before');
            } else if (e.key === 'ArrowDown' && index < ids.length - 1) {
                e.preventDefault();
                moveTo(id, ids[index + 1], 'after');
            }
        },
    });

    const itemProps = (id: number) => ({
        'data-reorder-item': true,
        onDragOver: (e: React.DragEvent<HTMLElement>) => {
            if (!e.dataTransfer.types.includes(type)) return;
            e.preventDefault();
            e.stopPropagation();
            const position = positionFor(e);
            if (target?.id !== id || target.position !== position) setTarget({ id, position });
        },
        onDrop: (e: React.DragEvent<HTMLElement>) => {
            if (!e.dataTransfer.types.includes(type)) return;
            e.preventDefault();
            e.stopPropagation();
            const draggedId = Number(e.dataTransfer.getData(type));
            const position = positionFor(e);
            clear();
            if (draggedId !== id && ids.includes(draggedId)) moveTo(draggedId, id, position);
        },
    });

    const dropPosition = (id: number): DropPosition | null =>
        target?.id === id && dragging !== id ? target.position : null;

    return { handleProps, itemProps, dropPosition, dragging };
}

export type ReorderHandleProps = ReturnType<ReturnType<typeof useReorder>['handleProps']>;
