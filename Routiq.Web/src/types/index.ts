// ── API Request / Response ──

export interface RouteRequest {
    passportCountry: string;
    totalBudget: number;
    durationDays: number;
}

export interface RouteResponse {
    options: RouteOption[];
}

export interface RouteOption {
    routeType: string;
    description: string;
    totalEstimatedCost: number;
    stops: RouteStop[];
}

export interface RouteStop {
    city: string;
    country: string;
    days: number;
    estimatedCost: number;
    climate: string;
    visaStatus: string;
}

// ── Rich Itinerary Types (mirrors backend entities) ──

export interface FlightInfo {
    origin: string;
    destination: string;
    airlineName: string;
    flightNumber: string;
    departureTime: string;
    arrivalTime: string;
    isDirect: boolean;
    averagePrice: number;
    minPrice: number;
    maxPrice: number;
    currency: string;
}

export interface AttractionInfo {
    name: string;
    description: string;
    category: string;       // "Historical", "Nature", "Museum", "Entertainment"
    estimatedCost: number;
    estimatedDurationInHours: number;
    bestTimeOfDay: string;   // "Morning", "Afternoon", "Evening", "Anytime"
}

export interface AccommodationInfo {
    zoneName: string;
    description: string;
    category: string;        // "Budget", "Mid-Range", "Luxury"
    averageNightlyCost: number;
    currency: string;
}

// ── Community & Gamification Types ──

export interface DestinationTip {
    username: string;
    countryCode: string;     // "TR", "US", "JP" — rendered as emoji flag
    content: string;
    upvotes: number;
    createdAt: string;
}
