import { motion } from 'framer-motion';
import { useAuth } from '../context/AuthContext';
import { Card } from '../components/ui/Card';
import { Button } from '../components/ui/Button';
import {
    User, Trophy, MapPin, Plane, Star, TrendingUp,
    ArrowLeft, Award, Target, CheckCircle2, Globe
} from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { countryCodeToFlag } from '../utils/communityData';

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

const mockLeaderboard = [
    { rank: 1, username: 'WanderSarah', countryCode: 'US', age: 28, points: 1200, trips: 14, badge: '🥇' },
    { rank: 2, username: 'NomadKai', countryCode: 'DE', age: 31, points: 950, trips: 11, badge: '🥈' },
    { rank: 3, username: 'ExplorerMax', countryCode: 'GB', age: 25, points: 800, trips: 9, badge: '🥉' },
    { rank: 4, username: 'TanerCam', countryCode: 'TR', age: 22, points: 350, trips: 7, badge: '⭐' },
    { rank: 5, username: 'GlobeAnya', countryCode: 'PL', age: 27, points: 300, trips: 6, badge: '' },
    { rank: 6, username: 'TrailBlazerJay', countryCode: 'CA', age: 34, points: 250, trips: 5, badge: '' },
];

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

export const ProfilePage = () => {
    const { user, logout } = useAuth();
    const navigate = useNavigate();
    const levelInfo = getLevelInfo(mockProfile.totalPoints);

    return (
        <div className="min-h-screen bg-gray-50 dark:bg-gray-900 transition-colors duration-200">

            {/* Nav */}
            <header className="border-b border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 sticky top-0 z-20">
                <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 h-14 flex items-center justify-between">
                    <Button variant="outline" onClick={() => navigate('/')} className="flex items-center gap-2 text-sm">
                        <ArrowLeft size={14} /> Dashboard
                    </Button>
                    <Button variant="outline" onClick={logout} className="text-sm">
                        Sign Out
                    </Button>
                </div>
            </header>

            <main className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-8">

                {/* Profile Header */}
                <motion.div
                    initial={{ opacity: 0, y: -10 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.4 }}
                    className="mb-8"
                >
                    <Card>
                        <div className="flex flex-col md:flex-row items-center gap-6">
                            {/* Avatar */}
                            <div className="relative">
                                <div className={`w-20 h-20 rounded-full bg-gradient-to-br ${levelInfo.color} flex items-center justify-center`}>
                                    <User size={32} className="text-white" />
                                </div>
                                <div className="absolute -bottom-1 -right-1 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-600 rounded-full px-2 py-0.5 text-[10px] font-bold text-teal-600 dark:text-teal-400">
                                    {levelInfo.level}
                                </div>
                            </div>

                            {/* User info */}
                            <div className="flex-1 text-center md:text-left">
                                <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-1">{user?.name || mockProfile.username}</h1>
                                <p className="text-sm text-gray-500 dark:text-gray-400 mb-3">{user?.email}</p>
                                <div className="flex flex-wrap gap-2 justify-center md:justify-start">
                                    <span className="flex items-center gap-1.5 border border-gray-200 dark:border-gray-700 rounded-full px-3 py-1 text-xs text-gray-600 dark:text-gray-300">
                                        <Globe size={12} className="text-teal-500" /> {mockProfile.passportCountry}
                                    </span>
                                    <span className="flex items-center gap-1.5 border border-gray-200 dark:border-gray-700 rounded-full px-3 py-1 text-xs text-gray-600 dark:text-gray-300">
                                        💰 {mockProfile.preferredCurrency}
                                    </span>
                                    <span className="flex items-center gap-1.5 bg-teal-50 dark:bg-teal-500/10 border border-teal-200 dark:border-teal-500/20 rounded-full px-3 py-1 text-xs text-teal-700 dark:text-teal-300 font-medium">
                                        <Star size={12} /> {mockProfile.totalPoints} pts
                                    </span>
                                </div>
                            </div>

                            {/* Level progress */}
                            <div className="w-full md:w-56">
                                <div className="flex justify-between text-xs text-gray-500 dark:text-gray-400 mb-1.5">
                                    <span>{levelInfo.level}</span>
                                    {levelInfo.next && <span>{levelInfo.next} pts</span>}
                                </div>
                                <div className="h-2 bg-gray-200 dark:bg-gray-700 rounded-full overflow-hidden">
                                    <motion.div
                                        className={`h-full bg-gradient-to-r ${levelInfo.color} rounded-full`}
                                        initial={{ width: 0 }}
                                        animate={{ width: `${levelInfo.progress}%` }}
                                        transition={{ duration: 1, delay: 0.3 }}
                                    />
                                </div>
                                <p className="text-[10px] text-gray-400 mt-1 text-right">
                                    {levelInfo.next ? `${levelInfo.next - mockProfile.totalPoints} pts to next` : 'Max level!'}
                                </p>
                            </div>
                        </div>
                    </Card>
                </motion.div>

                {/* Stats Row */}
                <motion.div
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: 0.1 }}
                    className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8"
                >
                    {[
                        { icon: <Trophy size={18} className="text-amber-500" />, label: 'Total Points', value: mockProfile.totalPoints },
                        { icon: <Plane size={18} className="text-blue-500" />, label: 'Trips Created', value: mockProfile.tripsCreated },
                        { icon: <CheckCircle2 size={18} className="text-green-500" />, label: 'Check-Ins', value: mockProfile.checkIns },
                        { icon: <MapPin size={18} className="text-pink-500" />, label: 'Cities Visited', value: mockProfile.citiesVisited },
                    ].map((stat, i) => (
                        <Card key={i} className="text-center">
                            <div className="flex flex-col items-center gap-2">
                                {stat.icon}
                                <span className="text-2xl font-bold text-gray-900 dark:text-white">{stat.value}</span>
                                <span className="text-xs text-gray-500 dark:text-gray-400">{stat.label}</span>
                            </div>
                        </Card>
                    ))}
                </motion.div>

                {/* Two Column: Leaderboard + Recent Trips */}
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
                    <motion.div
                        initial={{ opacity: 0, x: -10 }}
                        animate={{ opacity: 1, x: 0 }}
                        transition={{ delay: 0.2 }}
                    >
                        <Card className="h-full">
                            <div className="flex items-center gap-2 mb-5">
                                <TrendingUp size={18} className="text-teal-600 dark:text-teal-400" />
                                <h2 className="text-base font-semibold text-gray-900 dark:text-white">Leaderboard</h2>
                            </div>
                            <div className="space-y-1.5">
                                {mockLeaderboard.map((entry) => (
                                    <div
                                        key={entry.rank}
                                        className={`flex items-center gap-3 px-3 py-2.5 rounded-lg transition-colors ${entry.username === 'TanerCam'
                                                ? 'bg-teal-50 dark:bg-teal-500/10 border border-teal-200 dark:border-teal-500/20'
                                                : 'hover:bg-gray-50 dark:hover:bg-gray-700/40'
                                            }`}
                                    >
                                        <span className="w-7 text-center font-bold text-gray-400 text-sm">
                                            {entry.badge || `#${entry.rank}`}
                                        </span>
                                        <div className="flex-1 min-w-0">
                                            <span className={`text-sm font-medium ${entry.username === 'TanerCam' ? 'text-teal-700 dark:text-teal-300' : 'text-gray-900 dark:text-gray-100'}`}>
                                                {countryCodeToFlag(entry.countryCode)} {entry.username}{entry.age ? ` (${entry.age})` : ''}
                                            </span>
                                            <span className="text-xs text-gray-400 ml-2">{entry.trips} trips</span>
                                        </div>
                                        <span className="text-xs font-semibold text-gray-500 dark:text-gray-400">{entry.points.toLocaleString()}</span>
                                    </div>
                                ))}
                            </div>
                        </Card>
                    </motion.div>

                    <motion.div
                        initial={{ opacity: 0, x: 10 }}
                        animate={{ opacity: 1, x: 0 }}
                        transition={{ delay: 0.25 }}
                    >
                        <Card className="h-full">
                            <div className="flex items-center gap-2 mb-5">
                                <Plane size={18} className="text-blue-500" />
                                <h2 className="text-base font-semibold text-gray-900 dark:text-white">Recent Trips</h2>
                            </div>
                            <div className="space-y-3">
                                {mockRecentTrips.map((trip, i) => (
                                    <div key={i} className="border border-gray-200 dark:border-gray-700/60 rounded-lg p-4 hover:bg-gray-50 dark:hover:bg-gray-700/30 transition-colors">
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
                                            <span className="text-gray-500 dark:text-gray-400">
                                                💰 <span className="font-medium text-gray-700 dark:text-gray-200">${trip.budget}</span>
                                            </span>
                                            <span className="text-gray-500 dark:text-gray-400">
                                                📍 <span className="font-medium text-green-600 dark:text-green-400">{trip.checkIns} check-ins</span>
                                            </span>
                                            <span className="text-gray-500 dark:text-gray-400">
                                                ⭐ <span className="font-medium text-teal-600 dark:text-teal-400">+{trip.checkIns * 50} pts</span>
                                            </span>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </Card>
                    </motion.div>
                </div>

                {/* Achievements */}
                <motion.div
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: 0.3 }}
                >
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
                                        {achievement.earned && (
                                            <Target size={12} className="text-green-500 ml-auto" />
                                        )}
                                    </div>
                                    <p className="text-xs text-gray-500 dark:text-gray-400">{achievement.desc}</p>
                                </motion.div>
                            ))}
                        </div>
                    </Card>
                </motion.div>
            </main>
        </div>
    );
};
