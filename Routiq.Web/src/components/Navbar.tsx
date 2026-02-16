import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useTheme } from '../context/ThemeContext';
import { User } from 'lucide-react';

const NAV_LINKS = [
    { label: 'Routes', path: '/routes' },
    { label: 'Analytics', path: '/analytics' },
    { label: 'Travel Groups', path: '/team' },
    { label: 'Settings', path: '/settings' },
];

export const Navbar = () => {
    const { logout } = useAuth();
    const { theme, toggleTheme } = useTheme();
    const navigate = useNavigate();
    const location = useLocation();

    return (
        <header className="border-b border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-950 sticky top-0 z-20">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
                {/* Brand */}
                <div className="flex items-center gap-3">
                    <button onClick={() => navigate('/')} className="flex items-center gap-3 group">
                        <span className="text-2xl font-extrabold text-blue-600 dark:text-blue-400 tracking-tight group-hover:text-blue-700 dark:group-hover:text-blue-300 transition-colors">
                            Routiq
                        </span>
                        <span className="text-sm text-gray-400 dark:text-gray-500 font-medium hidden sm:inline">
                            Enterprise Dashboard
                        </span>
                    </button>
                </div>

                {/* Nav Links */}
                <nav className="hidden md:flex items-center gap-1">
                    {NAV_LINKS.map(({ label, path }) => {
                        const isActive = location.pathname === path;
                        return (
                            <button
                                key={path}
                                onClick={() => navigate(path)}
                                className={`text-sm font-medium px-3 py-2 rounded-lg transition-colors ${isActive
                                        ? 'text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-500/10'
                                        : 'text-gray-500 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white hover:bg-gray-50 dark:hover:bg-gray-800'
                                    }`}
                            >
                                {label}
                            </button>
                        );
                    })}
                </nav>

                {/* Right Actions */}
                <div className="flex items-center gap-3">
                    <button
                        onClick={toggleTheme}
                        className="p-2 rounded-full hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors text-lg"
                        title={`Switch to ${theme === 'dark' ? 'light' : 'dark'} mode`}
                    >
                        {theme === 'dark' ? '☀️' : '🌙'}
                    </button>
                    <button
                        onClick={() => navigate('/profile')}
                        className={`w-9 h-9 rounded-full flex items-center justify-center transition-all ${location.pathname === '/profile'
                                ? 'bg-blue-600 dark:bg-blue-500 ring-2 ring-blue-300 dark:ring-blue-500/40'
                                : 'bg-blue-100 dark:bg-blue-500/20 hover:ring-2 ring-blue-300 dark:ring-blue-500/40'
                            }`}
                        title="Profile"
                    >
                        <User size={16} className={location.pathname === '/profile' ? 'text-white' : 'text-blue-600 dark:text-blue-400'} />
                    </button>
                    <button
                        onClick={logout}
                        className="text-sm text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200 font-medium transition-colors"
                    >
                        Sign Out
                    </button>
                </div>
            </div>
        </header>
    );
};
