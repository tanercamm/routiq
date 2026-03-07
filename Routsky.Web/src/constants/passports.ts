/** Canonical list of supported citizenship countries with Unicode flag emojis. */
export const PASSPORT_CODES: { code: string; flag: string; label: string }[] = [
    { code: 'AU', flag: '🇦🇺', label: '🇦🇺 Australia' },
    { code: 'BR', flag: '🇧🇷', label: '🇧🇷 Brazil' },
    { code: 'CA', flag: '🇨🇦', label: '🇨🇦 Canada' },
    { code: 'CN', flag: '🇨🇳', label: '🇨🇳 China' },
    { code: 'EG', flag: '🇪🇬', label: '🇪🇬 Egypt' },
    { code: 'FR', flag: '🇫🇷', label: '🇫🇷 France' },
    { code: 'DE', flag: '🇩🇪', label: '🇩🇪 Germany' },
    { code: 'IN', flag: '🇮🇳', label: '🇮🇳 India' },
    { code: 'ID', flag: '🇮🇩', label: '🇮🇩 Indonesia' },
    { code: 'JP', flag: '🇯🇵', label: '🇯🇵 Japan' },
    { code: 'MX', flag: '🇲🇽', label: '🇲🇽 Mexico' },
    { code: 'NG', flag: '🇳🇬', label: '🇳🇬 Nigeria' },
    { code: 'PK', flag: '🇵🇰', label: '🇵🇰 Pakistan' },
    { code: 'PH', flag: '🇵🇭', label: '🇵🇭 Philippines' },
    { code: 'RU', flag: '🇷🇺', label: '🇷🇺 Russia' },
    { code: 'ZA', flag: '🇿🇦', label: '🇿🇦 South Africa' },
    { code: 'KR', flag: '🇰🇷', label: '🇰🇷 South Korea' },
    { code: 'TR', flag: '🇹🇷', label: '🇹🇷 Turkey' },
    { code: 'GB', flag: '🇬🇧', label: '🇬🇧 United Kingdom' },
    { code: 'US', flag: '🇺🇸', label: '🇺🇸 United States' },
];

/** Returns the flag emoji + country name for a given ISO code, e.g. "🇹🇷 Turkey". Falls back to the raw code. */
export function flagLabel(code: string): string {
    return PASSPORT_CODES.find(p => p.code === code)?.label ?? code;
}

/** Returns just the flag emoji for a given ISO code, e.g. "🇹🇷". Falls back to the raw code. */
export function flagEmoji(code: string): string {
    return PASSPORT_CODES.find(p => p.code === code)?.flag ?? code;
}

/**
 * Pure arithmetic flag emoji derivation — works for ANY valid ISO 3166-1 alpha-2 code,
 * even ones not in our PASSPORT_CODES list.
 * e.g. getFlagEmoji('TR') → '🇹🇷'
 */
export const getFlagEmoji = (countryCode: string): string => {
    if (!countryCode) return '';
    return countryCode.toUpperCase().replace(/./g, char =>
        String.fromCodePoint(char.charCodeAt(0) + 127397)
    );
};
