import { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { useTheme } from '../context/ThemeContext';
import { register as apiRegister } from '../api/routiqApi';
import { Button } from '../components/ui/Button';
import { Input } from '../components/ui/Input';
import { motion } from 'framer-motion';
import { Link, useNavigate } from 'react-router-dom';
import { UserPlus, AlertCircle, Sun, Moon } from 'lucide-react';

export const RegisterPage = () => {
    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
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
            const response = await apiRegister({ email, password, firstName, lastName });
            login(response.token, {
                email: response.email,
                name: response.name,
                role: response.role
            });
            navigate('/');
        } catch (err: any) {
            setError(err.response?.data?.message || 'Failed to register');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="min-h-screen grid lg:grid-cols-2 bg-gray-50 dark:bg-gray-900">
            {/* Theme Toggle */}
            <button
                onClick={toggleTheme}
                className="absolute top-4 right-4 z-50 p-2 rounded-full bg-white/80 dark:bg-gray-800/80 backdrop-blur-sm border border-gray-200 dark:border-gray-700 shadow-sm hover:shadow-md transition-all text-gray-500 dark:text-gray-400 hover:text-purple-600 dark:hover:text-purple-400"
            >
                {theme === 'dark' ? <Sun size={20} /> : <Moon size={20} />}
            </button>

            {/* Left Column — Form */}
            <div className="flex items-center justify-center p-8 lg:p-16 order-2 lg:order-1">
                <motion.div
                    initial={{ opacity: 0, x: -20 }}
                    animate={{ opacity: 1, x: 0 }}
                    className="w-full max-w-sm"
                >
                    <div className="mb-8">
                        <div className="w-10 h-10 bg-purple-50 dark:bg-purple-500/10 rounded-lg flex items-center justify-center mb-5">
                            <UserPlus size={20} className="text-purple-600 dark:text-purple-400" />
                        </div>
                        <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-1">Create an account</h1>
                        <p className="text-sm text-gray-500 dark:text-gray-400">Enter your information to get started.</p>
                    </div>

                    {error && (
                        <div className="bg-red-50 dark:bg-red-500/10 border border-red-200 dark:border-red-500/20 rounded-lg p-3 mb-5 flex items-center gap-2.5 text-red-600 dark:text-red-400 text-sm">
                            <AlertCircle size={16} />
                            {error}
                        </div>
                    )}

                    <form onSubmit={handleSubmit} className="space-y-4">
                        <div className="grid grid-cols-2 gap-3">
                            <Input
                                label="First Name"
                                value={firstName}
                                onChange={(e) => setFirstName(e.target.value)}
                                placeholder="John"
                            />
                            <Input
                                label="Last Name"
                                value={lastName}
                                onChange={(e) => setLastName(e.target.value)}
                                placeholder="Doe"
                            />
                        </div>
                        <Input
                            label="Email"
                            type="email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            required
                            placeholder="you@example.com"
                        />
                        <Input
                            label="Password"
                            type="password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            required
                            placeholder="Create a password"
                            minLength={6}
                        />

                        <Button
                            variant="primary"
                            className="w-full bg-purple-600 hover:bg-purple-500"
                            disabled={loading}
                        >
                            {loading ? 'Creating account...' : 'Create account'}
                        </Button>
                    </form>

                    <p className="mt-6 text-center text-sm text-gray-500 dark:text-gray-400">
                        Already have an account?{' '}
                        <Link to="/login" className="text-purple-600 dark:text-purple-400 font-medium hover:underline">
                            Log in
                        </Link>
                    </p>
                </motion.div>
            </div>

            {/* Right Column — Branding */}
            <div className="relative hidden lg:flex flex-col justify-end p-16 bg-gray-900 dark:bg-gray-950 order-1 lg:order-2">
                <div className="absolute inset-0 bg-gradient-to-br from-purple-600/20 to-pink-600/10" />
                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: 0.2 }}
                    className="relative z-10"
                >
                    <h2 className="text-4xl font-bold text-white mb-4 leading-tight">
                        Start Your Journey<br />
                        <span className="text-purple-400">With Confidence.</span>
                    </h2>
                    <p className="text-lg text-gray-400 max-w-md">
                        Create an account to save routes, track budgets, and explore the world efficiently.
                    </p>
                </motion.div>
            </div>
        </div>
    );
};
