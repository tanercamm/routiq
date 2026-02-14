import { useState } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import { HeroInput } from './components/HeroInput';
import { RouteCard } from './components/RouteCard';
import { ItineraryModal } from './components/ItineraryModal';
import { generateRoutes } from './api/routiqApi';
import type { RouteRequest, RouteOption } from './types';
import { motion, AnimatePresence } from 'framer-motion';
import { Map, Loader2, LogOut } from 'lucide-react';
import { Button } from './components/ui/Button';

import { CostVsDurationChart } from './components/CostVsDurationChart';

function Dashboard() {
  const [loading, setLoading] = useState(false);
  const [routes, setRoutes] = useState<RouteOption[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [selectedRoute, setSelectedRoute] = useState<RouteOption | null>(null);
  const { logout, user } = useAuth();

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
    <div className="min-h-screen bg-slate-900 text-white font-sans selection:bg-teal-500/30 relative">
      <div className="fixed inset-0 z-0">
        <div className="absolute inset-0 bg-[url('https://images.unsplash.com/photo-1469854523086-cc02fe5d8800?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=2021&q=80')] bg-cover bg-center opacity-20" />
        <div className="absolute inset-0 bg-gradient-to-b from-slate-900/80 via-slate-900/90 to-slate-900" />
      </div>

      <div className="relative z-10 container mx-auto px-4 py-8 flex flex-col items-center min-h-screen">
        <div className="w-full flex justify-between items-center mb-12">
          <div className="flex items-center gap-2">
            <div className="p-2 bg-teal-500/10 rounded-lg border border-teal-500/20">
              <Map size={24} className="text-teal-400" />
            </div>
            <span className="text-xl font-bold bg-gradient-to-r from-teal-400 via-blue-500 to-purple-600 bg-clip-text text-transparent">
              Routiq
            </span>
          </div>
          <div className="flex items-center gap-4">
            <span className="text-gray-400 hidden sm:inline">Welcome, {user?.name}</span>
            <Button variant="outline" onClick={logout} className="flex items-center gap-2">
              <LogOut size={16} /> Sign Out
            </Button>
          </div>
        </div>

        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6 }}
          className="text-center mb-12"
        >
          <h1 className="text-5xl md:text-7xl font-bold mb-4 bg-gradient-to-r from-teal-400 via-blue-500 to-purple-600 bg-clip-text text-transparent tracking-tight">
            Routiq
          </h1>
          <p className="text-xl text-gray-300 max-w-2xl mx-auto font-light">
            AI-Free, Deterministic Travel Planning for the Modern Explorer.
          </p>
        </motion.div>

        <motion.div
          initial={{ opacity: 0, scale: 0.95 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ delay: 0.2, duration: 0.5 }}
          className="w-full"
        >
          <HeroInput onSearch={handleSearch} loading={loading} />
        </motion.div>

        <div className="w-full max-w-6xl mt-16">
          <AnimatePresence mode="wait">
            {loading ? (
              <motion.div
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                exit={{ opacity: 0 }}
                className="flex flex-col items-center justify-center py-20"
              >
                <Loader2 size={48} className="text-teal-400 animate-spin mb-4" />
                <p className="text-gray-400 animate-pulse">Calculating optimal routes...</p>
              </motion.div>
            ) : error ? (
              <motion.div
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                className="text-center text-red-400 py-10 bg-red-500/10 rounded-xl border border-red-500/20"
              >
                {error}
              </motion.div>
            ) : routes.length > 0 ? (
              <div className="space-y-12">
                <CostVsDurationChart data={routes} />
                <motion.div
                  initial={{ opacity: 0 }}
                  animate={{ opacity: 1 }}
                  transition={{ duration: 0.5 }}
                  className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8"
                >
                  {routes.map((option, index) => (
                    <RouteCard key={index} option={option} index={index} onViewItinerary={setSelectedRoute} />
                  ))}
                </motion.div>
              </div>
            ) : null}
          </AnimatePresence>
        </div>
      </div>

      <ItineraryModal route={selectedRoute} onClose={() => setSelectedRoute(null)} />
    </div>
  );
}

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route element={<ProtectedRoute />}>
            <Route path="/" element={<Dashboard />} />
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;
