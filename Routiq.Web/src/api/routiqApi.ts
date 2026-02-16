import axios from 'axios';
import type { RouteRequest, RouteResponse } from '../types';

const api = axios.create({
    baseURL: 'http://localhost:5107/api',
});

export const routiqApi = api;

export const login = async (credentials: { email: string; password: string }) => {
    const response = await api.post('/auth/login', credentials);
    return response.data;
};

export const register = async (userData: { email: string; password: string; firstName: string; lastName: string }) => {
    const response = await api.post('/auth/register', userData);
    return response.data;
};

export const generateRoutes = async (payload: RouteRequest): Promise<RouteResponse> => {
    const response = await api.post<RouteResponse>('/routes/generate', payload);
    return response.data;
};
// Gamification
export interface LeaderboardDto {
    username: string;
    countryCode: string; // ISO 2-letter, e.g. "TR", "US"
    age: number;
    totalPoints: number;
    tripCount: number;
    checkInCount: number;
}

export const getLeaderboard = async (topCount: number = 10): Promise<LeaderboardDto[]> => {
    const response = await api.get<LeaderboardDto[]>(`/gamification/leaderboard?topCount=${topCount}`);
    return response.data;
};
