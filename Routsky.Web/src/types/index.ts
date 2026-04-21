// ── V2 Enums ──

export type BudgetBracket = 'Shoestring' | 'Budget' | 'Mid' | 'Comfort' | 'Luxury';

export type RegionPreference =
    | 'Any'
    | 'SoutheastAsia'
    | 'EasternEurope'
    | 'Balkans'
    | 'LatinAmerica'
    | 'NorthAfrica'
    | 'CentralAsia'
    | 'CentralAmerica'
    | 'MiddleEast'
    | 'Caribbean';

export type UnitPreference = 'Metric' | 'Imperial';
export type TravelStyle = 'Shoestring' | 'Budget' | 'Comfort' | 'Luxury';

export interface User {
    id: number;
    email: string;
    name: string;
    role: string;
    avatarUrl?: string | null;
    avatarBase64?: string | null;
    passports: string[];
    origin: string;
    preferredCurrency: string;
    unitPreference: string;
    travelStyle: TravelStyle;
    notificationsEnabled: boolean;
    priceAlertsEnabled: boolean;
}

export interface AnalyticsData {
    totalGroupSavings: number;
    carbonFootprintEstimate: {
        totalKgCo2: number;
        offsetPercentage: number;
    };
    popularRegions: { name: string; value: number }[];
}

// ── V2 API Request ──

export interface RouteRequest {
    /** All passport codes held by the traveler. e.g. ["TR", "DE"] */
    passports: string[];
    budgetBracket: BudgetBracket;
    totalBudgetUsd: number;
    durationDays: number;
    regionPreference: RegionPreference;
    hasSchengenVisa: boolean;
    hasUsVisa: boolean;
    hasUkVisa: boolean;
}

// ── V2 API Response ──

export interface RouteResponse {
    options: RouteOption[];
    eliminations: EliminationSummary[];
}

export interface RouteOption {
    routeName: string;
    selectionReason: string;
    estimatedBudgetRange: string;
    stops: RouteStop[];
}

export interface RouteStop {
    order: number;
    city: string;
    country: string;
    countryCode: string;
    region: string;
    recommendedDays: number;
    costLevel: string;        // "Low" | "Medium" | "High"
    dailyBudgetRange: string; // e.g. "$20–$45/day"
    visaStatus: string;       // "Visa-Free (DE)" | "eVisa (TR)" | etc.
    bestPassport?: string;    // which passport yielded the best visa outcome
    stopReason?: string;
}

export interface EliminationSummary {
    city: string;
    country: string;
    reason: string;           // EliminationReason enum name
    explanation: string;
}

export type VisaMapStatus =
    | 'VisaFree'
    | 'EVisaOrOnArrival'
    | 'ConditionalOrTimeLimited'
    | 'VisaRequired'
    | 'BannedOrRefused'
    | 'Unknown';

export interface GlobalVisaCountryStatus {
    status: VisaMapStatus;
    source: string;
    rawRuleName?: string;
    rawColor?: string;
    durationDays?: number;
}

export interface GlobalVisaMapResponse {
    passportCode: string;
    generatedAtUtc: string;
    countries: Record<string, GlobalVisaCountryStatus>;
}

export type VoteType = 'Upvote' | 'Downvote';

export interface RouteVoteSummary {
    userId: number;
    isUpvote: boolean;
}

export interface GroupShortlistRoute {
    id: string;
    destinationId: string;
    addedByUserId: number;
    addedAt: string;
    upvotes: number;
    downvotes: number;
    upvoterIds: number[];
    downvoterIds: number[];
    currentUserVote?: VoteType;
    votes: RouteVoteSummary[];
}

export interface VoteShortlistRequest {
    voteType: VoteType;
}

export interface VoteShortlistResponse {
    message: string;
    route: GroupShortlistRoute;
}
