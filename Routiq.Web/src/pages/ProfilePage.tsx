import { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useAuth } from '../context/AuthContext';
import { Card } from '../components/ui/Card';
import { Button } from '../components/ui/Button';
import { PASSPORT_CODES } from '../constants/passports';
import ReactCountryFlag from 'react-country-flag';
import {
    User, Plane, Globe, Settings, ChevronDown, ChevronUp, CheckCircle2
} from 'lucide-react';

// ── Constants ─────────────────────────────────────────────────────────────────
import { countryNames } from '../utils/countryMapper';

const CURRENCIES = ['USD', 'EUR', 'GBP', 'TRY', 'JPY', 'THB', 'CAD', 'AUD', 'PLN'];

// ── Component ─────────────────────────────────────────────────────────────────

export const ProfilePage = () => {
    const { user, updatePassports } = useAuth();

    // Citizenship is driven by AuthContext — this is the ONLY place to edit it
    const [passports, setPassports] = useState<string[]>(Array.isArray(user?.passports) ? user.passports : ['TR']);
    const [preferredCurrency, setPreferredCurrency] = useState('USD');
    const [prefsSaved, setPrefsSaved] = useState(false);
    const [isPreferencesOpen, setIsPreferencesOpen] = useState(false);
    const [savedTrips, setSavedTrips] = useState<any[]>([]);

    // Keep local passport state in sync if context loads late
    useEffect(() => {
        if (Array.isArray(user?.passports) && user.passports.length) setPassports(user.passports);
    }, [user?.passports]);

    const handleSavePrefs = () => {
        updatePassports(passports);
        setPrefsSaved(true);
        setTimeout(() => setPrefsSaved(false), 2500);
    };

    useEffect(() => {
        const fetchSavedTrips = async () => {
            try {
                const userId = 1; // TODO: resolve from JWT
                const response = await fetch(`http://localhost:5107/api/routes/user/${userId}`);
                if (response.ok) setSavedTrips(await response.json());
            } catch (err) {
                console.error('Failed to fetch saved trips', err);
            }
        };
        fetchSavedTrips();
    }, []);

    const activeTrip = savedTrips.find((t: any) => t.status === 'Active');

    const handleSetActive = async (routeId: string) => {
        setSavedTrips(prev => prev.map(t => ({ ...t, status: t.id === routeId ? 'Active' : 'Saved' })));
        try {
            await fetch(`http://localhost:5107/api/routes/${routeId}/set-active`, { method: 'PUT' });
        } catch {
            setSavedTrips(prev => prev.map(t => ({ ...t, status: 'Saved' })));
        }
    };

    return (
        <div className="min-h-screen">
            <main className="max-w-5xl mx-auto px-4 sm:px-6 py-6">
                <div className="space-y-4">

                    {/* ── Profile Header ─────────────────────────────────────── */}
                    <motion.div initial={{ opacity: 0, y: -8 }} animate={{ opacity: 1, y: 0 }}>
                        <Card>
                            <div className="flex items-center gap-4">
                                {/* Avatar */}
                                <div className="w-12 h-12 rounded-full bg-gradient-to-br from-teal-400 to-blue-500 flex items-center justify-center shrink-0">
                                    <User size={20} className="text-white" />
                                </div>
                                {/* Info */}
                                <div className="flex-1 min-w-0">
                                    <h1 className="text-base font-bold text-gray-900 dark:text-white leading-tight">{user?.name ?? 'Traveler'}</h1>
                                    <p className="text-xs text-gray-500 dark:text-gray-400 truncate">{user?.email}</p>
                                    {/* Citizenship + currency badges */}
                                    <div className="flex flex-wrap gap-1.5 mt-1.5">
                                        <span className="inline-flex items-center gap-1.5 text-[11px] border border-gray-200 dark:border-gray-700 rounded-full px-2 py-0.5 text-gray-600 dark:text-gray-300">
                                            <Globe size={10} className="text-teal-500" />
                                            {(!user || !user.passports) ? (
                                                <span className="text-xs text-gray-400 italic">...</span>
                                            ) : Array.isArray(passports) && passports.map(c => {
                                                if (!c) return null;
                                                const name = countryNames[c] || c;
                                                return (
                                                    <span key={c} className="inline-flex items-center justify-center gap-1.5">
                                                        <ReactCountryFlag countryCode={c} svg style={{ width: '1.2em', height: '1.2em', borderRadius: '2px', display: 'flex', alignItems: 'center' }} title={name} />
                                                        <span className="mt-0.5">{name}</span>
                                                    </span>
                                                );
                                            })}
                                        </span>
                                        <span className="inline-flex items-center gap-1 text-[11px] border border-gray-200 dark:border-gray-700 rounded-full px-2 py-0.5 text-gray-600 dark:text-gray-300">
                                            💰 {preferredCurrency}
                                        </span>
                                    </div>
                                </div>
                                {/* Active trip banner (right side) */}
                                {activeTrip && (
                                    <div className="shrink-0 flex items-center gap-1.5 bg-emerald-50 dark:bg-emerald-900/20 border border-emerald-200 dark:border-emerald-500/30 rounded-lg px-3 py-1.5">
                                        <span className="text-sm">📍</span>
                                        <span className="text-xs font-semibold text-emerald-700 dark:text-emerald-300">
                                            {activeTrip.routeName ?? 'Active Trip'}
                                        </span>
                                    </div>
                                )}
                            </div>
                        </Card>
                    </motion.div>

                    {/* ── Preferences (Citizenship editing lives HERE) ─────── */}
                    <motion.div initial={{ opacity: 0, x: -8 }} animate={{ opacity: 1, x: 0 }} transition={{ delay: 0.1 }}>
                        <Card className="overflow-hidden">
                            <button
                                onClick={() => setIsPreferencesOpen(!isPreferencesOpen)}
                                className="w-full flex items-center justify-between hover:bg-gray-50 dark:hover:bg-gray-800/50 rounded-lg transition-colors"
                            >
                                <div className="flex items-center gap-2">
                                    <Settings size={15} className="text-gray-500 dark:text-gray-400" />
                                    <span className="text-sm font-semibold text-gray-900 dark:text-white">Preferences</span>
                                </div>
                                {isPreferencesOpen ? <ChevronUp size={15} className="text-gray-400" /> : <ChevronDown size={15} className="text-gray-400" />}
                            </button>

                            <AnimatePresence>
                                {isPreferencesOpen && (
                                    <motion.div
                                        initial={{ height: 0, opacity: 0 }}
                                        animate={{ height: 'auto', opacity: 1 }}
                                        exit={{ height: 0, opacity: 0 }}
                                        transition={{ duration: 0.25 }}
                                        className="overflow-hidden"
                                    >
                                        <div className="pt-4 grid md:grid-cols-2 gap-4">
                                            {/* Read-only identity */}
                                            <div className="space-y-3">
                                                <div>
                                                    <label className="text-[11px] font-medium text-gray-500 dark:text-gray-400 mb-1 block uppercase tracking-wide">Name</label>
                                                    <div className="bg-gray-50 dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg px-3 py-2 text-sm text-gray-600 dark:text-gray-300 cursor-not-allowed">
                                                        {user?.name ?? '—'}
                                                    </div>
                                                </div>
                                                <div>
                                                    <label className="text-[11px] font-medium text-gray-500 dark:text-gray-400 mb-1 block uppercase tracking-wide">Email</label>
                                                    <div className="bg-gray-50 dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg px-3 py-2 text-sm text-gray-600 dark:text-gray-300 cursor-not-allowed">
                                                        {user?.email ?? '—'}
                                                    </div>
                                                </div>
                                            </div>

                                            {/* Editable citizenship + currency */}
                                            <div className="space-y-3">
                                                {/* Citizenship — the canonical edit surface */}
                                                <div>
                                                    <label className="text-[11px] font-medium text-gray-700 dark:text-gray-300 mb-1 block uppercase tracking-wide">Citizenships</label>
                                                    <div className="flex flex-wrap gap-1.5 mb-2">
                                                        {(!user || !user.passports) ? (
                                                            <span className="text-xs text-gray-400 italic">Loading passports...</span>
                                                        ) : Array.isArray(passports) && passports.map(code => {
                                                            if (!code) return null;
                                                            const name = countryNames[code] || code;
                                                            return (
                                                                <span key={code} className="inline-flex items-center justify-center gap-1.5 bg-teal-50 dark:bg-teal-500/10 border border-teal-200 dark:border-teal-500/30 text-teal-700 dark:text-teal-300 text-xs font-semibold px-2 py-0.5 rounded-full">
                                                                    <ReactCountryFlag countryCode={code} svg style={{ width: '1.2em', height: '1.2em', borderRadius: '2px', display: 'flex', alignItems: 'center' }} title={name} />
                                                                    <span className="mt-0.5">{name}</span>
                                                                    {passports.length > 1 && (
                                                                        <button type="button" onClick={() => setPassports(prev => prev.filter(p => p !== code))} className="ml-1 hover:text-red-500 transition-colors" aria-label={`Remove ${name}`}>×</button>
                                                                    )}
                                                                </span>
                                                            );
                                                        })}
                                                    </div>
                                                    <select
                                                        value=""
                                                        onChange={e => { const v = e.target.value; if (v && Array.isArray(passports) && !passports.includes(v)) setPassports(prev => [...prev, v]); }}
                                                        className="w-full bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg px-3 py-2 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-teal-500/40 transition-colors"
                                                    >
                                                        <option value="">+ Add citizenship...</option>
                                                        {Array.isArray(passports) && PASSPORT_CODES.filter(o => !passports.includes(o.code)).map(o => (
                                                            <option key={o.code} value={o.code}>{o.code} — {o.label.replace(/^[^ ]+ /, '')}</option>
                                                        ))}
                                                    </select>
                                                </div>

                                                {/* Currency */}
                                                <div>
                                                    <label className="text-[11px] font-medium text-gray-700 dark:text-gray-300 mb-1 block uppercase tracking-wide">Preferred Currency</label>
                                                    <select
                                                        value={preferredCurrency}
                                                        onChange={e => setPreferredCurrency(e.target.value)}
                                                        className="w-full bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg px-3 py-2 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-teal-500/40 transition-colors"
                                                    >
                                                        {CURRENCIES.map(c => <option key={c} value={c}>{c}</option>)}
                                                    </select>
                                                </div>
                                            </div>
                                        </div>

                                        <div className="mt-4 flex justify-end">
                                            <Button onClick={handleSavePrefs} className="text-sm px-4 py-2 flex items-center gap-2">
                                                {prefsSaved ? <><CheckCircle2 size={14} /> Saved!</> : 'Save Preferences'}
                                            </Button>
                                        </div>
                                    </motion.div>
                                )}
                            </AnimatePresence>
                        </Card>
                    </motion.div>

                    {/* ── Saved Trips (V2 API + clean) ─────────────────────── */}
                    <motion.div initial={{ opacity: 0, x: 8 }} animate={{ opacity: 1, x: 0 }} transition={{ delay: 0.15 }}>
                        <Card>
                            <div className="flex items-center gap-2 mb-3">
                                <Plane size={15} className="text-blue-500" />
                                <h2 className="text-sm font-semibold text-gray-900 dark:text-white">Saved Trips</h2>
                                {savedTrips.length > 0 && (
                                    <span className="ml-auto text-[11px] text-gray-400">{savedTrips.length} saved</span>
                                )}
                            </div>

                            {savedTrips.length === 0 ? (
                                <div className="py-8 text-center">
                                    <p className="text-sm text-gray-400 dark:text-gray-500">No saved trips yet.</p>
                                    <p className="text-xs text-gray-400 dark:text-gray-600 mt-1">Generate a route and save it to see it here.</p>
                                </div>
                            ) : (
                                <div className="space-y-2">
                                    {savedTrips.map((trip: any) => {
                                        const isActive = trip.status === 'Active';
                                        const stops: any[] = trip.stops ?? [];
                                        return (
                                            <div
                                                key={trip.id}
                                                className={`border rounded-lg p-3 transition-all ${isActive
                                                    ? 'border-emerald-400 dark:border-emerald-500 bg-emerald-50/60 dark:bg-emerald-900/20 ring-1 ring-emerald-300/50 dark:ring-emerald-500/30'
                                                    : 'border-gray-200 dark:border-gray-700/60 hover:bg-gray-50 dark:hover:bg-gray-800/30'
                                                    }`}
                                            >
                                                <div className="flex items-start justify-between gap-2 mb-1.5">
                                                    <div className="flex items-center gap-2 flex-wrap">
                                                        <span className="text-sm font-semibold text-gray-900 dark:text-white">{trip.routeName}</span>
                                                        {isActive ? (
                                                            <span className="text-[10px] bg-emerald-100 dark:bg-emerald-500/20 text-emerald-700 dark:text-emerald-300 border border-emerald-300 dark:border-emerald-500/30 px-1.5 py-0.5 rounded font-semibold animate-pulse">📍 Active</span>
                                                        ) : (
                                                            <span className="text-[10px] bg-gray-100 dark:bg-gray-700 text-gray-500 dark:text-gray-400 px-1.5 py-0.5 rounded">Saved</span>
                                                        )}
                                                    </div>
                                                    <span className="shrink-0 text-[11px] bg-blue-50 dark:bg-blue-500/10 border border-blue-200 dark:border-blue-500/20 text-blue-600 dark:text-blue-300 px-2 py-0.5 rounded-full">
                                                        {trip.durationDays}d
                                                    </span>
                                                </div>

                                                <div className="flex flex-wrap gap-3 text-xs text-gray-500 dark:text-gray-400 mb-2">
                                                    <span>💰 <span className="font-medium text-gray-700 dark:text-gray-200">${(trip.totalBudgetUsd ?? 0).toLocaleString()}</span></span>
                                                    {stops.length > 0 && <span>📍 <span className="font-medium text-gray-700 dark:text-gray-200">{stops.map((s: any) => s.city).join(' → ')}</span></span>}
                                                    {(trip.passports?.length > 0) && (
                                                        <span>🛂 <span className="font-medium text-gray-700 dark:text-gray-200">{trip.passports.join(', ')}</span></span>
                                                    )}
                                                </div>

                                                <button
                                                    onClick={() => handleSetActive(trip.id)}
                                                    disabled={isActive}
                                                    className={`text-[11px] px-2.5 py-1 rounded border transition-colors font-medium ${isActive
                                                        ? 'bg-emerald-100 dark:bg-emerald-500/20 text-emerald-600 dark:text-emerald-400 border-emerald-300 dark:border-emerald-500/30 cursor-default'
                                                        : 'text-gray-500 dark:text-gray-400 border-gray-200 dark:border-gray-600 hover:text-emerald-600 hover:border-emerald-300 hover:bg-emerald-50 dark:hover:bg-emerald-900/20'
                                                        }`}
                                                >
                                                    {isActive ? '✓ Active' : '📍 Set Active'}
                                                </button>
                                            </div>
                                        );
                                    })}
                                </div>
                            )}
                        </Card>
                    </motion.div>

                </div>
            </main>
        </div>
    );
};
