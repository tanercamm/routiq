import { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { register as apiRegister } from '../api/routiqApi';
import { Button } from '../components/ui/Button';
import { Input } from '../components/ui/Input';
import { motion } from 'framer-motion';
import { Link, useNavigate } from 'react-router-dom';
import { UserPlus, AlertCircle, ArrowRight } from 'lucide-react';

export const RegisterPage = () => {
    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
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
        <div className="min-h-screen grid lg:grid-cols-2 bg-[#0f172a]">
            {/* Left Column - Image & Branding */}
            <div className="relative hidden lg:block overflow-hidden order-1 lg:order-2">
                <div className="absolute inset-0 bg-cover bg-center transition-transform duration-[20s] hover:scale-105"
                    style={{ backgroundImage: "url('https://images.unsplash.com/photo-1502791451862-7bd8c1df43a7?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=1964&q=80')" }}
                />
                <div className="absolute inset-0 bg-gradient-to-t from-slate-900 via-slate-900/40 to-transparent" />

                <div className="absolute bottom-0 left-0 p-16 text-white z-10">
                    <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ delay: 0.2 }}
                    >
                        <h2 className="text-5xl font-bold mb-6 leading-tight">
                            Start Your Journey<br />
                            <span className="text-purple-400">With Confidence.</span>
                        </h2>
                        <p className="text-xl text-slate-300 max-w-md">
                            Create an account to save your favorite routes, track your budget, and explore the world efficiently.
                        </p>
                    </motion.div>
                </div>
            </div>


            {/* Right Column - Form */}
            <div className="flex items-center justify-center p-8 lg:p-16 relative order-2 lg:order-1">
                <div className="absolute top-0 left-0 p-8">
                    <Link to="/" className="text-slate-400 hover:text-white transition-colors flex items-center gap-2 group">
                        <ArrowRight size={16} className="group-hover:-translate-x-1 transition-transform rotate-180" /> Back to Home
                    </Link>
                </div>

                <motion.div
                    initial={{ opacity: 0, x: -20 }}
                    animate={{ opacity: 1, x: 0 }}
                    className="w-full max-w-md"
                >
                    <div className="mb-10">
                        <div className="w-12 h-12 bg-purple-500/10 rounded-xl flex items-center justify-center mb-6 text-purple-400">
                            <UserPlus size={24} />
                        </div>
                        <h1 className="text-3xl font-bold text-white mb-2">Create an account</h1>
                        <p className="text-slate-400">Enter your information to get started.</p>
                    </div>

                    {error && (
                        <div className="bg-red-500/10 border border-red-500/20 rounded-lg p-4 mb-6 flex items-center gap-3 text-red-400 text-sm">
                            <AlertCircle size={18} />
                            {error}
                        </div>
                    )}

                    <form onSubmit={handleSubmit} className="space-y-5">
                        <div className="grid grid-cols-2 gap-4">
                            <Input
                                label="First Name"
                                value={firstName}
                                onChange={(e) => setFirstName(e.target.value)}
                                placeholder="John"
                                className="bg-slate-800/50 border-slate-700 focus:border-purple-500 focus:ring-purple-500/20 h-11"
                            />
                            <Input
                                label="Last Name"
                                value={lastName}
                                onChange={(e) => setLastName(e.target.value)}
                                placeholder="Doe"
                                className="bg-slate-800/50 border-slate-700 focus:border-purple-500 focus:ring-purple-500/20 h-11"
                            />
                        </div>
                        <Input
                            label="Email"
                            type="email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            required
                            placeholder="name@example.com"
                            className="bg-slate-800/50 border-slate-700 focus:border-purple-500 focus:ring-purple-500/20 h-11"
                        />
                        <Input
                            label="Password"
                            type="password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            required
                            placeholder="Create a password"
                            minLength={6}
                            className="bg-slate-800/50 border-slate-700 focus:border-purple-500 focus:ring-purple-500/20 h-11"
                        />

                        <Button
                            variant="primary"
                            className="w-full h-11 bg-purple-600 hover:bg-purple-500 transition-colors"
                            disabled={loading}
                        >
                            {loading ? 'Creating account...' : 'Create account'}
                        </Button>
                    </form>

                    <div className="mt-8 text-center text-sm text-slate-400">
                        Already have an account?{' '}
                        <Link to="/login" className="text-purple-400 hover:text-purple-300 font-medium">
                            Log in
                        </Link>
                    </div>
                </motion.div>
            </div>
        </div>
    );
};
