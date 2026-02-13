import { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { login as apiLogin } from '../api/routiqApi';
import { Button } from '../components/ui/Button';
import { Input } from '../components/ui/Input';
import { motion } from 'framer-motion';
import { Link, useNavigate } from 'react-router-dom';
import { LogIn, AlertCircle, ArrowRight } from 'lucide-react';

export const LoginPage = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    const { login } = useAuth();
    const navigate = useNavigate();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');
        setLoading(true);

        try {
            const response = await apiLogin({ email, password });
            login(response.token, {
                email: response.email,
                name: response.name,
                role: response.role
            });
            navigate('/');
        } catch (err: any) {
            setError(err.response?.data?.message || 'Failed to login');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="min-h-screen grid lg:grid-cols-2 bg-[#0f172a]">
            {/* Left Column - Image & Branding */}
            <div className="relative hidden lg:block overflow-hidden">
                <div className="absolute inset-0 bg-cover bg-center transition-transform duration-[20s] hover:scale-105"
                    style={{ backgroundImage: "url('https://images.unsplash.com/photo-1476514525535-07fb3b4ae5f1?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=2070&q=80')" }}
                />
                <div className="absolute inset-0 bg-gradient-to-t from-slate-900 via-slate-900/40 to-transparent" />

                <div className="absolute bottom-0 left-0 p-16 text-white z-10">
                    <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ delay: 0.2 }}
                    >
                        <h2 className="text-5xl font-bold mb-6 leading-tight">
                            Discover the World,<br />
                            <span className="text-teal-400">One Route at a Time.</span>
                        </h2>
                        <p className="text-xl text-slate-300 max-w-md">
                            Join thousands of travelers who are planning their perfect and precise adventures by leveraging AI predictions.
                        </p>
                    </motion.div>
                </div>
            </div>

            {/* Right Column - Form */}
            <div className="flex items-center justify-center p-8 lg:p-16 relative">
                <div className="absolute top-0 right-0 p-8">
                    <Link to="/" className="text-slate-400 hover:text-white transition-colors flex items-center gap-2 group">
                        Back to Home <ArrowRight size={16} className="group-hover:translate-x-1 transition-transform" />
                    </Link>
                </div>

                <motion.div
                    initial={{ opacity: 0, x: 20 }}
                    animate={{ opacity: 1, x: 0 }}
                    className="w-full max-w-md"
                >
                    <div className="mb-10">
                        <div className="w-12 h-12 bg-blue-500/10 rounded-xl flex items-center justify-center mb-6 text-blue-400">
                            <LogIn size={24} />
                        </div>
                        <h1 className="text-3xl font-bold text-white mb-2">Welcome back</h1>
                        <p className="text-slate-400">Please enter your details to sign in.</p>
                    </div>

                    {error && (
                        <div className="bg-red-500/10 border border-red-500/20 rounded-lg p-4 mb-6 flex items-center gap-3 text-red-400 text-sm">
                            <AlertCircle size={18} />
                            {error}
                        </div>
                    )}

                    <form onSubmit={handleSubmit} className="space-y-5">
                        <Input
                            label="Email"
                            type="email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            required
                            placeholder="Enter your email"
                            className="bg-slate-800/50 border-slate-700 focus:border-blue-500 focus:ring-blue-500/20 h-11"
                        />
                        <div>
                            <Input
                                label="Password"
                                type="password"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                required
                                placeholder="Enter your password"
                                className="bg-slate-800/50 border-slate-700 focus:border-blue-500 focus:ring-blue-500/20 h-11"
                            />
                            <div className="flex justify-end mt-2">
                                <a href="#" className="text-sm text-blue-400 hover:text-blue-300">Forgot password?</a>
                            </div>
                        </div>

                        <Button
                            variant="primary"
                            className="w-full h-11 bg-blue-600 hover:bg-blue-500 transition-colors"
                            disabled={loading}
                        >
                            {loading ? 'Signing in...' : 'Sign in'}
                        </Button>
                    </form>

                    <div className="mt-8 text-center text-sm text-slate-400">
                        Don't have an account?{' '}
                        <Link to="/register" className="text-blue-400 hover:text-blue-300 font-medium">
                            Sign up for free
                        </Link>
                    </div>
                </motion.div>
            </div>
        </div>
    );
};
