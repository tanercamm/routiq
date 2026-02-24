import { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useAuth } from '../context/AuthContext';
import { Card } from '../components/ui/Card';
import { Button } from '../components/ui/Button';
import {
    User, Trophy, MapPin, Plane, Star,
    Award, Target, CheckCircle2, Globe, Settings, ChevronDown, ChevronUp
} from 'lucide-react';

const mockProfile = {
    username: 'TanerCam',
    passportCountry: 'Turkey',
    preferredCurrency: 'USD',
    totalPoints: 350,
    level: 'Explorer',
    tripsCreated: 7,
    checkIns: 5,
    citiesVisited: 4,
};

const mockRecentTrips = [
    { city: 'Belgrade', country: 'Serbia', days: 5, budget: 450, date: '2026-01-20', checkIns: 2 },
    { city: 'Tirana', country: 'Albania', days: 4, budget: 350, date: '2026-01-10', checkIns: 1 },
    { city: 'Bangkok', country: 'Thailand', days: 7, budget: 800, date: '2025-12-15', checkIns: 2 },
];

const mockAchievements = [
    { icon: '🗺️', title: 'First Route', desc: 'Generated your first route', earned: true },
    { icon: '📍', title: 'First Check-In', desc: 'Checked in at your first attraction', earned: true },
    { icon: '🌍', title: 'Globe Trotter', desc: 'Visited 3+ countries', earned: true },
    { icon: '⭐', title: 'Rising Star', desc: 'Earned 500 points', earned: false },
    { icon: '🏆', title: 'Top 3', desc: 'Reach the top 3 on the leaderboard', earned: false },
    { icon: '✈️', title: 'Frequent Flyer', desc: 'Generate 10+ trips', earned: false },
];

function getLevelInfo(points: number) {
    if (points >= 2000) return { level: 'Legend', color: 'from-amber-400 to-yellow-600', next: null, progress: 100 };
    if (points >= 1000) return { level: 'Voyager', color: 'from-purple-400 to-pink-500', next: 2000, progress: ((points - 1000) / 1000) * 100 };
    if (points >= 500) return { level: 'Adventurer', color: 'from-blue-400 to-indigo-500', next: 1000, progress: ((points - 500) / 500) * 100 };
    if (points >= 200) return { level: 'Explorer', color: 'from-teal-400 to-blue-500', next: 500, progress: ((points - 200) / 300) * 100 };
    return { level: 'Novice', color: 'from-gray-400 to-gray-500', next: 200, progress: (points / 200) * 100 };
}

const COUNTRIES = ['Turkey', 'Germany', 'USA', 'UK', 'Canada', 'Poland', 'Japan', 'Thailand', 'Australia', 'France'];
const CURRENCIES = ['USD', 'EUR', 'GBP', 'TRY', 'JPY', 'THB', 'CAD', 'AUD', 'PLN'];

export const ProfilePage = () => {
    const { user } = useAuth();
    const levelInfo = getLevelInfo(mockProfile.totalPoints);

    const [passportCountry, setPassportCountry] = useState(mockProfile.passportCountry);
    const [preferredCurrency, setPreferredCurrency] = useState(mockProfile.preferredCurrency);
    const [prefsSaved, setPrefsSaved] = useState(false);
    const [isPreferencesOpen, setIsPreferencesOpen] = useState(false);
    const [savedTrips, setSavedTrips] = useState<any[]>([]);

    const handleSavePrefs = () => {
        setPrefsSaved(true);
        setTimeout(() => setPrefsSaved(false), 2500);
    };

    useEffect(() => {
        const fetchSavedTrips = async () => {
            try {
                // TODO: use actual user ID
                const userId = 1;
                const response = await fetch(`http://localhost:5001/api/routes/user/${userId}`);
                if (response.ok) {
                    const data = await response.json();
                    setSavedTrips(data);
                }
            } catch (err) {
                console.error("Failed to fetch saved trips", err);
            }
        };

        fetchSavedTrips();
    }, []);

    const activeTrip = savedTrips.find((t: any) => t.isActiveTrip);

    const handleSetActive = async (routeId: string) => {
        // Optimistic update
        setSavedTrips(prev => prev.map(t => ({
            ...t,
            isActiveTrip: t.id === routeId
        })));

        try {
            await fetch(`http://localhost:5001/api/routes/${routeId}/set-active`, { method: 'PUT' });
        } catch (err) {
            console.error('Failed to set active route', err);
            // Revert on failure
            setSavedTrips(prev => prev.map(t => ({ ...t, isActiveTrip: false })));
        }
    };

    return (
        <div className="min-h-screen">
            <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
                <div className="grid grid-cols-1 lg:grid-cols-12 gap-8">

                    {/* ═══ Left Column (Main Content) ═══ */}
                    <div className="lg:col-span-8 space-y-8">

                        {/* Profile Header */}
                        <motion.div initial={{ opacity: 0, y: -10 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.4 }}>
                            <Card>
                                <div className="flex flex-col md:flex-row items-center gap-6">
                                    <div className="relative">
                                        <div className={`w-20 h-20 rounded-full bg-gradient-to-br ${levelInfo.color} flex items-center justify-center`}>
                                            <User size={32} className="text-white" />
                                        </div>
                                        <div className="absolute -bottom-1 -right-1 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-600 rounded-full px-2 py-0.5 text-[10px] font-bold text-teal-600 dark:text-teal-400">
                                            {levelInfo.level}
                                        </div>
                                    </div>
                                    <div className="flex-1 text-center md:text-left">
                                        <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-1">{user?.name || mockProfile.username}</h1>
                                        <p className="text-sm text-gray-500 dark:text-gray-400 mb-3">{user?.email}</p>
                                        <div className="flex flex-wrap gap-2 justify-center md:justify-start">
                                            <span className="flex items-center gap-1.5 border border-gray-200 dark:border-gray-700 rounded-full px-3 py-1 text-xs text-gray-600 dark:text-gray-300">
                                                <Globe size={12} className="text-teal-500" /> {passportCountry}
                                            </span>
                                            <span className="flex items-center gap-1.5 border border-gray-200 dark:border-gray-700 rounded-full px-3 py-1 text-xs text-gray-600 dark:text-gray-300">
                                                💰 {preferredCurrency}
                                            </span>
                                            <span className="flex items-center gap-1.5 bg-teal-50 dark:bg-teal-500/10 border border-teal-200 dark:border-teal-500/20 rounded-full px-3 py-1 text-xs text-teal-700 dark:text-teal-300 font-medium">
                                                <Star size={12} /> {mockProfile.totalPoints} pts
                                            </span>
                                        </div>
                                        {activeTrip && (
                                            <div className="mt-3 flex items-center gap-2 bg-gradient-to-r from-emerald-50 to-teal-50 dark:from-emerald-900/20 dark:to-teal-900/20 border border-emerald-200 dark:border-emerald-500/30 rounded-xl px-4 py-2">
                                                <span className="text-lg">📍</span>
                                                <span className="text-sm font-semibold text-emerald-700 dark:text-emerald-300">Currently Traveling:</span>
                                                <span className="text-sm font-bold text-gray-900 dark:text-white">{activeTrip.destinationCity}</span>
                                            </div>
                                        )}
                                    </div>
                                    <div className="w-full md:w-56">
                                        <div className="flex justify-between text-xs text-gray-500 dark:text-gray-400 mb-1.5">
                                            <span>{levelInfo.level}</span>
                                            {levelInfo.next && <span>{levelInfo.next} pts</span>}
                                        </div>
                                        <div className="h-2 bg-gray-200 dark:bg-gray-700 rounded-full overflow-hidden">
                                            <motion.div className={`h-full bg-gradient-to-r ${levelInfo.color} rounded-full`} initial={{ width: 0 }} animate={{ width: `${levelInfo.progress}%` }} transition={{ duration: 1, delay: 0.3 }} />
                                        </div>
                                        <p className="text-[10px] text-gray-400 mt-1 text-right">
                                            {levelInfo.next ? `${levelInfo.next - mockProfile.totalPoints} pts to next` : 'Max level!'}
                                        </p>
                                    </div>
                                </div>
                            </Card>
                        </motion.div>

                        {/* Preferences */}
                        <motion.div initial={{ opacity: 0, x: -10 }} animate={{ opacity: 1, x: 0 }} transition={{ delay: 0.2 }}>
                            <Card className="h-full overflow-hidden">
                                <button
                                    onClick={() => setIsPreferencesOpen(!isPreferencesOpen)}
                                    className="w-full flex items-center justify-between p-1 hover:bg-gray-50 dark:hover:bg-gray-800/50 rounded-lg transition-colors"
                                >
                                    <div className="flex items-center gap-2">
                                        <Settings size={18} className="text-gray-500 dark:text-gray-400" />
                                        <h2 className="text-base font-semibold text-gray-900 dark:text-white">Preferences</h2>
                                    </div>
                                    {isPreferencesOpen ? <ChevronUp size={18} className="text-gray-400" /> : <ChevronDown size={18} className="text-gray-400" />}
                                </button>

                                <AnimatePresence>
                                    {isPreferencesOpen && (
                                        <motion.div
                                            initial={{ height: 0, opacity: 0 }}
                                            animate={{ height: 'auto', opacity: 1 }}
                                            exit={{ height: 0, opacity: 0 }}
                                            transition={{ duration: 0.3, ease: 'easeInOut' }}
                                        >
                                            <div className="pt-5">
                                                <div className="grid md:grid-cols-2 gap-6">
                                                    {/* Read-only fields */}
                                                    <div className="space-y-4">
                                                        <div>
                                                            <label className="text-xs font-medium text-gray-500 dark:text-gray-400 mb-1 block">Name</label>
                                                            <div className="bg-gray-50 dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg px-4 py-2.5 text-sm text-gray-700 dark:text-gray-300 cursor-not-allowed">
                                                                {user?.name || mockProfile.username}
                                                            </div>
                                                        </div>
                                                        <div>
                                                            <label className="text-xs font-medium text-gray-500 dark:text-gray-400 mb-1 block">Email</label>
                                                            <div className="bg-gray-50 dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg px-4 py-2.5 text-sm text-gray-700 dark:text-gray-300 cursor-not-allowed">
                                                                {user?.email || 'user@routiq.com'}
                                                            </div>
                                                        </div>
                                                    </div>

                                                    {/* Editable fields */}
                                                    <div className="space-y-4">
                                                        <div>
                                                            <label className="text-xs font-medium text-gray-700 dark:text-gray-300 mb-1 block">Passport Country</label>
                                                            <select
                                                                value={passportCountry}
                                                                onChange={(e) => setPassportCountry(e.target.value)}
                                                                className="w-full bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg px-4 py-2.5 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-teal-500/40 focus:border-teal-500 transition-colors"
                                                            >
                                                                {COUNTRIES.map(c => <option key={c} value={c}>{c}</option>)}
                                                            </select>
                                                        </div>
                                                        <div>
                                                            <label className="text-xs font-medium text-gray-700 dark:text-gray-300 mb-1 block">Preferred Currency</label>
                                                            <select
                                                                value={preferredCurrency}
                                                                onChange={(e) => setPreferredCurrency(e.target.value)}
                                                                className="w-full bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg px-4 py-2.5 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-teal-500/40 focus:border-teal-500 transition-colors"
                                                            >
                                                                {CURRENCIES.map(c => <option key={c} value={c}>{c}</option>)}
                                                            </select>
                                                        </div>
                                                    </div>
                                                </div>

                                                <div className="mt-6 flex justify-end">
                                                    <Button onClick={handleSavePrefs} className="w-full md:w-auto min-w-[140px] flex items-center justify-center gap-2">
                                                        {prefsSaved ? <><CheckCircle2 size={16} /> Saved!</> : 'Save Preferences'}
                                                    </Button>
                                                </div>
                                            </div>
                                        </motion.div>
                                    )}
                                </AnimatePresence>
                            </Card>
                        </motion.div>

                        {/* Saved & Recent Trips */}
                        <motion.div initial={{ opacity: 0, x: 10 }} animate={{ opacity: 1, x: 0 }} transition={{ delay: 0.25 }}>
                            <Card className="h-full">
                                <div className="flex items-center gap-2 mb-5">
                                    <Plane size={18} className="text-blue-500" />
                                    <h2 className="text-base font-semibold text-gray-900 dark:text-white">Saved & Recent Trips</h2>
                                </div>
                                <div className="space-y-3">
                                    {savedTrips.length === 0 && mockRecentTrips.length === 0 && (
                                        <p className="text-sm text-gray-500 text-center py-4">No trips found. Start planning!</p>
                                    )}

                                    {/* Saved Trips from API */}
                                    {savedTrips.map((trip) => {
                                        let parsedRoute: any = null;
                                        try {
                                            if (trip.itinerarySnapshotJson) {
                                                parsedRoute = JSON.parse(trip.itinerarySnapshotJson);
                                            }
                                        } catch { /* ignore parse errors */ }

                                        return (
                                            <div key={trip.id} className={`border rounded-lg p-4 transition-all ${trip.isActiveTrip
                                                    ? 'border-emerald-400 dark:border-emerald-500 bg-emerald-50/60 dark:bg-emerald-900/20 ring-2 ring-emerald-300/50 dark:ring-emerald-500/30 shadow-lg shadow-emerald-100 dark:shadow-emerald-900/20'
                                                    : 'border-teal-200 dark:border-teal-700/60 bg-teal-50/50 dark:bg-teal-900/10 hover:bg-teal-100/50 dark:hover:bg-teal-800/20'
                                                }`}>
                                                <div className="flex justify-between items-start mb-2">
                                                    <div>
                                                        <div className="flex items-center gap-2">
                                                            <h3 className="text-sm font-semibold text-gray-900 dark:text-white">{trip.destinationCity}</h3>
                                                            {trip.isActiveTrip ? (
                                                                <span className="bg-emerald-100 dark:bg-emerald-500/20 text-emerald-700 dark:text-emerald-300 text-[10px] px-1.5 py-0.5 rounded border border-emerald-300 dark:border-emerald-500/30 font-semibold animate-pulse">📍 Active</span>
                                                            ) : (
                                                                <span className="bg-teal-100 dark:bg-teal-500/20 text-teal-700 dark:text-teal-300 text-[10px] px-1.5 py-0.5 rounded border border-teal-200 dark:border-teal-500/20">Saved</span>
                                                            )}
                                                        </div>
                                                        <p className="text-xs text-gray-400 mt-0.5">Saved {new Date(trip.savedAt).toLocaleDateString()}</p>
                                                    </div>
                                                    <span className="bg-blue-50 dark:bg-blue-500/10 border border-blue-200 dark:border-blue-500/20 text-blue-600 dark:text-blue-300 text-[10px] px-2 py-0.5 rounded-full font-medium">
                                                        {trip.durationDays} days
                                                    </span>
                                                </div>
                                                <div className="flex gap-4 text-xs mb-3">
                                                    <span className="text-gray-500 dark:text-gray-400">💰 <span className="font-medium text-gray-700 dark:text-gray-200">${trip.totalBudget.toLocaleString()}</span></span>
                                                    {parsedRoute && <span className="text-gray-500 dark:text-gray-400">📝 <span className="font-medium text-gray-700 dark:text-gray-200">{parsedRoute.stops?.length || 1} stops</span></span>}
                                                    {parsedRoute && <span className="text-gray-500 dark:text-gray-400">🗺️ <span className="font-medium text-gray-700 dark:text-gray-200">{parsedRoute.routeType}</span></span>}
                                                </div>
                                                <button
                                                    onClick={() => handleSetActive(trip.id)}
                                                    disabled={trip.isActiveTrip}
                                                    className={`text-[11px] px-3 py-1.5 rounded-lg border transition-colors font-medium ${trip.isActiveTrip
                                                            ? 'bg-emerald-100 dark:bg-emerald-500/20 text-emerald-600 dark:text-emerald-400 border-emerald-300 dark:border-emerald-500/30 cursor-default'
                                                            : 'text-gray-500 dark:text-gray-400 border-gray-200 dark:border-gray-600 hover:text-emerald-600 hover:border-emerald-300 hover:bg-emerald-50 dark:hover:bg-emerald-900/20 dark:hover:border-emerald-500/40'
                                                        }`}
                                                >
                                                    {trip.isActiveTrip ? '✓ Active Trip' : '📍 Set as Active Trip'}
                                                </button>
                                            </div>
                                        );
                                    })}

                                    {/* Mock Recent Trips (Legacy/Placeholder) */}
                                    {mockRecentTrips.map((trip, i) => (
                                        <div key={`mock-${i}`} className="border border-gray-200 dark:border-gray-700/60 rounded-lg p-4 hover:bg-gray-50 dark:hover:bg-gray-700/30 transition-colors opacity-75">
                                            <div className="flex justify-between items-start mb-2">
                                                <div>
                                                    <h3 className="text-sm font-semibold text-gray-900 dark:text-white">{trip.city}, {trip.country}</h3>
                                                    <p className="text-xs text-gray-400 mt-0.5">{trip.date}</p>
                                                </div>
                                                <span className="bg-blue-50 dark:bg-blue-500/10 border border-blue-200 dark:border-blue-500/20 text-blue-600 dark:text-blue-300 text-[10px] px-2 py-0.5 rounded-full font-medium">
                                                    {trip.days} days
                                                </span>
                                            </div>
                                            <div className="flex gap-4 text-xs">
                                                <span className="text-gray-500 dark:text-gray-400">💰 <span className="font-medium text-gray-700 dark:text-gray-200">${trip.budget}</span></span>
                                                <span className="text-gray-500 dark:text-gray-400">📍 <span className="font-medium text-green-600 dark:text-green-400">{trip.checkIns} check-ins</span></span>
                                                <span className="text-gray-500 dark:text-gray-400">⭐ <span className="font-medium text-teal-600 dark:text-teal-400">+{trip.checkIns * 50} pts</span></span>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            </Card>
                        </motion.div>

                        {/* Achievements */}
                        <motion.div initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.3 }}>
                            <Card>
                                <div className="flex items-center gap-2 mb-5">
                                    <Award size={18} className="text-amber-500" />
                                    <h2 className="text-base font-semibold text-gray-900 dark:text-white">Achievements</h2>
                                    <span className="ml-auto text-xs text-gray-400">
                                        {mockAchievements.filter(a => a.earned).length}/{mockAchievements.length} unlocked
                                    </span>
                                </div>
                                <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
                                    {mockAchievements.map((achievement, i) => (
                                        <motion.div
                                            key={i}
                                            initial={{ opacity: 0, scale: 0.95 }}
                                            animate={{ opacity: 1, scale: 1 }}
                                            transition={{ delay: 0.35 + i * 0.04 }}
                                            className={`p-4 rounded-lg border transition-all ${achievement.earned
                                                ? 'border-gray-200 dark:border-gray-700/60 bg-white dark:bg-gray-800/40'
                                                : 'border-gray-100 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/20 opacity-50'
                                                }`}
                                        >
                                            <div className="flex items-center gap-2 mb-2">
                                                <span className="text-lg">{achievement.icon}</span>
                                                <span className="text-sm font-medium text-gray-900 dark:text-white">{achievement.title}</span>
                                                {achievement.earned && <Target size={12} className="text-green-500 ml-auto" />}
                                            </div>
                                            <p className="text-xs text-gray-500 dark:text-gray-400">{achievement.desc}</p>
                                        </motion.div>
                                    ))}
                                </div>
                            </Card>
                        </motion.div>
                    </div>

                    {/* ═══ Right Column (Stats Sidebar) ═══ */}
                    <div className="lg:col-span-4 space-y-4">
                        <motion.div initial={{ opacity: 0, x: 20 }} animate={{ opacity: 1, x: 0 }} transition={{ delay: 0.1 }} className="space-y-4">
                            {[
                                { icon: <Trophy size={18} className="text-amber-500" />, label: 'Total Points', value: mockProfile.totalPoints },
                                { icon: <Plane size={18} className="text-blue-500" />, label: 'Trips Created', value: mockProfile.tripsCreated },
                                { icon: <CheckCircle2 size={18} className="text-green-500" />, label: 'Check-Ins', value: mockProfile.checkIns },
                                { icon: <MapPin size={18} className="text-pink-500" />, label: 'Cities Visited', value: mockProfile.citiesVisited },
                            ].map((stat, i) => (
                                <Card key={i} className="flex flex-row items-center gap-4 p-5">
                                    <div className="p-3 rounded-full bg-gray-50 dark:bg-gray-800 border border-gray-100 dark:border-gray-700">
                                        {stat.icon}
                                    </div>
                                    <div>
                                        <div className="text-2xl font-bold text-gray-900 dark:text-white leading-none mb-1">{stat.value}</div>
                                        <div className="text-xs text-gray-500 dark:text-gray-400">{stat.label}</div>
                                    </div>
                                </Card>
                            ))}
                        </motion.div>

                        {/* Placeholder for future sidebar items (e.g. Friends list) */}
                        <div className="p-4 border border-dashed border-gray-200 dark:border-gray-800 rounded-xl text-center text-xs text-gray-400">
                            More stats coming soon...
                        </div>
                    </div>

                </div>
            </main>
        </div>
    );
};
