export type IconName =
    | 'chevron-down'
    | 'check'
    | 'expand'
    | 'collapse'
    | 'plus'
    | 'close'
    | 'style'
    | 'content'
    | 'grip'
    | 'trash'
    | 'eye'
    | 'eye-off'
    | 'pin'
    | 'chevron-right'
    | 'tag';

export interface IconProps {
    name: IconName;
    className?: string;
}
