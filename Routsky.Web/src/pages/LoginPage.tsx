import { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { useTheme } from '../context/ThemeContext';
import { login as apiLogin } from '../api/routskyApi';
import { Input } from '../components/ui/Input';
import { motion } from 'framer-motion';
import { Link, useNavigate } from 'react-router-dom';
import { LogIn, AlertCircle, Sun, Moon } from 'lucide-react';
import { AuthGlobeFlip } from '../components/AuthGlobeFlip';

export const LoginPage = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    const { login } = useAuth();
    const { theme, toggleTheme } = useTheme();
    const navigate = useNavigate();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');
        setLoading(true);

        try {
            const response = await apiLogin({ email, password });
            login(response.token, {
                id: response.id,
                email: response.email,
                name: response.name,
                role: response.role,
                passports: response.passports || [],
                avatarUrl: response.avatarUrl || response.AvatarUrl,
                origin: response.origin || "",
                preferredCurrency: response.preferredCurrency || "USD",
                unitPreference: response.unitPreference || "Metric",
                travelStyle: response.travelStyle || "Comfort",
                notificationsEnabled: response.notificationsEnabled ?? true,
                priceAlertsEnabled: response.priceAlertsEnabled ?? true
            });
            navigate('/');
        } catch (err: any) {
            setError(err.response?.data?.message || 'Failed to login');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="min-h-screen relative flex items-center justify-center bg-[#020308] overflow-hidden">
            {/* Background Globe 20% Opacity + Blur */}
            <div className="absolute inset-0 z-0 pointer-events-none opacity-20">
                 <AuthGlobeFlip />
            </div>
            <div className="absolute inset-0 z-0 pointer-events-none backdrop-blur-[8px]" />

            {/* Theme Toggle */}
            <button
                onClick={toggleTheme}
                className="absolute top-4 right-4 z-50 p-2 rounded-full bg-white/10 hover:bg-white/20 backdrop-blur-md border border-white/10 transition-all text-white/50 hover:text-white"
            >
                {theme === 'dark' ? <Sun size={20} /> : <Moon size={20} />}
            </button>

            {/* Central Glassmorphism Card */}
            <motion.div
                initial={{ opacity: 0, scale: 0.95, y: 10 }}
                animate={{ opacity: 1, scale: 1, y: 0 }}
                transition={{ duration: 0.4, ease: [0.4, 0, 0.2, 1] }}
                className="relative z-10 w-full max-w-[450px] mx-4 p-8 sm:p-10 rounded-3xl bg-white/5 dark:bg-[#060810]/60 backdrop-blur-2xl border border-white/10 shadow-[0_0_40px_rgba(0,166,126,0.1)]"
            >
                <div className="flex flex-col items-center mb-10 text-center">
                    <img
                        src="/assets/logo.png"
                        alt="Routsky"
                        className="h-12 w-auto object-contain drop-shadow-[0_0_15px_rgba(0,166,126,0.4)] mb-6"
                    />
                    <h1 className="text-2xl sm:text-3xl font-bold tracking-tight text-gray-900 dark:text-white mb-2">
                        Routsky <span className="text-[#00A67E]">STAGING - VERIFIED V2</span>
                    </h1>
                    <p className="text-sm text-gray-500 dark:text-gray-400">Sign in to continue your journey.</p>
                </div>

                {error && (
                    <div className="bg-red-500/10 border border-red-500/20 rounded-xl p-3 mb-6 flex items-center gap-2.5 text-red-500 text-sm font-medium">
                        <AlertCircle size={16} />
                        {error}
                    </div>
                )}

                <form onSubmit={handleSubmit} className="space-y-5">
                    <div className="space-y-1.5 group">
                        <label className="text-[10px] uppercase tracking-[0.25em] font-medium text-gray-500 dark:text-gray-400">Email</label>
                        <Input
                            type="email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            required
                            placeholder="you@example.com"
                            className="h-12 bg-white/50 dark:bg-white/5 border-gray-200 dark:border-white/10 focus:!border-[#00A67E]/50 rounded-xl"
                        />
                    </div>
                    <div className="space-y-1.5 group">
                        <div className="flex justify-between items-center">
                            <label className="text-[10px] uppercase tracking-[0.25em] font-medium text-gray-500 dark:text-gray-400">Password</label>
                            <a href="#" className="text-[10px] uppercase font-semibold text-[#00A67E] hover:text-[#00A67E]/80 transition-colors">Forgot?</a>
                        </div>
                        <Input
                            type="password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            required
                            placeholder="••••••••"
                             className="h-12 bg-white/50 dark:bg-white/5 border-gray-200 dark:border-white/10 focus:!border-[#00A67E]/50 rounded-xl"
                        />
                    </div>

                    <button
                        type="submit"
                        className="w-full h-12 mt-2 rounded-xl font-bold tracking-wide text-sm transition-all duration-300 text-white flex justify-center items-center gap-2 hover:shadow-[0_0_20px_rgba(0,166,126,0.3)] disabled:opacity-50 disabled:hover:shadow-none"
                        style={{ backgroundColor: '#00A67E' }}
                        disabled={loading}
                    >
                        {loading ? 'Signing in...' : 'Sign In'} <LogIn size={16}/>
                    </button>
                </form>

                <p className="mt-8 text-center text-sm font-medium text-gray-500 dark:text-gray-400">
                    Don't have an account?{' '}
                    <Link to="/register" className="text-white hover:text-[#00A67E] transition-colors">
                        Create one →
                    </Link>
                </p>
            </motion.div>
        </div>
    );
};
