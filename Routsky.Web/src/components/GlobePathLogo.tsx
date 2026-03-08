/** Minimalist Globe with a Path icon for Routsky branding */

interface GlobePathLogoProps {
    className?: string;
    size?: number;
}

export const GlobePathLogo = ({ className = '', size = 24 }: GlobePathLogoProps) => (
    <svg
        xmlns="http://www.w3.org/2000/svg"
        viewBox="0 0 24 24"
        width={size}
        height={size}
        fill="none"
        stroke="currentColor"
        strokeWidth="1.5"
        strokeLinecap="round"
        strokeLinejoin="round"
        className={className}
    >
        {/* Globe circle */}
        <circle cx="12" cy="12" r="9" />
        {/* Latitude lines */}
        <path d="M3 12h18" />
        <path d="M3 7.5a9 9 0 0 1 18 0" />
        <path d="M3 16.5a9 9 0 0 0 18 0" />
        {/* Path/route arc — journey across the globe */}
        <path
            d="M6 9 Q12 6 18 9 Q12 12 6 15"
            strokeWidth="2"
        />
    </svg>
);
