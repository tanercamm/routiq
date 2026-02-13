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
