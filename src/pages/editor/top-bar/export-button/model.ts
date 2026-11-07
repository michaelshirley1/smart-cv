export interface ExportButtonProps {}

export const fileName = (path: string) => path.split(/[\\/]/).pop() ?? path;
