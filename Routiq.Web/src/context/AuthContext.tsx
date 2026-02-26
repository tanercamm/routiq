import { createContext, useContext, useState, useEffect } from 'react';
import type { ReactNode } from 'react';
import { routiqApi } from '../api/routiqApi';

interface User {
    email: string;
    name: string;
    role: string;
    /** ISO-3166-1 alpha-2 codes for all citizenships, e.g. ["TR", "DE"] */
    passports: string[];
}

interface AuthContextType {
    user: User | null;
    token: string | null;
    login: (token: string, user: User) => void;
    logout: () => void;
    isAuthenticated: boolean;
    /** Update the stored passports list (used from ProfilePage preferences) */
    updatePassports: (passports: string[]) => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
    const [user, setUser] = useState<User | null>(null);
    const [token, setToken] = useState<string | null>(null);

    useEffect(() => {
        const storedToken = localStorage.getItem('token');
        const storedUser = localStorage.getItem('user');

        if (storedToken && storedUser) {
            setToken(storedToken);
            const parsed = JSON.parse(storedUser);
            // Backward-compat: add passports if missing from stored data
            if (!parsed.passports) parsed.passports = ['TR'];
            setUser(parsed);
            routiqApi.defaults.headers.common['Authorization'] = `Bearer ${storedToken}`;
        }
    }, []);

    const login = (newToken: string, newUser: User) => {
        if (!newUser.passports) newUser.passports = [];
        setToken(newToken);
        setUser(newUser);
        localStorage.setItem('token', newToken);
        localStorage.setItem('user', JSON.stringify(newUser));
        routiqApi.defaults.headers.common['Authorization'] = `Bearer ${newToken}`;
    };

    const logout = () => {
        setToken(null);
        setUser(null);
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        delete routiqApi.defaults.headers.common['Authorization'];
    };

    const updatePassports = (passports: string[]) => {
        setUser(prev => {
            if (!prev) return prev;
            const updated = { ...prev, passports };
            localStorage.setItem('user', JSON.stringify(updated));
            return updated;
        });
    };

    return (
        <AuthContext.Provider value={{ user, token, login, logout, isAuthenticated: !!token, updatePassports }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error('useAuth must be used within an AuthProvider');
    }
    return context;
};
