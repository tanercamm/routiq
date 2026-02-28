import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useTheme } from '../context/ThemeContext';
import { User, LogOut } from 'lucide-react';

const NAV_LINKS = [
    { label: 'Routes', path: '/routes' },
    { label: 'Analytics', path: '/analytics' },
    { label: 'Travel Groups', path: '/team' },
    { label: 'Settings', path: '/settings' },
];

export const Navbar = () => {
    const { logout, user } = useAuth();
    const { theme, toggleTheme } = useTheme();
    const navigate = useNavigate();
    const location = useLocation();

    return (
        <header className="border-b border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-950 sticky top-0 z-20">
            <div className="max-w-[1600px] w-[96%] mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">

                {/* 1. Left Group: Logo & Brand Name */}
                <div className="flex-1 flex items-center justify-start gap-3">
                    <button onClick={() => navigate('/')} className="flex items-center gap-3 group">
                        <span className="text-2xl font-extrabold text-blue-600 dark:text-blue-400 tracking-tight group-hover:text-blue-700 dark:group-hover:text-blue-300 transition-colors">
                            Routiq
                        </span>
                        <span className="text-sm text-gray-400 dark:text-gray-500 font-medium hidden sm:inline">
                            Travel Management
                        </span>
                    </button>
                </div>

                {/* 2. Center Group: Navigation Links */}
                <nav className="flex-1 hidden md:flex items-center justify-center gap-1">
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

                {/* 3. Right Group: Theme Toggle, User Profile, Logout */}
                <div className="flex-1 flex items-center justify-end gap-4">
                    <button
                        onClick={toggleTheme}
                        className="p-2 rounded-full hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors text-lg"
                        title={`Switch to ${theme === 'dark' ? 'light' : 'dark'} mode`}
                    >
                        {theme === 'dark' ? '☀️' : '🌙'}
                    </button>

                    <div className="flex items-center gap-2">
                        <button
                            onClick={() => navigate('/profile')}
                            className={`w-9 h-9 rounded-full flex items-center justify-center transition-all overflow-hidden ${location.pathname === '/profile'
                                ? 'bg-blue-600 dark:bg-blue-500 ring-2 ring-blue-300 dark:ring-blue-500/40'
                                : 'bg-blue-100 dark:bg-blue-500/20 hover:ring-2 hover:ring-blue-300 dark:hover:ring-blue-500/40'
                                }`}
                            title="Profile"
                        >
                            {user?.avatarUrl ? (
                                <img src={user.avatarUrl.startsWith('http') ? user.avatarUrl : `http://localhost:5107${user.avatarUrl}`} alt="Navbar Avatar" className="w-full h-full object-cover" />
                            ) : (
                                <User size={16} className={location.pathname === '/profile' ? 'text-white' : 'text-blue-600 dark:text-blue-400'} />
                            )}
                        </button>
                        <span className="text-sm font-medium text-gray-700 dark:text-gray-300 hidden sm:inline cursor-pointer hover:text-gray-900 dark:hover:text-white" onClick={() => navigate('/profile')}>
                            {user?.name || user?.email || 'User'}
                        </span>
                    </div>

                    <div className="h-5 w-px bg-gray-200 dark:bg-gray-700 mx-1"></div>

                    <button
                        onClick={logout}
                        title="Sign Out"
                        className="p-2 text-gray-500 dark:text-gray-400 hover:text-red-600 dark:hover:text-red-400 hover:bg-red-50 dark:hover:bg-red-500/10 rounded-full transition-colors flex items-center justify-center"
                    >
                        <LogOut size={18} />
                    </button>
                </div>
            </div>
        </header>
    );
};
