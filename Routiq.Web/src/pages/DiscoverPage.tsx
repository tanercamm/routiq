import { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import { RotateCcw, GitFork, MapPin, Calendar, DollarSign, Check } from 'lucide-react';
import { Card } from '../components/ui/Card';
import ReactCountryFlag from 'react-country-flag';

// ── Mock trip data ──

const tripCards = [
    { id: 1, destination: 'Istanbul', country: 'Turkey', countryCode: 'TR', creator: 'WanderSarah', cost: 620, days: 5, region: 'Europe', description: 'Historic city tour with local street food experiences and Bosphorus cruise.' },
    { id: 2, destination: 'Bangkok', country: 'Thailand', countryCode: 'TH', creator: 'NomadKai', cost: 800, days: 7, region: 'Asia', description: 'Temple hopping, floating markets, and Khao San Road nightlife.' },
    { id: 3, destination: 'Tokyo', country: 'Japan', countryCode: 'JP', creator: 'TrailBlazerJay', cost: 1500, days: 10, region: 'Asia', description: 'From Shibuya to Akihabara — ramen, shrines, and bullet trains.' },
    { id: 4, destination: 'Lisbon', country: 'Portugal', countryCode: 'PT', creator: 'GlobeAnya', cost: 550, days: 4, region: 'Europe', description: 'Tram 28, pastéis de nata, and sunset views from Alfama.' },
    { id: 5, destination: 'Marrakech', country: 'Morocco', countryCode: 'MA', creator: 'ExplorerMax', cost: 430, days: 5, region: 'Africa', description: 'Souks, riads, and the Atlas Mountains on a budget.' },
    { id: 6, destination: 'Buenos Aires', country: 'Argentina', countryCode: 'AR', creator: 'TanerCam', cost: 700, days: 6, region: 'South America', description: 'Tango, steak, and colorful La Boca streets.' },
    { id: 7, destination: 'Prague', country: 'Czech Republic', countryCode: 'CZ', creator: 'NomadKai', cost: 480, days: 4, region: 'Europe', description: 'Charles Bridge, Old Town Square, and cheap Czech beer.' },
    { id: 8, destination: 'Bali', country: 'Indonesia', countryCode: 'ID', creator: 'WanderSarah', cost: 650, days: 8, region: 'Asia', description: 'Rice terraces, surf lessons, and Uluwatu temple at sunset.' },
    { id: 9, destination: 'Cape Town', country: 'South Africa', countryCode: 'ZA', creator: 'ExplorerMax', cost: 900, days: 7, region: 'Africa', description: 'Table Mountain, wine country, and the Garden Route.' },
    { id: 10, destination: 'Reykjavik', country: 'Iceland', countryCode: 'IS', creator: 'TrailBlazerJay', cost: 1800, days: 5, region: 'Europe', description: 'Northern lights, geysers, and the Blue Lagoon.' },
    { id: 11, destination: 'Mexico City', country: 'Mexico', countryCode: 'MX', creator: 'GlobeAnya', cost: 520, days: 6, region: 'North America', description: 'Tacos al pastor, Frida Kahlo museum, and Chapultepec Park.' },
    { id: 12, destination: 'Seoul', country: 'South Korea', countryCode: 'KR', creator: 'TanerCam', cost: 950, days: 7, region: 'Asia', description: 'K-BBQ, Gyeongbokgung Palace, and Hongdae nightlife.' },
];

const REGIONS = ['All', 'Europe', 'Asia', 'Africa', 'North America', 'South America'];
const BUDGET_LIMITS = ['Any', '< $500', '< $1000', '< $1500'];
const DURATIONS = ['Any', '1–4 days', '5–7 days', '8+ days'];

export const DiscoverPage = () => {
    const [region, setRegion] = useState('All');
    const [budgetLimit, setBudgetLimit] = useState('Any');
    const [duration, setDuration] = useState('Any');
    const [showToast, setShowToast] = useState(false);
    const [forkedName, setForkedName] = useState('');
    const navigate = useNavigate();

    const handleFork = (card: typeof tripCards[0]) => {
        setForkedName(card.destination);
        setShowToast(true);
        setTimeout(() => {
            setShowToast(false);
            navigate('/', { state: { forkedBudget: card.cost, forkedDays: card.days } });
        }, 1500);
    };

    const filtered = tripCards.filter((card) => {
        if (region !== 'All' && card.region !== region) return false;
        if (budgetLimit === '< $500' && card.cost >= 500) return false;
        if (budgetLimit === '< $1000' && card.cost >= 1000) return false;
        if (budgetLimit === '< $1500' && card.cost >= 1500) return false;
        if (duration === '1–4 days' && card.days > 4) return false;
        if (duration === '5–7 days' && (card.days < 5 || card.days > 7)) return false;
        if (duration === '8+ days' && card.days < 8) return false;
        return true;
    });

    return (
        <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">

            {/* Hero */}
            <motion.div
                initial={{ opacity: 0, y: -10 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.4 }}
                className="mb-10"
            >
                <h1 className="text-4xl font-extrabold text-gray-900 dark:text-white tracking-tight mb-2">
                    Discover Community Routes
                </h1>
                <p className="text-base text-gray-400 dark:text-gray-500 max-w-2xl">
                    Explore hand-crafted itineraries from travelers around the world. Fork any route and make it your own.
                </p>
            </motion.div>

            {/* Filter Bar */}
            <motion.div
                initial={{ opacity: 0, y: 10 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.1, duration: 0.4 }}
                className="mb-8"
            >
                <Card>
                    <div className="flex flex-col sm:flex-row items-start sm:items-end gap-4">
                        {/* Region */}
                        <div className="flex-1 w-full">
                            <label className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5 flex items-center gap-2">
                                <MapPin size={14} /> Region
                            </label>
                            <select
                                value={region}
                                onChange={(e) => setRegion(e.target.value)}
                                className="w-full bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg px-4 py-2.5 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500/40 focus:border-blue-500 transition-colors"
                            >
                                {REGIONS.map((r) => (
                                    <option key={r} value={r}>{r}</option>
                                ))}
                            </select>
                        </div>

                        {/* Budget Limit */}
                        <div className="flex-1 w-full">
                            <label className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5 flex items-center gap-2">
                                <DollarSign size={14} /> Budget Limit
                            </label>
                            <select
                                value={budgetLimit}
                                onChange={(e) => setBudgetLimit(e.target.value)}
                                className="w-full bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg px-4 py-2.5 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500/40 focus:border-blue-500 transition-colors"
                            >
                                {BUDGET_LIMITS.map((b) => (
                                    <option key={b} value={b}>{b}</option>
                                ))}
                            </select>
                        </div>

                        {/* Duration */}
                        <div className="flex-1 w-full">
                            <label className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5 flex items-center gap-2">
                                <Calendar size={14} /> Duration
                            </label>
                            <select
                                value={duration}
                                onChange={(e) => setDuration(e.target.value)}
                                className="w-full bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg px-4 py-2.5 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500/40 focus:border-blue-500 transition-colors"
                            >
                                {DURATIONS.map((d) => (
                                    <option key={d} value={d}>{d}</option>
                                ))}
                            </select>
                        </div>

                        {/* Reset filters */}
                        <div className="w-full sm:w-auto">
                            <button
                                onClick={() => { setRegion('All'); setBudgetLimit('Any'); setDuration('Any'); }}
                                className="w-full sm:w-auto h-[42px] flex items-center justify-center gap-2 px-5 bg-gray-100 hover:bg-gray-200 dark:bg-gray-800 dark:hover:bg-gray-700 text-gray-700 dark:text-gray-300 text-sm font-medium rounded-lg transition-colors"
                            >
                                <RotateCcw size={16} /> Reset Filters
                            </button>
                        </div>
                    </div>
                </Card>
            </motion.div>

            {/* Results count */}
            <div className="mb-6">
                <p className="text-sm text-gray-400 dark:text-gray-500">
                    Showing <span className="font-semibold text-gray-700 dark:text-gray-300">{filtered.length}</span> routes
                </p>
            </div>

            {/* Trip Cards Grid */}
            <motion.div
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                transition={{ delay: 0.2, duration: 0.4 }}
                className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6"
            >
                {filtered.map((card, index) => (
                    <motion.div
                        key={card.id}
                        initial={{ opacity: 0, y: 15 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ delay: 0.05 * index }}
                    >
                        <Card hoverEffect className="h-full flex flex-col justify-between">
                            <div>
                                {/* Header */}
                                <div className="flex items-start justify-between mb-3">
                                    <div>
                                        <h3 className="text-lg font-bold text-gray-900 dark:text-white flex items-center gap-2">
                                            <ReactCountryFlag countryCode={card.countryCode} svg style={{ width: '1.3em', height: '1.3em', borderRadius: '3px' }} title={card.country} />
                                            {card.destination}
                                        </h3>
                                        <p className="text-xs text-gray-400 dark:text-gray-500 mt-0.5">{card.country}</p>
                                    </div>
                                    <span className="bg-blue-50 dark:bg-blue-500/10 text-blue-700 dark:text-blue-300 text-xs px-2.5 py-1 rounded-full border border-blue-200 dark:border-blue-500/20 font-medium">
                                        {card.region}
                                    </span>
                                </div>

                                {/* Description */}
                                <p className="text-sm text-gray-600 dark:text-gray-400 leading-relaxed mb-4 line-clamp-2">
                                    {card.description}
                                </p>

                                {/* Meta */}
                                <div className="flex items-center gap-4 text-xs text-gray-500 dark:text-gray-400 mb-4">
                                    <span className="flex items-center gap-1"><Calendar size={12} /> {card.days} days</span>
                                    <span className="flex items-center gap-1"><DollarSign size={12} /> ${card.cost}</span>
                                </div>
                            </div>

                            {/* Footer */}
                            <div className="pt-4 border-t border-gray-100 dark:border-gray-700/50 flex items-center justify-between">
                                <span className="text-xs text-gray-400 dark:text-gray-500">
                                    by <span className="font-medium text-gray-600 dark:text-gray-300">{card.creator}</span>
                                </span>
                                <button
                                    onClick={() => handleFork(card)}
                                    className="flex items-center gap-1.5 text-sm font-medium text-blue-600 dark:text-blue-400 hover:text-white hover:bg-blue-600 dark:hover:bg-blue-500 rounded-lg px-3 py-1.5 border border-blue-200 dark:border-blue-500/30 hover:border-transparent transition-all">
                                    <GitFork size={14} /> Fork
                                </button>
                            </div>
                        </Card>
                    </motion.div>
                ))}
            </motion.div>

            {/* Empty state */}
            {filtered.length === 0 && (
                <motion.div
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    className="text-center py-20"
                >
                    <p className="text-gray-400 dark:text-gray-500 text-lg">No routes match your filters.</p>
                    <button
                        onClick={() => { setRegion('All'); setBudgetLimit('Any'); setDuration('Any'); }}
                        className="mt-3 text-sm text-blue-600 dark:text-blue-400 hover:underline"
                    >
                        Clear all filters
                    </button>
                </motion.div>
            )}
            {/* Fork toast */}
            <AnimatePresence>
                {showToast && (
                    <motion.div
                        initial={{ opacity: 0, y: 40 }}
                        animate={{ opacity: 1, y: 0 }}
                        exit={{ opacity: 0, y: 40 }}
                        className="fixed bottom-6 left-1/2 -translate-x-1/2 z-[60] bg-blue-600 text-white px-5 py-3 rounded-xl shadow-lg flex items-center gap-2 text-sm font-medium"
                    >
                        <Check size={16} /> Route to {forkedName} forked!
                    </motion.div>
                )}
            </AnimatePresence>
        </main>
    );
};
