import { useState, useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate, useLocation } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { ThemeProvider, useTheme } from './context/ThemeContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import { ProfilePage } from './pages/ProfilePage';
import { DiscoverPage } from './pages/DiscoverPage';
import { AnalyticsPage } from './pages/AnalyticsPage';
import { TravelGroupsPage } from './pages/TravelGroupsPage';
import { SettingsPage } from './pages/SettingsPage';
import { AppLayout } from './components/AppLayout';
import { HeroInput } from './components/HeroInput';
import { RouteCard } from './components/RouteCard';
import { ItineraryModal } from './components/ItineraryModal';
import { generateRoutes, getLeaderboard, type LeaderboardDto } from './api/routiqApi';
import type { RouteRequest, RouteOption } from './types';
import { motion, AnimatePresence } from 'framer-motion';
import { Loader2, TrendingUp, Heart, GitFork } from 'lucide-react';
import { Card } from './components/ui/Card';
import { CostVsDurationChart } from './components/CostVsDurationChart';
import { countryCodeToFlag } from './utils/communityData';
import {
  LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend,
  ResponsiveContainer, BarChart, Bar, PieChart, Pie, Cell
} from 'recharts';

// ── Mock data ──



const communityPicksInitial = [
  { id: '11111111-1111-1111-1111-111111111111', city: 'Bangkok', country: 'Thailand', user: 'NomadKai', countryCode: 'DE', tip: 'Skip the tourist boats — use Chao Phraya Express for 15 baht! Same river, 1/10th the price.', upvotes: 56, days: 7, budget: 800 },
  { id: '22222222-2222-2222-2222-222222222222', city: 'Istanbul', country: 'Turkey', user: 'WanderSarah', countryCode: 'US', tip: 'Get the Museum Pass Istanbul — covers Topkapi, Hagia Sophia and more. Saves hours of queuing!', upvotes: 58, days: 5, budget: 600 },
  { id: '33333333-3333-3333-3333-333333333333', city: 'Tokyo', country: 'Japan', user: 'TrailBlazerJay', countryCode: 'CA', tip: 'Get a Suica card immediately at the airport. Works on all trains, buses, and even vending machines.', upvotes: 63, days: 10, budget: 1500 },
  { id: '44444444-4444-4444-4444-444444444444', city: 'Belgrade', country: 'Serbia', user: 'ExplorerMax', countryCode: 'GB', tip: 'The Nikola Tesla Museum is small but absolutely fascinating. Book tickets online to skip the line!', upvotes: 21, days: 4, budget: 350 },
];

// ── Analytics data ──

const trendData = [
  { month: 'Jan', tripVolume: 35, budgetTrends: 40 },
  { month: 'Feb', tripVolume: 42, budgetTrends: 55 },
  { month: 'Mar', tripVolume: 58, budgetTrends: 62 },
  { month: 'Apr', tripVolume: 48, budgetTrends: 50 },
  { month: 'May', tripVolume: 65, budgetTrends: 70 },
  { month: 'Jun', tripVolume: 72, budgetTrends: 55 },
  { month: 'Jul', tripVolume: 120, budgetTrends: 62 },
];

const topDestinationsData = [
  { city: 'Istanbul', value: 334 },
  { city: 'Paris', value: 236 },
  { city: 'London', value: 184 },
  { city: 'Tokyo', value: 187 },
];

const expensesBreakdown = [
  { name: 'Flights', value: 40 },
  { name: 'Accommodation', value: 35 },
  { name: 'Food', value: 15 },
  { name: 'Activities', value: 10 },
];
const EXPENSES_COLORS = ['#3B82F6', '#14B8A6', '#F59E0B', '#EF4444'];

const AVATAR_COLORS = [
  'bg-blue-500', 'bg-orange-500', 'bg-purple-500',
  'bg-emerald-500', 'bg-pink-500', 'bg-cyan-500',
];

function Dashboard() {
  const [loading, setLoading] = useState(false);
  const [routes, setRoutes] = useState<RouteOption[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [selectedRoute, setSelectedRoute] = useState<RouteOption | null>(null);
  const { theme } = useTheme();

  // Gamification Data
  const [leaderboardData, setLeaderboardData] = useState<LeaderboardDto[]>([]);
  const [loadingLeaderboard, setLoadingLeaderboard] = useState(true);

  // Load Leaderboard
  useEffect(() => {
    const fetchLeaderboard = async () => {
      try {
        const data = await getLeaderboard(5); // Top 5
        setLeaderboardData(data);
      } catch (err) {
        console.error("Failed to load leaderboard", err);
      } finally {
        setLoadingLeaderboard(false);
      }
    };
    fetchLeaderboard();
  }, []);

  // Community Picks State
  const [picks, setPicks] = useState(communityPicksInitial);
  const [likedTips, setLikedTips] = useState<Set<string>>(new Set());

  const handleLike = async (id: string) => {
    if (likedTips.has(id)) return;

    // Optimistic Update
    setPicks(prev => prev.map(pick =>
      pick.id === id ? { ...pick, upvotes: pick.upvotes + 1 } : pick
    ));
    setLikedTips(prev => new Set(prev).add(id));

    try {
      // API Call
      await fetch(`/api/community/tips/${id}/like`, { method: 'POST' });
    } catch (error) {
      console.error("Failed to like tip", error);
      // Revert if failed
      setPicks(prev => prev.map(pick =>
        pick.id === id ? { ...pick, upvotes: pick.upvotes - 1 } : pick
      ));
      setLikedTips(prev => {
        const next = new Set(prev);
        next.delete(id);
        return next;
      });
    }
  };

  const location = useLocation();
  const forkedState = location.state as { forkedBudget?: number; forkedDays?: number } | null;

  const isDark = theme === 'dark';

  const handleSearch = async (request: RouteRequest) => {
    setLoading(true);
    setError(null);
    try {
      const response = await generateRoutes(request);
      setRoutes(response.options);
    } catch (err) {
      console.error(err);
      setError('Failed to fetch routes. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      {/* ── Main Content: 2-Column Grid ── */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-8">

          {/* ═══ Left Column (Main) ═══ */}
          <div className="lg:col-span-8">

            {/* Hero */}
            <motion.div
              initial={{ opacity: 0, y: -10 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.4 }}
              className="mb-8"
            >
              <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-1 tracking-tight">
                Plan Your Next Trip
              </h1>
            </motion.div>

            {/* Search */}
            <motion.div
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.1, duration: 0.4 }}
              className="mb-8"
            >
              <HeroInput onSearch={handleSearch} loading={loading} initialBudget={forkedState?.forkedBudget} initialDays={forkedState?.forkedDays} />
            </motion.div>

            {/* Route Results */}
            <AnimatePresence mode="wait">
              {loading ? (
                <motion.div
                  initial={{ opacity: 0 }}
                  animate={{ opacity: 1 }}
                  exit={{ opacity: 0 }}
                  className="flex flex-col items-center justify-center py-20"
                >
                  <Loader2 size={36} className="text-blue-500 animate-spin mb-3" />
                  <p className="text-gray-500 dark:text-gray-400">Calculating optimal routes...</p>
                </motion.div>
              ) : error ? (
                <motion.div
                  initial={{ opacity: 0 }}
                  animate={{ opacity: 1 }}
                  className="text-center text-red-600 dark:text-red-400 py-10 bg-red-50 dark:bg-red-500/10 rounded-2xl border border-red-100 dark:border-red-500/20"
                >
                  {error}
                </motion.div>
              ) : routes.length > 0 ? (
                <div className="space-y-8">
                  <CostVsDurationChart data={routes} />
                  <motion.div
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    transition={{ duration: 0.4 }}
                    className="grid grid-cols-1 md:grid-cols-2 gap-5"
                  >
                    {routes.map((option, index) => (
                      <RouteCard key={index} option={option} index={index} onViewItinerary={setSelectedRoute} />
                    ))}
                  </motion.div>
                </div>
              ) : null}
            </AnimatePresence>

            {/* ── Analytics Dashboard (default view) ── */}
            {routes.length === 0 && !loading && !error && (
              <motion.div
                initial={{ opacity: 0, y: 10 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.15, duration: 0.5 }}
                className="grid grid-cols-1 md:grid-cols-2 gap-6"
              >
                {/* Trip Volume / Budget Trends */}
                <Card>
                  <h3 className="text-sm font-semibold text-gray-900 dark:text-white mb-4">
                    Trip Volume/Budget Trends
                  </h3>
                  <ResponsiveContainer width="100%" height={200}>
                    <LineChart data={trendData}>
                      <CartesianGrid strokeDasharray="3 3" stroke={isDark ? '#334155' : '#e5e7eb'} />
                      <XAxis dataKey="month" tick={{ fontSize: 11 }} stroke={isDark ? '#64748b' : '#9ca3af'} />
                      <YAxis tick={{ fontSize: 11 }} stroke={isDark ? '#64748b' : '#9ca3af'} />
                      <Tooltip
                        contentStyle={{
                          backgroundColor: isDark ? '#1e293b' : '#fff',
                          border: `1px solid ${isDark ? '#334155' : '#e5e7eb'}`,
                          borderRadius: '8px',
                          fontSize: '12px',
                        }}
                      />
                      <Legend iconSize={8} wrapperStyle={{ fontSize: '11px' }} />
                      <Line type="monotone" dataKey="tripVolume" name="Trip Volume" stroke="#3B82F6" strokeWidth={2} dot={{ r: 3 }} />
                      <Line type="monotone" dataKey="budgetTrends" name="Budget Trends" stroke="#14B8A6" strokeWidth={2} dot={{ r: 3 }} />
                    </LineChart>
                  </ResponsiveContainer>
                </Card>

                {/* Top Destinations */}
                <Card>
                  <h3 className="text-sm font-semibold text-gray-900 dark:text-white mb-4">
                    Top Destinations (This Quarter)
                  </h3>
                  <ResponsiveContainer width="100%" height={200}>
                    <BarChart data={topDestinationsData} layout="vertical" margin={{ left: 10, right: 20 }}>
                      <CartesianGrid strokeDasharray="3 3" stroke={isDark ? '#334155' : '#e5e7eb'} />
                      <XAxis type="number" tick={{ fontSize: 11 }} stroke={isDark ? '#64748b' : '#9ca3af'} />
                      <YAxis type="category" dataKey="city" tick={{ fontSize: 11 }} stroke={isDark ? '#64748b' : '#9ca3af'} width={60} />
                      <Tooltip
                        contentStyle={{
                          backgroundColor: isDark ? '#1e293b' : '#fff',
                          border: `1px solid ${isDark ? '#334155' : '#e5e7eb'}`,
                          borderRadius: '8px',
                          fontSize: '12px',
                        }}
                      />
                      <Bar dataKey="value" radius={[0, 4, 4, 0]}>
                        {topDestinationsData.map((_, index) => (
                          <Cell key={index} fill={index % 2 === 0 ? '#3B82F6' : '#14B8A6'} />
                        ))}
                      </Bar>
                    </BarChart>
                  </ResponsiveContainer>
                </Card>

                {/* Expenses Breakdown (Replaces Duplicate Chart) */}
                <Card>
                  <h3 className="text-sm font-semibold text-gray-900 dark:text-white mb-4">
                    Expenses Breakdown
                  </h3>
                  <ResponsiveContainer width="100%" height={200}>
                    <PieChart>
                      <Pie
                        data={expensesBreakdown}
                        cx="50%"
                        cy="50%"
                        innerRadius={60}
                        outerRadius={80}
                        paddingAngle={5}
                        dataKey="value"
                      >
                        {expensesBreakdown.map((_, index) => (
                          <Cell key={`cell-${index}`} fill={EXPENSES_COLORS[index % EXPENSES_COLORS.length]} />
                        ))}
                      </Pie>
                      <Tooltip
                        contentStyle={{
                          backgroundColor: isDark ? '#1e293b' : '#fff',
                          border: `1px solid ${isDark ? '#334155' : '#e5e7eb'}`,
                          borderRadius: '8px',
                          fontSize: '12px',
                        }}
                      />
                      <Legend
                        layout="vertical"
                        verticalAlign="middle"
                        align="right"
                        iconSize={8}
                        wrapperStyle={{ fontSize: '11px' }}
                      />
                    </PieChart>
                  </ResponsiveContainer>
                </Card>

                {/* Budget Utilization */}
                <Card>
                  <h3 className="text-sm font-semibold text-gray-900 dark:text-white mb-4">
                    Budget Utilization
                  </h3>
                  <div className="flex items-center justify-center h-[200px] relative">
                    <ResponsiveContainer width="100%" height={180}>
                      <PieChart>
                        <Pie
                          data={[{ value: 74 }, { value: 26 }]}
                          innerRadius={55}
                          outerRadius={75}
                          startAngle={90}
                          endAngle={-270}
                          dataKey="value"
                          stroke="none"
                        >
                          <Cell fill="#14B8A6" />
                          <Cell fill={isDark ? '#1e293b' : '#e5e7eb'} />
                        </Pie>
                      </PieChart>
                    </ResponsiveContainer>
                    <div className="absolute inset-0 flex items-center justify-center">
                      <span className="text-3xl font-bold text-gray-900 dark:text-white">74%</span>
                    </div>
                  </div>
                </Card>
              </motion.div>
            )}
          </div>

          {/* ═══ Right Column (Sidebar) ═══ */}
          <div className="lg:col-span-4">
            <div className="lg:sticky lg:top-24 space-y-6">

              {/* 🏆 Leaderboard */}
              <motion.div
                initial={{ opacity: 0, y: 15 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.2 }}
              >
                <Card className="h-full">
                  <div className="flex items-center gap-2 mb-5">
                    <TrendingUp size={18} className="text-blue-600 dark:text-blue-400" />
                    <h2 className="text-base font-semibold text-gray-900 dark:text-white">Top Travelers</h2>
                  </div>
                  <div className="space-y-2">
                    {loadingLeaderboard ? (
                      <div className="text-center py-4 text-xs text-gray-400">Loading top travelers...</div>
                    ) : leaderboardData.map((entry, i) => {
                      const rank = i + 1;
                      const isTop3 = rank <= 3;
                      return (
                        <div
                          key={entry.username}
                          className={`flex items-center gap-3 px-3 py-3 rounded-xl transition-colors ${isTop3
                            ? 'bg-amber-50 dark:bg-amber-500/10 border border-amber-100 dark:border-amber-500/20' // Gold/highlight for Top 3
                            : 'hover:bg-gray-50 dark:hover:bg-gray-800/60' // Standard hover for others
                            }`}
                        >
                          {/* Rank badge */}
                          <span className={`w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold text-white shrink-0 ${isTop3 ? 'bg-amber-500 shadow-sm' : 'bg-gray-400 dark:bg-gray-600'
                            }`}>
                            {rank}
                          </span>
                          {/* Avatar */}
                          <div className={`w-9 h-9 rounded-full ${AVATAR_COLORS[i % AVATAR_COLORS.length]} flex items-center justify-center text-white text-sm font-semibold shrink-0`}>
                            {entry.username.charAt(0).toUpperCase()}
                          </div>
                          {/* Info */}
                          <div className="flex-1 min-w-0">
                            <span className="text-sm font-medium block text-gray-900 dark:text-gray-100">
                              {entry.username}
                            </span>
                            <span className="text-xs text-gray-400 dark:text-gray-500">
                              {countryCodeToFlag(entry.countryCode)} {entry.tripCount} trips
                            </span>
                          </div>
                          {/* Points */}
                          <span className={`text-sm font-bold ${isTop3 ? 'text-amber-600 dark:text-amber-400' : 'text-gray-700 dark:text-gray-300'}`}>
                            {entry.totalPoints.toLocaleString()}
                          </span>
                        </div>
                      );
                    })}
                  </div>
                </Card>
              </motion.div>

              {/* 🌍 Community Picks */}
              <motion.div
                initial={{ opacity: 0, y: 15 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.25 }}
              >
                <Card className="h-full">
                  <div className="flex items-center gap-2 mb-5">
                    <span className="text-base">🌍</span>
                    <h2 className="text-base font-semibold text-gray-900 dark:text-white">Community Picks</h2>
                  </div>
                  <div className="space-y-4">
                    {picks.map((pick, i) => (
                      <motion.div
                        key={pick.id}
                        initial={{ opacity: 0, y: 8 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ delay: 0.3 + i * 0.04 }}
                        className="border border-gray-100 dark:border-gray-700/50 rounded-xl p-4 hover:bg-gray-50 dark:hover:bg-gray-800/40 transition-colors"
                      >
                        <div className="flex items-start gap-3">
                          {/* Avatar */}
                          <div className={`w-10 h-10 rounded-full ${AVATAR_COLORS[(i + 2) % AVATAR_COLORS.length]} flex items-center justify-center text-white text-sm font-semibold shrink-0 mt-0.5`}>
                            {pick.user.charAt(0)}
                          </div>
                          <div className="flex-1 min-w-0">
                            <div className="flex items-center justify-between">
                              <h3 className="text-sm font-semibold text-gray-900 dark:text-white">{pick.city}, {pick.country}</h3>
                              <span className="text-sm font-bold text-gray-700 dark:text-gray-300">${pick.budget}</span>
                            </div>
                            <p className="text-[11px] text-gray-400 dark:text-gray-500 mt-0.5">
                              {countryCodeToFlag(pick.countryCode)} {pick.user} · {pick.days}d
                            </p>
                            <p className="text-xs text-gray-600 dark:text-gray-300 leading-relaxed mt-2 line-clamp-2">{pick.tip}</p>
                          </div>
                        </div>
                        <div className="flex items-center gap-2 mt-3 pl-[52px]">
                          <button
                            onClick={() => handleLike(pick.id)}
                            disabled={likedTips.has(pick.id)}
                            className={`flex items-center gap-1.5 text-[11px] transition-colors border rounded-lg px-3 py-1.5 
                              ${likedTips.has(pick.id)
                                ? 'text-red-500 border-red-200 bg-red-50 dark:bg-red-900/20'
                                : 'text-gray-500 border-gray-200 dark:border-gray-600 hover:text-red-500 hover:border-red-200 dark:hover:border-red-500/40'
                              }`}
                          >
                            <Heart size={10} className={likedTips.has(pick.id) ? "fill-current" : ""} /> Like {pick.upvotes}
                          </button>
                          <button className="flex items-center gap-1.5 text-[11px] text-gray-500 hover:text-blue-600 dark:hover:text-blue-400 transition-colors border border-gray-200 dark:border-gray-600 rounded-lg px-3 py-1.5 hover:border-blue-200 dark:hover:border-blue-500/40">
                            <GitFork size={10} /> Fork
                          </button>
                        </div>
                      </motion.div>
                    ))}
                  </div>
                </Card>
              </motion.div>

            </div>
          </div>

        </div>
      </main >

      <ItineraryModal route={selectedRoute} onClose={() => setSelectedRoute(null)} />
    </>
  );
}

function App() {
  return (
    <AuthProvider>
      <ThemeProvider>
        <BrowserRouter>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route element={<ProtectedRoute />}>
              <Route element={<AppLayout />}>
                <Route path="/" element={<Dashboard />} />
                <Route path="/profile" element={<ProfilePage />} />
                <Route path="/routes" element={<DiscoverPage />} />
                <Route path="/analytics" element={<AnalyticsPage />} />
                <Route path="/team" element={<TravelGroupsPage />} />
                <Route path="/settings" element={<SettingsPage />} />
              </Route>
            </Route>
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </BrowserRouter>
      </ThemeProvider>
    </AuthProvider>
  );
}

export default App;
