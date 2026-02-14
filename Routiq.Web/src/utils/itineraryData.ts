import type { FlightInfo, AttractionInfo, AccommodationInfo } from '../types';

// ──────────────────────────────────────────────────────────────
// Mock data that mirrors the backend seed data.
// This will be replaced by real API calls when the backend
// enriches its RouteStopDto with flight/attraction/accommodation data.
// ──────────────────────────────────────────────────────────────

const flights: Record<string, FlightInfo> = {
    Belgrade: {
        origin: 'Istanbul', destination: 'Belgrade', airlineName: 'Turkish Airlines',
        flightNumber: 'TK1081', departureTime: '08:30', arrivalTime: '10:15',
        isDirect: true, averagePrice: 120, minPrice: 90, maxPrice: 174, currency: 'USD',
    },
    Kotor: {
        origin: 'Istanbul', destination: 'Kotor', airlineName: 'Turkish Airlines',
        flightNumber: 'TK1093', departureTime: '07:00', arrivalTime: '08:40',
        isDirect: true, averagePrice: 145, minPrice: 109, maxPrice: 210, currency: 'USD',
    },
    Tirana: {
        origin: 'Istanbul', destination: 'Tirana', airlineName: 'Pegasus',
        flightNumber: 'PC1120', departureTime: '10:45', arrivalTime: '12:30',
        isDirect: true, averagePrice: 95, minPrice: 71, maxPrice: 138, currency: 'USD',
    },
    Sarajevo: {
        origin: 'Istanbul', destination: 'Sarajevo', airlineName: 'Turkish Airlines',
        flightNumber: 'TK1021', departureTime: '09:00', arrivalTime: '10:45',
        isDirect: true, averagePrice: 130, minPrice: 98, maxPrice: 189, currency: 'USD',
    },
    Paris: {
        origin: 'Istanbul', destination: 'Paris', airlineName: 'Turkish Airlines',
        flightNumber: 'TK1821', departureTime: '06:30', arrivalTime: '09:00',
        isDirect: true, averagePrice: 280, minPrice: 210, maxPrice: 406, currency: 'USD',
    },
    Rome: {
        origin: 'Istanbul', destination: 'Rome', airlineName: 'Turkish Airlines',
        flightNumber: 'TK1861', departureTime: '07:45', arrivalTime: '09:40',
        isDirect: true, averagePrice: 250, minPrice: 188, maxPrice: 363, currency: 'USD',
    },
    Berlin: {
        origin: 'Istanbul', destination: 'Berlin', airlineName: 'Turkish Airlines',
        flightNumber: 'TK1723', departureTime: '08:00', arrivalTime: '10:15',
        isDirect: true, averagePrice: 220, minPrice: 165, maxPrice: 319, currency: 'USD',
    },
    Barcelona: {
        origin: 'Istanbul', destination: 'Barcelona', airlineName: 'Turkish Airlines',
        flightNumber: 'TK1851', departureTime: '10:00', arrivalTime: '12:40',
        isDirect: true, averagePrice: 260, minPrice: 195, maxPrice: 377, currency: 'USD',
    },
    Bangkok: {
        origin: 'Istanbul', destination: 'Bangkok', airlineName: 'Turkish Airlines',
        flightNumber: 'TK0068', departureTime: '01:30', arrivalTime: '15:30',
        isDirect: true, averagePrice: 450, minPrice: 338, maxPrice: 653, currency: 'USD',
    },
    Tokyo: {
        origin: 'Istanbul', destination: 'Tokyo', airlineName: 'Turkish Airlines',
        flightNumber: 'TK0052', departureTime: '02:00', arrivalTime: '19:30',
        isDirect: true, averagePrice: 620, minPrice: 465, maxPrice: 899, currency: 'USD',
    },
    Hanoi: {
        origin: 'Istanbul', destination: 'Hanoi', airlineName: 'Turkish Airlines',
        flightNumber: 'TK0164', departureTime: '03:15', arrivalTime: '18:15',
        isDirect: false, averagePrice: 520, minPrice: 390, maxPrice: 754, currency: 'USD',
    },
    Bali: {
        origin: 'Istanbul', destination: 'Bali', airlineName: 'Turkish Airlines',
        flightNumber: 'TK0056', departureTime: '22:00', arrivalTime: '18:00',
        isDirect: false, averagePrice: 550, minPrice: 413, maxPrice: 798, currency: 'USD',
    },
    Antalya: {
        origin: 'Istanbul', destination: 'Antalya', airlineName: 'Turkish Airlines',
        flightNumber: 'TK2410', departureTime: '07:00', arrivalTime: '08:15',
        isDirect: true, averagePrice: 60, minPrice: 45, maxPrice: 87, currency: 'USD',
    },
    Cappadocia: {
        origin: 'Istanbul', destination: 'Cappadocia', airlineName: 'Turkish Airlines',
        flightNumber: 'TK2010', departureTime: '06:00', arrivalTime: '07:20',
        isDirect: true, averagePrice: 75, minPrice: 56, maxPrice: 109, currency: 'USD',
    },
    Istanbul: {
        origin: 'Ankara', destination: 'Istanbul', airlineName: 'Turkish Airlines',
        flightNumber: 'TK2124', departureTime: '07:00', arrivalTime: '08:10',
        isDirect: true, averagePrice: 65, minPrice: 49, maxPrice: 94, currency: 'USD',
    },
};

const attractions: Record<string, AttractionInfo[]> = {
    Belgrade: [
        { name: 'Belgrade Fortress', description: 'A sprawling fortress at the confluence of the Sava and Danube rivers with panoramic city views.', category: 'Historical', estimatedCost: 0, estimatedDurationInHours: 2, bestTimeOfDay: 'Afternoon' },
        { name: 'Skadarlija Bohemian Quarter', description: 'Charming cobblestone street lined with traditional Serbian restaurants and live music.', category: 'Entertainment', estimatedCost: 15, estimatedDurationInHours: 1.5, bestTimeOfDay: 'Evening' },
    ],
    Kotor: [
        { name: 'Old Town Walk', description: 'Wander through the UNESCO-listed medieval old town with Venetian architecture.', category: 'Historical', estimatedCost: 0, estimatedDurationInHours: 2, bestTimeOfDay: 'Morning' },
        { name: 'San Giovanni Fortress Hike', description: 'A steep 1,350-step climb with breathtaking views over the Bay of Kotor.', category: 'Nature', estimatedCost: 8, estimatedDurationInHours: 3, bestTimeOfDay: 'Morning' },
    ],
    Tirana: [
        { name: 'Skanderbeg Square', description: 'Vast central plaza surrounded by government buildings and museums.', category: 'Historical', estimatedCost: 0, estimatedDurationInHours: 1, bestTimeOfDay: 'Anytime' },
        { name: "Bunk'Art Museum", description: "Cold War nuclear bunker converted into a fascinating museum.", category: 'Museum', estimatedCost: 5, estimatedDurationInHours: 1.5, bestTimeOfDay: 'Afternoon' },
    ],
    Sarajevo: [
        { name: 'Baščaršija Old Bazaar', description: 'Ottoman-era heart with copper workshops, cafes, and the Sebilj fountain.', category: 'Historical', estimatedCost: 0, estimatedDurationInHours: 2, bestTimeOfDay: 'Morning' },
        { name: 'Tunnel of Hope', description: 'The wartime tunnel that kept Sarajevo alive during the 1990s siege.', category: 'Museum', estimatedCost: 10, estimatedDurationInHours: 1.5, bestTimeOfDay: 'Afternoon' },
    ],
    Paris: [
        { name: 'Eiffel Tower', description: 'Iconic iron lattice tower offering stunning views of Paris.', category: 'Historical', estimatedCost: 26, estimatedDurationInHours: 2.5, bestTimeOfDay: 'Evening' },
        { name: 'Louvre Museum', description: "World's largest art museum housing the Mona Lisa.", category: 'Museum', estimatedCost: 17, estimatedDurationInHours: 3, bestTimeOfDay: 'Morning' },
        { name: 'Montmartre & Sacré-Cœur', description: 'Bohemian hilltop village with sweeping city panoramas.', category: 'Historical', estimatedCost: 0, estimatedDurationInHours: 2, bestTimeOfDay: 'Afternoon' },
    ],
    Rome: [
        { name: 'Colosseum', description: 'Legendary 2,000-year-old amphitheater where gladiators once battled.', category: 'Historical', estimatedCost: 18, estimatedDurationInHours: 2, bestTimeOfDay: 'Morning' },
        { name: 'Vatican Museums', description: "Vast art collection culminating in Michelangelo's Sistine Chapel.", category: 'Museum', estimatedCost: 20, estimatedDurationInHours: 3.5, bestTimeOfDay: 'Morning' },
        { name: 'Trevi Fountain', description: "Rome's most famous Baroque fountain — toss a coin to return.", category: 'Historical', estimatedCost: 0, estimatedDurationInHours: 0.5, bestTimeOfDay: 'Evening' },
    ],
    Berlin: [
        { name: 'Brandenburg Gate', description: 'Neoclassical triumphal arch symbolizing German unity.', category: 'Historical', estimatedCost: 0, estimatedDurationInHours: 1, bestTimeOfDay: 'Anytime' },
        { name: 'Museum Island', description: 'A UNESCO complex of five world-class museums on the Spree River.', category: 'Museum', estimatedCost: 19, estimatedDurationInHours: 3, bestTimeOfDay: 'Morning' },
    ],
    Barcelona: [
        { name: 'Sagrada Familia', description: "Gaudí's unfinished masterpiece with organic facades and light-filled interiors.", category: 'Historical', estimatedCost: 26, estimatedDurationInHours: 2, bestTimeOfDay: 'Morning' },
        { name: 'Park Güell', description: "Whimsical park with Gaudí's colorful mosaics and Mediterranean views.", category: 'Nature', estimatedCost: 10, estimatedDurationInHours: 2, bestTimeOfDay: 'Afternoon' },
    ],
    Bangkok: [
        { name: 'Grand Palace', description: 'Dazzling former royal residence with the Emerald Buddha.', category: 'Historical', estimatedCost: 15, estimatedDurationInHours: 2.5, bestTimeOfDay: 'Morning' },
        { name: 'Wat Pho', description: 'Home to the massive 46-meter reclining Buddha.', category: 'Historical', estimatedCost: 7, estimatedDurationInHours: 1.5, bestTimeOfDay: 'Morning' },
        { name: 'Chatuchak Weekend Market', description: 'Over 15,000 stalls selling everything imaginable.', category: 'Entertainment', estimatedCost: 0, estimatedDurationInHours: 3, bestTimeOfDay: 'Morning' },
    ],
    Tokyo: [
        { name: 'Senso-ji Temple', description: "Tokyo's oldest Buddhist temple with a vibrant shopping street.", category: 'Historical', estimatedCost: 0, estimatedDurationInHours: 1.5, bestTimeOfDay: 'Morning' },
        { name: 'Shibuya Crossing & Meiji Shrine', description: "World's busiest intersection then a serene forest shrine.", category: 'Entertainment', estimatedCost: 0, estimatedDurationInHours: 2, bestTimeOfDay: 'Afternoon' },
        { name: 'TeamLab Borderless', description: 'Immersive digital art museum with interactive projections.', category: 'Museum', estimatedCost: 30, estimatedDurationInHours: 2.5, bestTimeOfDay: 'Evening' },
    ],
    Hanoi: [
        { name: 'Hoan Kiem Lake', description: 'Tranquil lake with the iconic red Huc Bridge.', category: 'Nature', estimatedCost: 0, estimatedDurationInHours: 1, bestTimeOfDay: 'Morning' },
        { name: 'Ho Chi Minh Mausoleum', description: "Solemn mausoleum honoring Vietnam's revolutionary leader.", category: 'Historical', estimatedCost: 0, estimatedDurationInHours: 1.5, bestTimeOfDay: 'Morning' },
    ],
    Bali: [
        { name: 'Uluwatu Temple', description: 'Dramatic clifftop temple famous for sunset Kecak dance.', category: 'Historical', estimatedCost: 3, estimatedDurationInHours: 2, bestTimeOfDay: 'Evening' },
        { name: 'Tegallalang Rice Terraces', description: 'Stunning cascading rice paddies with ancient irrigation.', category: 'Nature', estimatedCost: 2, estimatedDurationInHours: 2.5, bestTimeOfDay: 'Morning' },
    ],
    Istanbul: [
        { name: 'Hagia Sophia', description: '1,500-year-old marvel: cathedral, mosque, and museum.', category: 'Historical', estimatedCost: 0, estimatedDurationInHours: 1.5, bestTimeOfDay: 'Morning' },
        { name: 'Grand Bazaar', description: "One of the world's oldest covered markets with 4,000 shops.", category: 'Entertainment', estimatedCost: 0, estimatedDurationInHours: 2, bestTimeOfDay: 'Afternoon' },
        { name: 'Topkapi Palace', description: 'Ottoman sultans seat for 400 years with imperial treasures.', category: 'Museum', estimatedCost: 15, estimatedDurationInHours: 2.5, bestTimeOfDay: 'Morning' },
        { name: 'Bosphorus Cruise', description: 'Scenic ferry between two continents past palaces and fortresses.', category: 'Entertainment', estimatedCost: 10, estimatedDurationInHours: 2, bestTimeOfDay: 'Afternoon' },
    ],
    Antalya: [
        { name: 'Kaleiçi Old Town', description: 'Walled old town of Ottoman lanes, Roman ruins, and ancient harbor.', category: 'Historical', estimatedCost: 0, estimatedDurationInHours: 2, bestTimeOfDay: 'Afternoon' },
        { name: 'Düden Waterfalls', description: 'Spectacular waterfalls cascading off sea cliffs into the Mediterranean.', category: 'Nature', estimatedCost: 5, estimatedDurationInHours: 1.5, bestTimeOfDay: 'Morning' },
    ],
    Cappadocia: [
        { name: 'Hot Air Balloon Ride', description: "Float over fairy chimneys at sunrise — world's top balloon experience.", category: 'Entertainment', estimatedCost: 180, estimatedDurationInHours: 1.5, bestTimeOfDay: 'Morning' },
        { name: 'Göreme Open-Air Museum', description: 'UNESCO rock-carved churches with stunning Byzantine frescoes.', category: 'Museum', estimatedCost: 12, estimatedDurationInHours: 2, bestTimeOfDay: 'Morning' },
    ],
};

const accommodations: Record<string, AccommodationInfo> = {
    Belgrade: { zoneName: 'Stari Grad (Old Town)', description: 'Historic heart with cafes and walkable access to Kalemegdan.', category: 'Mid-Range', averageNightlyCost: 45, currency: 'USD' },
    Kotor: { zoneName: 'Old Town Walls', description: 'Inside the UNESCO-listed medieval walls with bay views.', category: 'Mid-Range', averageNightlyCost: 55, currency: 'USD' },
    Tirana: { zoneName: 'Blloku District', description: "Tirana's trendiest neighborhood packed with cafes and nightlife.", category: 'Mid-Range', averageNightlyCost: 35, currency: 'USD' },
    Sarajevo: { zoneName: 'Baščaršija (Old Bazaar)', description: 'Ottoman-era district with atmospheric pensions.', category: 'Mid-Range', averageNightlyCost: 40, currency: 'USD' },
    Paris: { zoneName: 'Montmartre (18th Arr.)', description: 'Bohemian hilltop near Sacré-Cœur, more affordable than center.', category: 'Mid-Range', averageNightlyCost: 110, currency: 'USD' },
    Rome: { zoneName: 'Trastevere', description: 'Charming cobblestone streets with authentic trattorias.', category: 'Mid-Range', averageNightlyCost: 95, currency: 'USD' },
    Berlin: { zoneName: 'Mitte', description: 'Central Berlin with museums and Brandenburg Gate.', category: 'Mid-Range', averageNightlyCost: 90, currency: 'USD' },
    Barcelona: { zoneName: 'Gothic Quarter', description: 'Medieval lanes near La Rambla and the cathedral.', category: 'Mid-Range', averageNightlyCost: 100, currency: 'USD' },
    Bangkok: { zoneName: 'Khao San Road Area', description: 'Legendary backpacker hub with ultra-cheap stays.', category: 'Budget', averageNightlyCost: 15, currency: 'USD' },
    Tokyo: { zoneName: 'Shinjuku', description: 'Massive transit hub — ideal base for exploring Tokyo.', category: 'Mid-Range', averageNightlyCost: 110, currency: 'USD' },
    Hanoi: { zoneName: 'Old Quarter', description: 'Bustling 36 streets with budget hostels and street food.', category: 'Budget', averageNightlyCost: 15, currency: 'USD' },
    Bali: { zoneName: 'Ubud Center', description: 'Cultural heart surrounded by rice terraces and yoga retreats.', category: 'Mid-Range', averageNightlyCost: 50, currency: 'USD' },
    Istanbul: { zoneName: 'Sultanahmet (Old City)', description: 'Historic peninsula with Hagia Sophia and Blue Mosque.', category: 'Mid-Range', averageNightlyCost: 60, currency: 'USD' },
    Antalya: { zoneName: 'Kaleiçi (Old Town)', description: 'Boutique pensions inside ancient harbor walls.', category: 'Mid-Range', averageNightlyCost: 45, currency: 'USD' },
    Cappadocia: { zoneName: 'Göreme Center', description: 'Cave hotels carved into fairy chimneys with balloon views.', category: 'Mid-Range', averageNightlyCost: 70, currency: 'USD' },
};

export function getFlightForCity(city: string): FlightInfo | null {
    return flights[city] ?? null;
}

export function getReturnFlight(city: string): FlightInfo | null {
    const outbound = flights[city];
    if (!outbound) return null;
    return {
        ...outbound,
        origin: outbound.destination,
        destination: outbound.origin,
        flightNumber: outbound.flightNumber.slice(0, 2) + String(Number(outbound.flightNumber.slice(2)) + 1).padStart(4, '0'),
        departureTime: outbound.arrivalTime,
        arrivalTime: outbound.departureTime,
    };
}

export function getAttractionsForCity(city: string): AttractionInfo[] {
    return attractions[city] ?? [];
}

export function getAccommodationForCity(city: string): AccommodationInfo | null {
    return accommodations[city] ?? null;
}
