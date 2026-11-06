export type IconName =
    | 'chevron-down'
    | 'check'
    | 'expand'
    | 'collapse'
    | 'plus'
    | 'close'
    | 'style'
    | 'content';

export interface IconProps {
    name: IconName;
    className?: string;
}
