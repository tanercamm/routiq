import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { useTheme } from '../context/ThemeContext';
import { login as apiLogin, register as apiRegister } from '../api/routskyApi';
import { Button } from './ui/Button';
import { Input } from './ui/Input';
import { motion, AnimatePresence } from 'framer-motion';
import { useNavigate, useLocation } from 'react-router-dom';
import { LogIn, UserPlus, AlertCircle, Sun, Moon, Globe, ChevronLeft } from 'lucide-react';
import { GlobePathLogo } from './GlobePathLogo';
import { PASSPORT_CODES } from '../constants/passports';

// ── Account step for Register (step 1) ───────────────────────────────────────
function AccountStep({
    firstName, setFirstName, lastName, setLastName,
    email, setEmail, password, setPassword, onNext,
}: {
    firstName: string; setFirstName: (v: string) => void;
    lastName: string; setLastName: (v: string) => void;
    email: string; setEmail: (v: string) => void;
    password: string; setPassword: (v: string) => void;
    onNext: () => void;
}) {
    return (
        <>
            <div className="grid grid-cols-2 gap-3">
                <Input label="First Name" value={firstName} onChange={e => setFirstName(e.target.value)} placeholder="John" />
                <Input label="Last Name" value={lastName} onChange={e => setLastName(e.target.value)} placeholder="Doe" />
            </div>
            <Input label="Email" type="email" value={email} onChange={e => setEmail(e.target.value)} required placeholder="you@example.com" />
            <Input label="Password" type="password" value={password} onChange={e => setPassword(e.target.value)} required placeholder="Create a password" minLength={6} />
            <Button
                variant="primary"
                className="w-full bg-purple-600 hover:bg-purple-500"
                onClick={e => { e.preventDefault(); if (firstName && email && password) onNext(); }}
            >
                Continue →
            </Button>
        </>
    );
}

// ── Citizenship step for Register (step 2) ───────────────────────────────────
function CitizenshipStep({
    passports, setPassports,
}: {
    passports: string[];
    setPassports: (v: string[]) => void;
}) {
    return (
        <div className="space-y-4">
            <div>
                <label className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-2 block">
                    Your Passport(s)
                </label>
                <p className="text-xs text-gray-400 dark:text-gray-500 mb-3 leading-relaxed">
                    Select all citizenships you hold. The route engine will always evaluate your best-case visa outcome.
                </p>
                <div className="flex flex-wrap gap-1.5 mb-3 min-h-[2.25rem]">
                    {passports.length === 0 && (
                        <span className="text-xs text-gray-400 italic">No passports selected yet — add one below.</span>
                    )}
                    {Array.isArray(passports) && passports.map(code => {
                        if (!code) return null;
                        const opt = PASSPORT_CODES.find(o => o.code === code);
                        return (
                            <span key={code} className="inline-flex items-center gap-1 bg-purple-50 dark:bg-purple-500/10 border border-purple-200 dark:border-purple-500/30 text-purple-700 dark:text-purple-300 text-xs font-semibold px-2.5 py-1 rounded-full">
                                {opt?.label ?? code}
                                <button
                                    type="button"
                                    onClick={() => setPassports(passports.filter(p => p !== code))}
                                    className="ml-0.5 hover:text-red-500 transition-colors"
                                    aria-label={`Remove ${code}`}
                                >
                                    ×
                                </button>
                            </span>
                        );
                    })}
                </div>
                <select
                    value=""
                    onChange={e => { const v = e.target.value; if (v && Array.isArray(passports) && !passports.includes(v)) setPassports([...passports, v]); }}
                    className="w-full bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg px-3 py-2 text-sm text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-purple-500/40 transition-colors"
                >
                    <option value="">+ Add a passport country...</option>
                    {Array.isArray(passports) && PASSPORT_CODES.filter(o => !passports.includes(o.code)).map(o => (
                        <option key={o.code} value={o.code}>{o.label}</option>
                    ))}
                </select>
            </div>
        </div>
    );
}

// ── AuthGlobeFlip: 3D flip card — Login (green) front, Register (purple) back ───
export const AuthGlobeFlip = () => {
    const { login } = useAuth();
    const { theme, toggleTheme } = useTheme();
    const navigate = useNavigate();
    const location = useLocation();

    const isRegisterPath = location.pathname === '/register';
    const [flipped, setFlipped] = useState(isRegisterPath);

    useEffect(() => {
        setFlipped(isRegisterPath);
    }, [isRegisterPath]);

    // Login state
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [loginError, setLoginError] = useState('');
    const [loginLoading, setLoginLoading] = useState(false);

    // Register state
    const [step, setStep] = useState<1 | 2>(1);
    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [regEmail, setRegEmail] = useState('');
    const [regPassword, setRegPassword] = useState('');
    const [passports, setPassports] = useState<string[]>([]);
    const [regError, setRegError] = useState('');
    const [regLoading, setRegLoading] = useState(false);

    const handleLoginSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoginError('');
        setLoginLoading(true);
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
            setLoginError(err.response?.data?.message || 'Failed to login');
        } finally {
            setLoginLoading(false);
        }
    };

    const handleRegisterSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (step === 1) { setStep(2); return; }
        if (passports.length === 0) { setRegError('Please add at least one passport.'); return; }

        setRegError('');
        setRegLoading(true);
        try {
            const response = await apiRegister({ email: regEmail, password: regPassword, firstName, lastName, passports });
            login(response.token, {
                id: response.id,
                email: response.email,
                name: response.name,
                role: response.role,
                passports,
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
            setRegError(err.response?.data?.message || 'Failed to register');
        } finally {
            setRegLoading(false);
        }
    };

    const goToRegister = () => {
        setFlipped(true);
        navigate('/register');
    };

    const goToLogin = () => {
        setFlipped(false);
        navigate('/login');
    };

    return (
        <div className="min-h-screen flex flex-col items-center justify-center p-6 bg-gray-50 dark:bg-gray-900 relative">
            <div className="absolute top-6 left-1/2 -translate-x-1/2 flex items-center gap-2">
                <GlobePathLogo size={32} className="text-teal-600 dark:text-teal-400" />
                <span className="text-xl font-bold text-gray-900 dark:text-white">Routsky</span>
            </div>
            <button
                onClick={toggleTheme}
                className="absolute top-4 right-4 z-50 p-2 rounded-full bg-white/80 dark:bg-gray-800/80 backdrop-blur-sm border border-gray-200 dark:border-gray-700 transition-all text-gray-500 dark:text-gray-400 hover:text-teal-600 dark:hover:text-teal-400"
            >
                {theme === 'dark' ? <Sun size={20} /> : <Moon size={20} />}
            </button>

            <div className="w-full max-w-md perspective-1000">
                <motion.div
                    className="relative w-full h-[520px]"
                    style={{ perspective: '1000px' }}
                >
                    <motion.div
                        className="relative w-full h-full"
                        style={{ transformStyle: 'preserve-3d' }}
                        animate={{ rotateY: flipped ? 180 : 0 }}
                        transition={{ duration: 0.6, ease: [0.23, 1, 0.32, 1] }}
                    >
                        {/* Front: Login (Green theme) */}
                        <div
                            className="absolute inset-0 w-full h-full rounded-2xl overflow-hidden"
                            style={{ backfaceVisibility: 'hidden', transform: 'rotateY(0deg)' }}
                        >
                            <div className="h-full bg-white dark:bg-gray-900 rounded-2xl border border-teal-200/50 dark:border-teal-500/20 shadow-xl flex flex-col">
                                <div className="flex-1 flex flex-col justify-center p-8">
                                    <div className="mb-6">
                                        <div className="w-10 h-10 bg-teal-50 dark:bg-teal-500/10 rounded-lg flex items-center justify-center mb-5">
                                            <LogIn size={20} className="text-teal-600 dark:text-teal-400" />
                                        </div>
                                        <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-1">Welcome back</h1>
                                        <p className="text-sm text-gray-500 dark:text-gray-400">Enter your details to sign in.</p>
                                    </div>

                                    {loginError && (
                                        <div className="bg-red-50 dark:bg-red-500/10 border border-red-200 dark:border-red-500/20 rounded-lg p-3 mb-5 flex items-center gap-2.5 text-red-600 dark:text-red-400 text-sm">
                                            <AlertCircle size={16} />
                                            {loginError}
                                        </div>
                                    )}

                                    <form onSubmit={handleLoginSubmit} className="space-y-4">
                                        <Input
                                            label="Email"
                                            type="email"
                                            value={email}
                                            onChange={(e) => setEmail(e.target.value)}
                                            required
                                            placeholder="you@example.com"
                                        />
                                        <div>
                                            <Input
                                                label="Password"
                                                type="password"
                                                value={password}
                                                onChange={(e) => setPassword(e.target.value)}
                                                required
                                                placeholder="Enter your password"
                                            />
                                            <div className="flex justify-end mt-1.5">
                                                <a href="#" className="text-xs text-teal-600 dark:text-teal-400 hover:underline">Forgot password?</a>
                                            </div>
                                        </div>

                                        <Button
                                            variant="primary"
                                            className="w-full bg-teal-600 hover:bg-teal-500"
                                            disabled={loginLoading}
                                        >
                                            {loginLoading ? 'Signing in...' : 'Sign in'}
                                        </Button>
                                    </form>

                                    <p className="mt-6 text-center text-sm text-gray-500 dark:text-gray-400">
                                        Don't have an account?{' '}
                                        <button type="button" onClick={goToRegister} className="text-teal-600 dark:text-teal-400 font-medium hover:underline">
                                            Sign up
                                        </button>
                                    </p>
                                </div>
                            </div>
                        </div>

                        {/* Back: Register (Purple theme) */}
                        <div
                            className="absolute inset-0 w-full h-full rounded-2xl overflow-hidden"
                            style={{ backfaceVisibility: 'hidden', transform: 'rotateY(180deg)' }}
                        >
                            <div className="h-full bg-white dark:bg-gray-900 rounded-2xl border border-purple-200/50 dark:border-purple-500/20 shadow-xl flex flex-col">
                                <div className="flex-1 flex flex-col justify-center p-8 overflow-y-auto">
                                    <div className="mb-5">
                                        <div className="w-9 h-9 bg-purple-50 dark:bg-purple-500/10 rounded-lg flex items-center justify-center mb-4">
                                            {step === 1
                                                ? <UserPlus size={18} className="text-purple-600 dark:text-purple-400" />
                                                : <Globe size={18} className="text-purple-600 dark:text-purple-400" />}
                                        </div>
                                        <h1 className="text-xl font-bold text-gray-900 dark:text-white mb-0.5">
                                            {step === 1 ? 'Create an account' : 'Set your citizenship'}
                                        </h1>
                                        <p className="text-xs text-gray-500 dark:text-gray-400">
                                            {step === 1 ? 'Step 1 of 2 — Account details' : 'Step 2 of 2 — Which passports do you hold?'}
                                        </p>
                                    </div>

                                    <div className="flex gap-1.5 mb-4">
                                        <div className="h-1 flex-1 rounded-full bg-purple-500" />
                                        <div className={`h-1 flex-1 rounded-full transition-colors ${step === 2 ? 'bg-purple-500' : 'bg-gray-200 dark:bg-gray-700'}`} />
                                    </div>

                                    {regError && (
                                        <div className="bg-red-50 dark:bg-red-500/10 border border-red-200 dark:border-red-500/20 rounded-lg p-3 mb-4 flex items-center gap-2 text-red-600 dark:text-red-400 text-xs">
                                            <AlertCircle size={14} /> {regError}
                                        </div>
                                    )}

                                    <form onSubmit={handleRegisterSubmit} className="space-y-3">
                                        <AnimatePresence mode="wait">
                                            {step === 1 ? (
                                                <motion.div key="step1" initial={{ opacity: 0, x: -10 }} animate={{ opacity: 1, x: 0 }} exit={{ opacity: 0, x: 10 }} className="space-y-3">
                                                    <AccountStep
                                                        firstName={firstName} setFirstName={setFirstName}
                                                        lastName={lastName} setLastName={setLastName}
                                                        email={regEmail} setEmail={setRegEmail}
                                                        password={regPassword} setPassword={setRegPassword}
                                                        onNext={() => setStep(2)}
                                                    />
                                                </motion.div>
                                            ) : (
                                                <motion.div key="step2" initial={{ opacity: 0, x: 10 }} animate={{ opacity: 1, x: 0 }} exit={{ opacity: 0, x: -10 }} className="space-y-3">
                                                    <CitizenshipStep passports={passports} setPassports={setPassports} />
                                                    <div className="flex gap-2 pt-1">
                                                        <button
                                                            type="button"
                                                            onClick={() => setStep(1)}
                                                            className="flex items-center gap-1 text-xs text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200 transition-colors px-3 py-2 rounded-lg border border-gray-200 dark:border-gray-700"
                                                        >
                                                            <ChevronLeft size={13} /> Back
                                                        </button>
                                                        <Button
                                                            variant="primary"
                                                            className="flex-1 bg-purple-600 hover:bg-purple-500"
                                                            disabled={regLoading || passports.length === 0}
                                                        >
                                                            {regLoading ? 'Creating account...' : 'Create account'}
                                                        </Button>
                                                    </div>
                                                </motion.div>
                                            )}
                                        </AnimatePresence>
                                    </form>

                                    <p className="mt-5 text-center text-xs text-gray-500 dark:text-gray-400">
                                        Already have an account?{' '}
                                        <button type="button" onClick={goToLogin} className="text-purple-600 dark:text-purple-400 font-medium hover:underline">Log in</button>
                                    </p>
                                </div>
                            </div>
                        </div>
                    </motion.div>
                </motion.div>
            </div>
        </div>
    );
};
