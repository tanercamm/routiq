export interface CityPoint {
  name: string;
  country: string;
  lat: number;
  lng: number;
  isSupported: boolean;
  safetyIndex?: number;
  costOfLivingIndex?: number;
  avgMealCost?: number;
  bestMonths?: string;
}

// Routiq-supported cities from CityIntelligences DB
export const SUPPORTED_CITIES: CityPoint[] = [
  { name: 'Sofia', country: 'Bulgaria', lat: 42.6977, lng: 23.3219, isSupported: true, safetyIndex: 62.1, costOfLivingIndex: 30, avgMealCost: 8, bestMonths: '5,6,9,10' },
  { name: 'London', country: 'United Kingdom', lat: 51.5074, lng: -0.1278, isSupported: true, safetyIndex: 65.5, costOfLivingIndex: 80, avgMealCost: 25, bestMonths: '5,6,7,8,9' },
  { name: 'Rome', country: 'Italy', lat: 41.9028, lng: 12.4964, isSupported: true, safetyIndex: 60.5, costOfLivingIndex: 60, avgMealCost: 18, bestMonths: '4,5,6,9,10' },
  { name: 'Barcelona', country: 'Spain', lat: 41.3851, lng: 2.1734, isSupported: true, safetyIndex: 55.4, costOfLivingIndex: 55, avgMealCost: 15, bestMonths: '4,5,6,9,10' },
  { name: 'Paris', country: 'France', lat: 48.8566, lng: 2.3522, isSupported: true, safetyIndex: 58.2, costOfLivingIndex: 75, avgMealCost: 22, bestMonths: '5,6,7,8,9,10' },
  { name: 'Amsterdam', country: 'Netherlands', lat: 52.3676, lng: 4.9041, isSupported: true, safetyIndex: 70, costOfLivingIndex: 78, avgMealCost: 22, bestMonths: '5,6,7,8,9' },
  { name: 'Prague', country: 'Czech Republic', lat: 50.0755, lng: 14.4378, isSupported: true, safetyIndex: 75.8, costOfLivingIndex: 45, avgMealCost: 12, bestMonths: '5,6,9,10' },
  { name: 'Vienna', country: 'Austria', lat: 48.2082, lng: 16.3738, isSupported: true, safetyIndex: 80.1, costOfLivingIndex: 65, avgMealCost: 20, bestMonths: '5,6,7,8,9' },
  { name: 'Budapest', country: 'Hungary', lat: 47.4979, lng: 19.0402, isSupported: true, safetyIndex: 68.3, costOfLivingIndex: 40, avgMealCost: 10, bestMonths: '5,6,9,10' },
  { name: 'Belgrade', country: 'Serbia', lat: 44.7866, lng: 20.4489, isSupported: true, safetyIndex: 63.8, costOfLivingIndex: 35, avgMealCost: 9, bestMonths: '5,6,9,10' },
  { name: 'Bucharest', country: 'Romania', lat: 44.4268, lng: 26.1025, isSupported: true, safetyIndex: 66.2, costOfLivingIndex: 32, avgMealCost: 9, bestMonths: '5,6,9,10' },
  { name: 'Sarajevo', country: 'Bosnia', lat: 43.8563, lng: 18.4131, isSupported: true, safetyIndex: 67.2, costOfLivingIndex: 29, avgMealCost: 6, bestMonths: '5,6,9,10' },
  { name: 'Tirana', country: 'Albania', lat: 41.3275, lng: 19.8187, isSupported: true, safetyIndex: 64.9, costOfLivingIndex: 30, avgMealCost: 7, bestMonths: '5,6,9,10' },
  { name: 'Skopje', country: 'North Macedonia', lat: 41.9973, lng: 21.428, isSupported: true, safetyIndex: 65.4, costOfLivingIndex: 28, avgMealCost: 6, bestMonths: '5,6,9,10' },
  { name: 'Ljubljana', country: 'Slovenia', lat: 46.0569, lng: 14.5058, isSupported: true, safetyIndex: 82.3, costOfLivingIndex: 50, avgMealCost: 12, bestMonths: '5,6,7,8,9' },
  { name: 'Kotor', country: 'Montenegro', lat: 42.4247, lng: 18.7712, isSupported: true, safetyIndex: 72.1, costOfLivingIndex: 40, avgMealCost: 12, bestMonths: '5,6,9,10' },
  { name: 'Dubrovnik', country: 'Croatia', lat: 42.6507, lng: 18.0944, isSupported: true, safetyIndex: 79.5, costOfLivingIndex: 60, avgMealCost: 20, bestMonths: '5,6,9,10' },
  { name: 'Krakow', country: 'Poland', lat: 50.0647, lng: 19.945, isSupported: true, safetyIndex: 78.9, costOfLivingIndex: 35, avgMealCost: 8, bestMonths: '5,6,7,8,9' },
  { name: 'Cluj-Napoca', country: 'Romania', lat: 46.7712, lng: 23.6236, isSupported: true, safetyIndex: 77.4, costOfLivingIndex: 34, avgMealCost: 9, bestMonths: '5,6,9,10' },
  { name: 'Tbilisi', country: 'Georgia', lat: 41.7151, lng: 44.8271, isSupported: true, safetyIndex: 73.5, costOfLivingIndex: 25, avgMealCost: 8, bestMonths: '5,6,9,10' },
  { name: 'Marrakech', country: 'Morocco', lat: 31.6295, lng: -7.9811, isSupported: true, safetyIndex: 63.4, costOfLivingIndex: 25, avgMealCost: 5, bestMonths: '3,4,5,9,10,11' },
  { name: 'Fez', country: 'Morocco', lat: 34.0181, lng: -5.0078, isSupported: true, safetyIndex: 60.1, costOfLivingIndex: 22, avgMealCost: 4, bestMonths: '3,4,5,9,10,11' },
  { name: 'Tangier', country: 'Morocco', lat: 35.7595, lng: -5.834, isSupported: true, safetyIndex: 65.5, costOfLivingIndex: 24, avgMealCost: 5, bestMonths: '4,5,6,9,10' },
  { name: 'Cairo', country: 'Egypt', lat: 30.0444, lng: 31.2357, isSupported: true, safetyIndex: 54.3, costOfLivingIndex: 18, avgMealCost: 3, bestMonths: '10,11,12,1,2,3,4' },
  { name: 'Tunis', country: 'Tunisia', lat: 36.8065, lng: 10.1815, isSupported: true, safetyIndex: 58.2, costOfLivingIndex: 20, avgMealCost: 4, bestMonths: '4,5,6,9,10' },
  { name: 'Bangkok', country: 'Thailand', lat: 13.7563, lng: 100.5018, isSupported: true, safetyIndex: 60.1, costOfLivingIndex: 35, avgMealCost: 4, bestMonths: '11,12,1,2' },
  { name: 'Chiang Mai', country: 'Thailand', lat: 18.7883, lng: 98.9853, isSupported: true, safetyIndex: 75.8, costOfLivingIndex: 30, avgMealCost: 3, bestMonths: '11,12,1,2' },
  { name: 'Phuket', country: 'Thailand', lat: 7.8804, lng: 98.3923, isSupported: true, safetyIndex: 58, costOfLivingIndex: 40, avgMealCost: 5, bestMonths: '11,12,1,2,3,4' },
  { name: 'Bali', country: 'Indonesia', lat: -8.3405, lng: 115.092, isSupported: true, safetyIndex: 68.2, costOfLivingIndex: 32, avgMealCost: 5, bestMonths: '5,6,7,8,9,10' },
  { name: 'Hanoi', country: 'Vietnam', lat: 21.0285, lng: 105.8542, isSupported: true, safetyIndex: 63.4, costOfLivingIndex: 28, avgMealCost: 3, bestMonths: '10,11,12,3,4' },
  { name: 'Ho Chi Minh City', country: 'Vietnam', lat: 10.8231, lng: 106.6297, isSupported: true, safetyIndex: 58.9, costOfLivingIndex: 30, avgMealCost: 3.5, bestMonths: '12,1,2,3' },
  { name: 'Hoi An', country: 'Vietnam', lat: 15.8801, lng: 108.338, isSupported: true, safetyIndex: 74.2, costOfLivingIndex: 25, avgMealCost: 4, bestMonths: '2,3,4,5' },
  { name: 'Siem Reap', country: 'Cambodia', lat: 13.3671, lng: 103.8448, isSupported: true, safetyIndex: 65.5, costOfLivingIndex: 26, avgMealCost: 5, bestMonths: '11,12,1,2' },
  { name: 'Kuala Lumpur', country: 'Malaysia', lat: 3.139, lng: 101.6869, isSupported: true, safetyIndex: 48.7, costOfLivingIndex: 32, avgMealCost: 4, bestMonths: '5,6,7' },
  { name: 'Seoul', country: 'South Korea', lat: 37.5665, lng: 126.978, isSupported: true, safetyIndex: 81.1, costOfLivingIndex: 65, avgMealCost: 7, bestMonths: '4,5,9,10,11' },
  { name: 'Tokyo', country: 'Japan', lat: 35.6762, lng: 139.6503, isSupported: true, safetyIndex: 82.5, costOfLivingIndex: 60, avgMealCost: 8, bestMonths: '3,4,5,9,10,11' },
  { name: 'Kyoto', country: 'Japan', lat: 35.0116, lng: 135.7681, isSupported: true, safetyIndex: 85.2, costOfLivingIndex: 55, avgMealCost: 7, bestMonths: '3,4,5,9,10,11' },
  { name: 'Osaka', country: 'Japan', lat: 34.6937, lng: 135.5023, isSupported: true, safetyIndex: 80.4, costOfLivingIndex: 50, avgMealCost: 6, bestMonths: '3,4,5,9,10,11' },
  { name: 'Colombo', country: 'Sri Lanka', lat: 6.9271, lng: 79.8612, isSupported: true, safetyIndex: 60.5, costOfLivingIndex: 30, avgMealCost: 4, bestMonths: '1,2,3,4' },
  { name: 'Malé Atolls', country: 'Maldives', lat: 4.1755, lng: 73.5093, isSupported: true, safetyIndex: 65, costOfLivingIndex: 75, avgMealCost: 30, bestMonths: '11,12,1,2,3,4' },
  { name: 'Buenos Aires', country: 'Argentina', lat: -34.6037, lng: -58.3816, isSupported: true, safetyIndex: 47.9, costOfLivingIndex: 20, avgMealCost: 6, bestMonths: '3,4,5,9,10,11' },
  { name: 'Cartagena', country: 'Colombia', lat: 10.391, lng: -75.5144, isSupported: true, safetyIndex: 46.5, costOfLivingIndex: 28, avgMealCost: 8, bestMonths: '1,2,3,12' },
  { name: 'Medellín', country: 'Colombia', lat: 6.2476, lng: -75.5658, isSupported: true, safetyIndex: 42.1, costOfLivingIndex: 22, avgMealCost: 5, bestMonths: '1,2,3,12' },
  { name: 'Mexico City', country: 'Mexico', lat: 19.4326, lng: -99.1332, isSupported: true, safetyIndex: 42.6, costOfLivingIndex: 35, avgMealCost: 8, bestMonths: '3,4,5,10,11' },
  { name: 'Lima', country: 'Peru', lat: -12.0464, lng: -77.0428, isSupported: true, safetyIndex: 33.8, costOfLivingIndex: 30, avgMealCost: 6, bestMonths: '12,1,2,3,4' },
  { name: 'Cusco', country: 'Peru', lat: -13.532, lng: -71.9675, isSupported: true, safetyIndex: 49.2, costOfLivingIndex: 25, avgMealCost: 5, bestMonths: '5,6,7,8,9,10' },
  { name: 'Panama City', country: 'Panama', lat: 8.9824, lng: -79.5199, isSupported: true, safetyIndex: 51.5, costOfLivingIndex: 55, avgMealCost: 10, bestMonths: '1,2,3,4' },
  { name: 'La Fortuna', country: 'Costa Rica', lat: 10.4719, lng: -84.6427, isSupported: true, safetyIndex: 65.5, costOfLivingIndex: 45, avgMealCost: 12, bestMonths: '12,1,2,3,4' },
  { name: 'San José', country: 'Costa Rica', lat: 9.9281, lng: -84.0907, isSupported: true, safetyIndex: 44.1, costOfLivingIndex: 50, avgMealCost: 10, bestMonths: '12,1,2,3,4' },
  { name: 'Guatemala City', country: 'Guatemala', lat: 14.6349, lng: -90.5069, isSupported: true, safetyIndex: 38.5, costOfLivingIndex: 35, avgMealCost: 6, bestMonths: '11,12,1,2,3,4' },
  { name: 'Antigua', country: 'Guatemala', lat: 14.5586, lng: -90.7295, isSupported: true, safetyIndex: 52.4, costOfLivingIndex: 40, avgMealCost: 8, bestMonths: '11,12,1,2,3,4' },
  { name: 'Granada', country: 'Nicaragua', lat: 11.9344, lng: -85.956, isSupported: true, safetyIndex: 55, costOfLivingIndex: 25, avgMealCost: 5, bestMonths: '12,1,2,3,4' },
  { name: 'Geneva', country: 'Switzerland', lat: 46.2044, lng: 6.1432, isSupported: true, safetyIndex: 75.4, costOfLivingIndex: 115, avgMealCost: 38, bestMonths: '6,7,8,9' },
  { name: 'Zurich', country: 'Switzerland', lat: 47.3769, lng: 8.5417, isSupported: true, safetyIndex: 82.3, costOfLivingIndex: 120, avgMealCost: 40, bestMonths: '6,7,8,9' },
  { name: 'Amalfi Coast', country: 'Italy', lat: 40.6333, lng: 14.6029, isSupported: true, safetyIndex: 75, costOfLivingIndex: 85, avgMealCost: 30, bestMonths: '5,6,7,8,9' },
];

// World capitals (non-supported) for subtle background markers
export const WORLD_CAPITALS: CityPoint[] = [
  { name: 'Washington D.C.', country: 'United States', lat: 38.9072, lng: -77.0369, isSupported: false },
  { name: 'Ottawa', country: 'Canada', lat: 45.4215, lng: -75.6972, isSupported: false },
  { name: 'Brasília', country: 'Brazil', lat: -15.7975, lng: -47.8919, isSupported: false },
  { name: 'Santiago', country: 'Chile', lat: -33.4489, lng: -70.6693, isSupported: false },
  { name: 'Bogotá', country: 'Colombia', lat: 4.711, lng: -74.0721, isSupported: false },
  { name: 'Quito', country: 'Ecuador', lat: -0.1807, lng: -78.4678, isSupported: false },
  { name: 'Montevideo', country: 'Uruguay', lat: -34.9011, lng: -56.1645, isSupported: false },
  { name: 'Havana', country: 'Cuba', lat: 23.1136, lng: -82.3666, isSupported: false },
  { name: 'Berlin', country: 'Germany', lat: 52.52, lng: 13.405, isSupported: false },
  { name: 'Madrid', country: 'Spain', lat: 40.4168, lng: -3.7038, isSupported: false },
  { name: 'Lisbon', country: 'Portugal', lat: 38.7223, lng: -9.1393, isSupported: false },
  { name: 'Warsaw', country: 'Poland', lat: 52.2297, lng: 21.0122, isSupported: false },
  { name: 'Stockholm', country: 'Sweden', lat: 59.3293, lng: 18.0686, isSupported: false },
  { name: 'Oslo', country: 'Norway', lat: 59.9139, lng: 10.7522, isSupported: false },
  { name: 'Copenhagen', country: 'Denmark', lat: 55.6761, lng: 12.5683, isSupported: false },
  { name: 'Helsinki', country: 'Finland', lat: 60.1699, lng: 24.9384, isSupported: false },
  { name: 'Dublin', country: 'Ireland', lat: 53.3498, lng: -6.2603, isSupported: false },
  { name: 'Brussels', country: 'Belgium', lat: 50.8503, lng: 4.3517, isSupported: false },
  { name: 'Bern', country: 'Switzerland', lat: 46.9481, lng: 7.4474, isSupported: false },
  { name: 'Athens', country: 'Greece', lat: 37.9838, lng: 23.7275, isSupported: false },
  { name: 'Ankara', country: 'Turkey', lat: 39.9334, lng: 32.8597, isSupported: false },
  { name: 'Kyiv', country: 'Ukraine', lat: 50.4501, lng: 30.5234, isSupported: false },
  { name: 'Moscow', country: 'Russia', lat: 55.7558, lng: 37.6173, isSupported: false },
  { name: 'Beijing', country: 'China', lat: 39.9042, lng: 116.4074, isSupported: false },
  { name: 'New Delhi', country: 'India', lat: 28.6139, lng: 77.209, isSupported: false },
  { name: 'Islamabad', country: 'Pakistan', lat: 33.6844, lng: 73.0479, isSupported: false },
  { name: 'Dhaka', country: 'Bangladesh', lat: 23.8103, lng: 90.4125, isSupported: false },
  { name: 'Jakarta', country: 'Indonesia', lat: -6.2088, lng: 106.8456, isSupported: false },
  { name: 'Manila', country: 'Philippines', lat: 14.5995, lng: 120.9842, isSupported: false },
  { name: 'Phnom Penh', country: 'Cambodia', lat: 11.5564, lng: 104.9282, isSupported: false },
  { name: 'Vientiane', country: 'Laos', lat: 17.9757, lng: 102.6331, isSupported: false },
  { name: 'Naypyidaw', country: 'Myanmar', lat: 19.7633, lng: 96.0785, isSupported: false },
  { name: 'Canberra', country: 'Australia', lat: -35.2809, lng: 149.13, isSupported: false },
  { name: 'Wellington', country: 'New Zealand', lat: -41.2865, lng: 174.7762, isSupported: false },
  { name: 'Nairobi', country: 'Kenya', lat: -1.2921, lng: 36.8219, isSupported: false },
  { name: 'Addis Ababa', country: 'Ethiopia', lat: 8.9806, lng: 38.7578, isSupported: false },
  { name: 'Pretoria', country: 'South Africa', lat: -25.7479, lng: 28.2293, isSupported: false },
  { name: 'Accra', country: 'Ghana', lat: 5.6037, lng: -0.187, isSupported: false },
  { name: 'Lagos', country: 'Nigeria', lat: 6.5244, lng: 3.3792, isSupported: false },
  { name: 'Algiers', country: 'Algeria', lat: 36.7538, lng: 3.0588, isSupported: false },
  { name: 'Riyadh', country: 'Saudi Arabia', lat: 24.7136, lng: 46.6753, isSupported: false },
  { name: 'Abu Dhabi', country: 'UAE', lat: 24.4539, lng: 54.3773, isSupported: false },
  { name: 'Doha', country: 'Qatar', lat: 25.2854, lng: 51.531, isSupported: false },
  { name: 'Tehran', country: 'Iran', lat: 35.6892, lng: 51.389, isSupported: false },
  { name: 'Baghdad', country: 'Iraq', lat: 33.3152, lng: 44.3661, isSupported: false },
  { name: 'Amman', country: 'Jordan', lat: 31.9454, lng: 35.9284, isSupported: false },
  { name: 'Beirut', country: 'Lebanon', lat: 33.8938, lng: 35.5018, isSupported: false },
  { name: 'Baku', country: 'Azerbaijan', lat: 40.4093, lng: 49.8671, isSupported: false },
  { name: 'Yerevan', country: 'Armenia', lat: 40.1792, lng: 44.4991, isSupported: false },
  { name: 'Astana', country: 'Kazakhstan', lat: 51.1694, lng: 71.4491, isSupported: false },
  { name: 'Tashkent', country: 'Uzbekistan', lat: 41.2995, lng: 69.2401, isSupported: false },
  { name: 'Ulaanbaatar', country: 'Mongolia', lat: 47.8864, lng: 106.9057, isSupported: false },
  { name: 'Taipei', country: 'Taiwan', lat: 25.033, lng: 121.5654, isSupported: false },
  { name: 'Singapore', country: 'Singapore', lat: 1.3521, lng: 103.8198, isSupported: false },
  { name: 'Reykjavik', country: 'Iceland', lat: 64.1466, lng: -21.9426, isSupported: false },
  { name: 'Vilnius', country: 'Lithuania', lat: 54.6872, lng: 25.2797, isSupported: false },
  { name: 'Riga', country: 'Latvia', lat: 56.9496, lng: 24.1052, isSupported: false },
  { name: 'Tallinn', country: 'Estonia', lat: 59.437, lng: 24.7536, isSupported: false },
  { name: 'Zagreb', country: 'Croatia', lat: 45.815, lng: 15.9819, isSupported: false },
  { name: 'Podgorica', country: 'Montenegro', lat: 42.4304, lng: 19.2594, isSupported: false },
];

export const ALL_CITIES: CityPoint[] = [...SUPPORTED_CITIES, ...WORLD_CAPITALS];

const MONTH_NAMES: Record<string, string> = {
  '1': 'Jan', '2': 'Feb', '3': 'Mar', '4': 'Apr', '5': 'May', '6': 'Jun',
  '7': 'Jul', '8': 'Aug', '9': 'Sep', '10': 'Oct', '11': 'Nov', '12': 'Dec',
};

export function formatBestMonths(months?: string): string {
  if (!months) return 'Year-round';
  return months.split(',').map(m => MONTH_NAMES[m.trim()] || m.trim()).join(', ');
}

export function getSafetyColor(index?: number): string {
  if (!index) return '#64748b';
  if (index >= 75) return '#22c55e';
  if (index >= 60) return '#eab308';
  return '#ef4444';
}

export function getSafetyLabel(index?: number): string {
  if (!index) return 'Unknown';
  if (index >= 75) return 'Very Safe';
  if (index >= 60) return 'Moderate';
  return 'Caution';
}
