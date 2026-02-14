import type { DestinationTip } from '../types';

/**
 * Convert a 2-letter ISO country code to an emoji flag.
 * Works by converting each letter to its regional indicator symbol.
 * Example: "TR" → "🇹🇷", "US" → "🇺🇸", "JP" → "🇯🇵"
 */
export function countryCodeToFlag(code: string): string {
    if (!code || code.length !== 2) return '🌍';
    return String.fromCodePoint(
        ...code.toUpperCase().split('').map(c => 0x1F1E6 + c.charCodeAt(0) - 65)
    );
}

/** Alias – matches the user-facing spec name */
export const getFlagEmoji = countryCodeToFlag;

// Mock community tips — mirrors what DestinationTip entities would return.
// Will be replaced by API calls when community endpoints are built.

const communityTips: Record<string, DestinationTip[]> = {
    Belgrade: [
        { username: 'WanderSarah', countryCode: 'US', content: 'Don\'t miss the sunset from Kalemegdan Fortress — it\'s one of the most beautiful in Europe. Bring a blanket and some street food from nearby vendors!', upvotes: 34, createdAt: '2026-01-15' },
        { username: 'NomadKai', countryCode: 'DE', content: 'Skadarlija street is the bohemian quarter — try ćevapi at a local kafana. Way cheaper and tastier than the tourist spots.', upvotes: 28, createdAt: '2026-01-10' },
        { username: 'ExplorerMax', countryCode: 'GB', content: 'The Nikola Tesla Museum is small but absolutely fascinating. Book tickets online to skip the line!', upvotes: 21, createdAt: '2025-12-20' },
    ],
    Tirana: [
        { username: 'GlobeAnya', countryCode: 'PL', content: 'Bunk\'Art 1 is a must-see — a massive Cold War bunker turned into an art museum. Allow 2+ hours.', upvotes: 42, createdAt: '2026-01-08' },
        { username: 'TrailBlazerJay', countryCode: 'CA', content: 'Take the Dajti Express cable car for amazing views over Tirana. Best in late afternoon light.', upvotes: 31, createdAt: '2025-12-28' },
    ],
    Bangkok: [
        { username: 'NomadKai', countryCode: 'DE', content: 'Skip the tourist boats — use Chao Phraya Express for 15 baht! Same river, 1/10th the price.', upvotes: 56, createdAt: '2026-01-12' },
        { username: 'WanderSarah', countryCode: 'US', content: 'Chatuchak Weekend Market is overwhelming — go early (9 AM) and head to Section 7 for vintage finds.', upvotes: 43, createdAt: '2026-01-05' },
        { username: 'GlobeAnya', countryCode: 'PL', content: 'Wat Arun at golden hour is magical. Cross the ferry from Wat Pho for 4 baht.', upvotes: 39, createdAt: '2025-12-15' },
    ],
    'Kuala Lumpur': [
        { username: 'ExplorerMax', countryCode: 'GB', content: 'Jalan Alor street food is incredible — try the grilled wings and durian ice cream. Go around 7 PM.', upvotes: 47, createdAt: '2026-01-14' },
        { username: 'TrailBlazerJay', countryCode: 'CA', content: 'The Batu Caves are free to enter but bring water — 272 steps in Malaysian heat is no joke!', upvotes: 38, createdAt: '2026-01-02' },
    ],
    Tbilisi: [
        { username: 'WanderSarah', countryCode: 'US', content: 'The sulfur baths in Abanotubani are incredible. Go to the public bath (~5 GEL) for an authentic experience.', upvotes: 36, createdAt: '2026-01-11' },
        { username: 'NomadKai', countryCode: 'DE', content: 'Take the cable car up to Narikala Fortress for free views of the entire city. Best at sunset.', upvotes: 29, createdAt: '2025-12-25' },
    ],
    Budapest: [
        { username: 'GlobeAnya', countryCode: 'PL', content: 'Széchenyi Baths are a must, but go on a weekday to avoid crowds. The outdoor pools are heated even in winter!', upvotes: 52, createdAt: '2026-01-13' },
        { username: 'ExplorerMax', countryCode: 'GB', content: 'Walk across the Chain Bridge at night — the Parliament building illuminated is the best view in Budapest.', upvotes: 44, createdAt: '2026-01-06' },
    ],
    'Mexico City': [
        { username: 'TrailBlazerJay', countryCode: 'CA', content: 'Chapultepec Castle has the best city view and it\'s inside a massive park. Perfect half-day combo.', upvotes: 41, createdAt: '2026-01-09' },
        { username: 'NomadKai', countryCode: 'DE', content: 'Mercado de San Juan for exotic food, Mercado Roma for hipster brunch vibes. Both are amazing.', upvotes: 33, createdAt: '2025-12-30' },
    ],
    Istanbul: [
        { username: 'WanderSarah', countryCode: 'US', content: 'Get the Museum Pass Istanbul — it covers Topkapi, Hagia Sophia and more. Saves hours of queuing!', upvotes: 58, createdAt: '2026-01-15' },
        { username: 'GlobeAnya', countryCode: 'PL', content: 'Turkish breakfast at Karaköy Lokantası is life-changing. Go hungry, order the serpme kahvaltı.', upvotes: 45, createdAt: '2026-01-04' },
        { username: 'ExplorerMax', countryCode: 'GB', content: 'Take the ferry to Kadıköy on the Asian side. Much cheaper, less touristy, and the street food market is incredible.', upvotes: 37, createdAt: '2025-12-18' },
    ],
    'Buenos Aires': [
        { username: 'TrailBlazerJay', countryCode: 'CA', content: 'Don\'t exchange money at the airport — use the "blue dollar" rate at local exchange houses for 2x the official rate.', upvotes: 62, createdAt: '2026-01-11' },
        { username: 'NomadKai', countryCode: 'DE', content: 'San Telmo Sunday market is the best. Arrive by 10 AM for antiques, stay for tango dancers in the afternoon.', upvotes: 40, createdAt: '2025-12-22' },
    ],
    Lisbon: [
        { username: 'WanderSarah', countryCode: 'US', content: 'Tram 28 is iconic but overcrowded. Walk the same route uphill through Alfama — way more charming.', upvotes: 49, createdAt: '2026-01-13' },
        { username: 'GlobeAnya', countryCode: 'PL', content: 'Pastéis de Belém is worth the queue. Get a dozen to go and eat them by the Tagus river.', upvotes: 42, createdAt: '2026-01-01' },
    ],
    Marrakech: [
        { username: 'ExplorerMax', countryCode: 'GB', content: 'Always negotiate in the souks — start at 1/3 of the asking price. Sellers expect it!', upvotes: 55, createdAt: '2026-01-10' },
        { username: 'TrailBlazerJay', countryCode: 'CA', content: 'Le Jardin Secret is a peaceful escape from the chaos of the medina. Beautiful tilework and gardens.', upvotes: 31, createdAt: '2025-12-28' },
    ],
    'Ho Chi Minh City': [
        { username: 'NomadKai', countryCode: 'DE', content: 'Grab a xe om (motorbike taxi) through District 1 at night — it\'s the best way to experience the energy of Saigon.', upvotes: 44, createdAt: '2026-01-07' },
        { username: 'WanderSarah', countryCode: 'US', content: 'The War Remnants Museum is heavy but essential. Plan at least 2 hours and go early.', upvotes: 38, createdAt: '2025-12-20' },
    ],
    Bali: [
        { username: 'GlobeAnya', countryCode: 'PL', content: 'Rent a scooter in Ubud and drive to Tegallalang Rice Terraces at sunrise — no crowds and magical light.', upvotes: 51, createdAt: '2026-01-14' },
        { username: 'ExplorerMax', countryCode: 'GB', content: 'Skip Kuta Beach — head to Seminyak or Canggu for much better surf and fewer hawkers.', upvotes: 36, createdAt: '2026-01-02' },
    ],
    Tokyo: [
        { username: 'TrailBlazerJay', countryCode: 'CA', content: 'Get a Suica card immediately at the airport. Works on all trains, buses, and even vending machines.', upvotes: 63, createdAt: '2026-01-12' },
        { username: 'NomadKai', countryCode: 'DE', content: 'Golden Gai in Shinjuku has tiny 6-seat bars with incredible character. Go after 9 PM, some charge a seat fee (~500 yen).', upvotes: 48, createdAt: '2025-12-25' },
        { username: 'WanderSarah', countryCode: 'US', content: 'Senso-ji Temple at 5 AM is empty and breathtaking. The Nakamise shopping street opens at 10.', upvotes: 41, createdAt: '2025-12-10' },
    ],
    Bogota: [
        { username: 'GlobeAnya', countryCode: 'PL', content: 'La Candelaria is the historic center — explore on foot but stay aware. The graffiti tour is world-class.', upvotes: 35, createdAt: '2026-01-08' },
        { username: 'ExplorerMax', countryCode: 'GB', content: 'Take the funicular up to Monserrate for panoramic views. Go on a clear day and bring a jacket — it\'s cold up top!', upvotes: 29, createdAt: '2025-12-15' },
    ],
    Hanoi: [
        { username: 'TrailBlazerJay', countryCode: 'CA', content: 'Egg coffee at Cafe Giang is a must — it tastes like tiramisu in a cup. The original shop is on Nguyen Huu Huan street.', upvotes: 57, createdAt: '2026-01-11' },
        { username: 'NomadKai', countryCode: 'DE', content: 'The Train Street experience is being restricted — check current schedules. Arrive 30 min before the train passes.', upvotes: 42, createdAt: '2026-01-03' },
    ],
};

export function getCommunityTipsForCity(city: string): DestinationTip[] {
    return communityTips[city] ?? [];
}
