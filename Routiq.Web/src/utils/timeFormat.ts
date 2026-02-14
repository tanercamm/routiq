/**
 * Flight time formatting utilities.
 * All time strings are in "HH:MM" 24-hour format from itineraryData.ts.
 */

/**
 * Convert "HH:MM" → "H:MM AM/PM"
 * Examples: "08:30" → "8:30 AM", "22:00" → "10:00 PM", "00:15" → "12:15 AM"
 */
export function formatTimeAMPM(time: string): string {
    const [h, m] = time.split(':').map(Number);
    const period = h >= 12 ? 'PM' : 'AM';
    const hour12 = h % 12 || 12;
    return `${hour12}:${m.toString().padStart(2, '0')} ${period}`;
}

/**
 * Calculate flight duration between departure and arrival times.
 * Handles overnight flights (arrival < departure → crosses midnight).
 * Returns "Xh Ym" format.
 */
export function calculateFlightDuration(departure: string, arrival: string): string {
    const [dh, dm] = departure.split(':').map(Number);
    const [ah, am] = arrival.split(':').map(Number);

    let totalMinutes = (ah * 60 + am) - (dh * 60 + dm);

    // If negative, it crosses midnight — add 24 hours
    if (totalMinutes <= 0) {
        totalMinutes += 24 * 60;
    }

    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;

    return minutes > 0 ? `${hours}h ${minutes}m` : `${hours}h`;
}

/**
 * Check if arrival is on the next day (crosses midnight).
 * True when arrival time is earlier than departure time.
 */
export function isNextDay(departure: string, arrival: string): boolean {
    const [dh, dm] = departure.split(':').map(Number);
    const [ah, am] = arrival.split(':').map(Number);
    return (ah * 60 + am) <= (dh * 60 + dm);
}
