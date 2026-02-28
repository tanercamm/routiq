import { createContext, useContext, useState, useEffect } from 'react';
import type { ReactNode } from 'react';
import { routiqApi } from '../api/routiqApi';

interface User {
    id: number;
    email: string;
    name: string;
    role: string;
    avatarUrl?: string;
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
    updatePassports: (passports: string[]) => Promise<void>;
    /** Update the user's avatar URL dynamically */
    setUserAvatar: (url: string | null) => void;
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
            try {
                const parsed = JSON.parse(storedUser);

                // Explicitly map the URL
                parsed.avatarUrl = parsed.avatarUrl || parsed.AvatarUrl;

                // Safely ensure passports is an array
                if (!parsed.passports) {
                    parsed.passports = ['TR'];
                } else if (typeof parsed.passports === 'string') {
                    try {
                        const innerParsed = JSON.parse(parsed.passports);
                        parsed.passports = Array.isArray(innerParsed) ? innerParsed : ['TR'];
                    } catch {
                        parsed.passports = ['TR'];
                    }
                } else if (!Array.isArray(parsed.passports)) {
                    parsed.passports = ['TR'];
                }

                setUser(parsed);
            } catch (e) {
                console.error("Failed to parse stored user:", e);
                localStorage.removeItem('user');
                localStorage.removeItem('token');
            }
        }

        if (storedToken) {
            routiqApi.defaults.headers.common['Authorization'] = `Bearer ${storedToken}`;

            // Hydrate the latest state directly from backend
            routiqApi.get('/auth/me').then(res => {
                if (res.data) {
                    const refreshedUser = res.data;

                    // EXACT MAPPING: Read from data.avatarUrl or data.AvatarUrl
                    refreshedUser.avatarUrl = refreshedUser.avatarUrl || refreshedUser.AvatarUrl;

                    // Safely ensure passports is an array from API too
                    if (!refreshedUser.passports) {
                        refreshedUser.passports = ['TR'];
                    } else if (typeof refreshedUser.passports === 'string') {
                        try {
                            const innerParsed = JSON.parse(refreshedUser.passports);
                            refreshedUser.passports = Array.isArray(innerParsed) ? innerParsed : ['TR'];
                        } catch {
                            refreshedUser.passports = ['TR'];
                        }
                    } else if (!Array.isArray(refreshedUser.passports)) {
                        refreshedUser.passports = ['TR'];
                    }

                    setUser(refreshedUser);
                    localStorage.setItem('user', JSON.stringify(refreshedUser));
                }
            }).catch(err => {
                console.error("Failed to hydrate user profile:", err);
            });
        }
    }, []);

    const login = (newToken: string, newUser: User) => {
        // Explicitly map the URL
        newUser.avatarUrl = newUser.avatarUrl || (newUser as any).AvatarUrl;

        // Safely ensure passports is an array
        if (!newUser.passports) {
            newUser.passports = ['TR'];
        } else if (typeof newUser.passports === 'string') {
            try {
                const innerParsed = JSON.parse(newUser.passports);
                newUser.passports = Array.isArray(innerParsed) ? innerParsed : ['TR'];
            } catch {
                newUser.passports = ['TR'];
            }
        } else if (!Array.isArray(newUser.passports)) {
            newUser.passports = ['TR'];
        }

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

    const updatePassports = async (passports: string[]) => {
        try {
            await routiqApi.put('/auth/profile', { passports });
            setUser(prev => {
                if (!prev) return prev;
                const updated = { ...prev, passports };
                localStorage.setItem('user', JSON.stringify(updated));
                return updated;
            });
        } catch (err) {
            console.error("Failed to update passports:", err);
        }
    };

    const setUserAvatar = (url: string | null) => {
        setUser(prev => {
            if (!prev) return prev;
            const updated = { ...prev, avatarUrl: url || undefined };
            localStorage.setItem('user', JSON.stringify(updated));
            return updated;
        });
    };

    return (
        <AuthContext.Provider value={{ user, token, login, logout, isAuthenticated: !!token, updatePassports, setUserAvatar }}>
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
