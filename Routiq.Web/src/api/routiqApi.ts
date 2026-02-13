import axios from 'axios';
import type { RouteRequest, RouteResponse } from '../types';

const api = axios.create({
    baseURL: 'http://localhost:5107/api',
});

export const routiqApi = {
    generateRoutes: async (payload: RouteRequest): Promise<RouteResponse> => {
        const response = await api.post<RouteResponse>('/routes/generate', payload);
        return response.data;
    },
};
