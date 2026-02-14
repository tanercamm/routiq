import { useState } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { ThemeProvider, useTheme } from './context/ThemeContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import { ProfilePage } from './pages/ProfilePage';
import { HeroInput } from './components/HeroInput';
import { RouteCard } from './components/RouteCard';
import { ItineraryModal } from './components/ItineraryModal';
import { generateRoutes } from './api/routiqApi';
import type { RouteRequest, RouteOption } from './types';
import { motion, AnimatePresence } from 'framer-motion';
import { Map, Loader2, LogOut, User, TrendingUp, ThumbsUp, GitFork } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { Button } from './components/ui/Button';
import { Card } from './components/ui/Card';
import { CostVsDurationChart } from './components/CostVsDurationChart';
import { countryCodeToFlag } from './utils/communityData';

// ── Mock data ──

const leaderboardData = [
  { rank: 1, username: 'WanderSarah', countryCode: 'US', age: 28, points: 1200, trips: 14, badge: '🥇' },
  { rank: 2, username: 'NomadKai', countryCode: 'DE', age: 31, points: 950, trips: 11, badge: '🥈' },
  { rank: 3, username: 'ExplorerMax', countryCode: 'GB', age: 25, points: 800, trips: 9, badge: '🥉' },
  { rank: 4, username: 'TanerCam', countryCode: 'TR', age: 22, points: 350, trips: 7, badge: '⭐' },
  { rank: 5, username: 'GlobeAnya', countryCode: 'PL', age: 27, points: 300, trips: 6, badge: '' },
  { rank: 6, username: 'TrailBlazerJay', countryCode: 'CA', age: 34, points: 250, trips: 5, badge: '' },
];

const communityPicks = [
  { city: 'Bangkok', country: 'Thailand', user: 'NomadKai', countryCode: 'DE', tip: 'Skip the tourist boats — use Chao Phraya Express for 15 baht! Same river, 1/10th the price.', upvotes: 56, days: 7, budget: 800 },
  { city: 'Istanbul', country: 'Turkey', user: 'WanderSarah', countryCode: 'US', tip: 'Get the Museum Pass Istanbul — covers Topkapi, Hagia Sophia and more. Saves hours of queuing!', upvotes: 58, days: 5, budget: 600 },
  { city: 'Tokyo', country: 'Japan', user: 'TrailBlazerJay', countryCode: 'CA', tip: 'Get a Suica card immediately at the airport. Works on all trains, buses, and even vending machines.', upvotes: 63, days: 10, budget: 1500 },
  { city: 'Belgrade', country: 'Serbia', user: 'ExplorerMax', countryCode: 'GB', tip: 'The Nikola Tesla Museum is small but absolutely fascinating. Book tickets online to skip the line!', upvotes: 21, days: 4, budget: 350 },
];

function Dashboard() {
  const [loading, setLoading] = useState(false);
  const [routes, setRoutes] = useState<RouteOption[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [selectedRoute, setSelectedRoute] = useState<RouteOption | null>(null);
  const { logout, user } = useAuth();
  const { theme, toggleTheme } = useTheme();
  const navigate = useNavigate();

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
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 transition-colors duration-200">

      {/* ── Navbar ── */}
      <header className="border-b border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 sticky top-0 z-20">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
          <div className="flex items-center gap-2.5">
            <div className="p-1.5 bg-teal-50 dark:bg-teal-500/10 rounded-lg">
              <Map size={22} className="text-teal-600 dark:text-teal-400" />
            </div>
            <span className="text-lg font-bold text-gray-900 dark:text-white tracking-tight">
              Routiq
            </span>
          </div>
          <div className="flex items-center gap-3">
            <span className="text-sm text-gray-500 dark:text-gray-400 hidden sm:inline">
              {user?.name}
            </span>
            <button
              onClick={toggleTheme}
              className="p-2 rounded-lg border border-gray-200 dark:border-gray-700 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors text-sm"
              title={`Switch to ${theme === 'dark' ? 'light' : 'dark'} mode`}
            >
              {theme === 'dark' ? '☀️' : '🌙'}
            </button>
            <Button variant="outline" onClick={() => navigate('/profile')} className="flex items-center gap-2">
              <User size={14} /> Profile
            </Button>
            <Button variant="outline" onClick={logout} className="flex items-center gap-2">
              <LogOut size={14} /> Sign Out
            </Button>
          </div>
        </div>
      </header>

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
              <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-2 tracking-tight">
                Plan Your Next Trip
              </h1>
              <p className="text-base text-gray-500 dark:text-gray-400">
                Deterministic travel planning — no AI hallucinations, just real routes and real prices.
              </p>
            </motion.div>

            {/* Search */}
            <motion.div
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.1, duration: 0.4 }}
              className="mb-10"
            >
              <HeroInput onSearch={handleSearch} loading={loading} />
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
                  <Loader2 size={36} className="text-teal-500 animate-spin mb-3" />
                  <p className="text-gray-500 dark:text-gray-400">Calculating optimal routes...</p>
                </motion.div>
              ) : error ? (
                <motion.div
                  initial={{ opacity: 0 }}
                  animate={{ opacity: 1 }}
                  className="text-center text-red-600 dark:text-red-400 py-10 bg-red-50 dark:bg-red-500/10 rounded-xl border border-red-200 dark:border-red-500/20"
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
                    className="grid grid-cols-1 md:grid-cols-2 gap-6"
                  >
                    {routes.map((option, index) => (
                      <RouteCard key={index} option={option} index={index} onViewItinerary={setSelectedRoute} />
                    ))}
                  </motion.div>
                </div>
              ) : null}
            </AnimatePresence>
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
                  <div className="flex items-center gap-2 mb-4">
                    <TrendingUp size={18} className="text-teal-600 dark:text-teal-400" />
                    <h2 className="text-base font-semibold text-gray-900 dark:text-white">Top Travelers</h2>
                  </div>
                  <div className="space-y-1.5">
                    {leaderboardData.map((entry) => (
                      <div
                        key={entry.rank}
                        className={`flex items-center gap-3 px-3 py-2.5 rounded-lg transition-colors ${entry.username === 'TanerCam'
                            ? 'bg-teal-50 dark:bg-teal-500/10 border border-teal-200 dark:border-teal-500/20'
                            : 'hover:bg-gray-50 dark:hover:bg-gray-700/40'
                          }`}
                      >
                        <span className="w-7 text-center font-bold text-gray-400 dark:text-gray-500 text-sm">
                          {entry.badge || `#${entry.rank}`}
                        </span>
                        <div className="flex-1 min-w-0">
                          <span className={`text-sm font-medium ${entry.username === 'TanerCam' ? 'text-teal-700 dark:text-teal-300' : 'text-gray-900 dark:text-gray-100'}`}>
                            {countryCodeToFlag(entry.countryCode)} {entry.username}{entry.age ? ` (${entry.age})` : ''}
                          </span>
                          <span className="text-xs text-gray-400 dark:text-gray-500 ml-2">{entry.trips} trips</span>
                        </div>
                        <span className="text-xs font-semibold text-gray-500 dark:text-gray-400">{entry.points.toLocaleString()}</span>
                      </div>
                    ))}
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
                  <div className="flex items-center gap-2 mb-4">
                    <span className="text-base">🌍</span>
                    <h2 className="text-base font-semibold text-gray-900 dark:text-white">Community Picks</h2>
                  </div>
                  <div className="space-y-3">
                    {communityPicks.map((pick, i) => (
                      <motion.div
                        key={i}
                        initial={{ opacity: 0, y: 8 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ delay: 0.3 + i * 0.04 }}
                        className="border border-gray-200 dark:border-gray-700/60 rounded-lg p-3.5 hover:bg-gray-50 dark:hover:bg-gray-700/30 transition-colors"
                      >
                        <h3 className="text-sm font-semibold text-gray-900 dark:text-white">{pick.city}, {pick.country}</h3>
                        <p className="text-[11px] text-gray-400 dark:text-gray-500 mt-0.5">
                          {countryCodeToFlag(pick.countryCode)} {pick.user} · {pick.days}d · ${pick.budget}
                        </p>
                        <p className="text-xs text-gray-600 dark:text-gray-300 leading-relaxed mt-2 line-clamp-2">{pick.tip}</p>
                        <div className="flex items-center gap-2 mt-2.5">
                          <button className="flex items-center gap-1.5 text-[11px] text-gray-500 hover:text-teal-600 dark:hover:text-teal-400 transition-colors border border-gray-200 dark:border-gray-600 rounded-md px-2 py-1 hover:border-teal-300 dark:hover:border-teal-500/40">
                            <ThumbsUp size={10} /> {pick.upvotes}
                          </button>
                          <button className="flex items-center gap-1.5 text-[11px] text-gray-500 hover:text-blue-600 dark:hover:text-blue-400 transition-colors border border-gray-200 dark:border-gray-600 rounded-md px-2 py-1 hover:border-blue-300 dark:hover:border-blue-500/40">
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
      </main>

      <ItineraryModal route={selectedRoute} onClose={() => setSelectedRoute(null)} />
    </div>
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
              <Route path="/" element={<Dashboard />} />
              <Route path="/profile" element={<ProfilePage />} />
            </Route>
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </BrowserRouter>
      </ThemeProvider>
    </AuthProvider>
  );
}

export default App;
