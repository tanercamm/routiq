import { useState, useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
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
import { generateRoutes, saveRoute } from './api/routiqApi';
import type { RouteRequest, RouteResponse, RouteOption, RouteStop, EliminationSummary, BudgetBracket, RegionPreference } from './types';
import { motion, AnimatePresence } from 'framer-motion';
import ReactCountryFlag from 'react-country-flag';
import { countryNames } from './utils/countryMapper';
import {
  Loader2, Wallet, Calendar, MapPin, CheckSquare,
  ChevronDown, ChevronRight, XCircle, CheckCircle2, Zap, AlertTriangle, Clock, Map, BookmarkPlus
} from 'lucide-react';

// PASSPORT_OPTIONS removed — citizenship is account-level, set in Profile/Registration

const BUDGET_BRACKETS: { value: BudgetBracket; label: string; sub: string }[] = [
  { value: 'Shoestring', label: 'Shoestring', sub: '~$0–30/day' },
  { value: 'Budget', label: 'Budget', sub: '~$30–60/day' },
  { value: 'Mid', label: 'Mid-Range', sub: '~$60–120/day' },
  { value: 'Comfort', label: 'Comfort', sub: '~$120–250/day' },
  { value: 'Luxury', label: 'Luxury', sub: '$250+/day' },
];

const REGION_OPTIONS: { value: RegionPreference; label: string }[] = [
  { value: 'Any', label: '🌍 Any Region' },
  { value: 'SoutheastAsia', label: '🌏 Southeast Asia' },
  { value: 'EasternEurope', label: '🏰 Eastern Europe' },
  { value: 'Balkans', label: '⛰️ Balkans' },
  { value: 'LatinAmerica', label: '🌿 Latin America' },
  { value: 'NorthAfrica', label: '🏜️ North Africa' },
  { value: 'CentralAsia', label: '🗺️ Central Asia' },
  { value: 'CentralAmerica', label: '🌺 Central America' },
  { value: 'MiddleEast', label: '🕌 Middle East' },
  { value: 'Caribbean', label: '🏝️ Caribbean' },
];

const REASON_META: Record<string, { label: string; color: string; icon: React.FC<{ size?: number; className?: string }> }> = {
  VisaRequired: { label: 'Visa Required', color: 'red', icon: XCircle },
  BudgetInsufficient: { label: 'Budget Too Low', color: 'orange', icon: Wallet },
  DaysInsufficient: { label: 'Not Enough Days', color: 'yellow', icon: Clock },
  BannedDestination: { label: 'Entry Banned', color: 'red', icon: AlertTriangle },
  RegionMismatch: { label: 'Region Mismatch', color: 'slate', icon: Map },
};

const COST_LEVEL_BADGE: Record<string, string> = {
  Low: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-500/20 dark:text-emerald-300',
  Medium: 'bg-amber-100 text-amber-700 dark:bg-amber-500/20 dark:text-amber-300',
  High: 'bg-rose-100 text-rose-700 dark:bg-rose-500/20 dark:text-rose-300',
};

// ── Sub-components ──

function FormLabel({ icon: Icon, children }: { icon: React.FC<{ size?: number; className?: string }>; children: React.ReactNode }) {
  return (
    <label className="flex items-center gap-1.5 text-[11px] font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-1">
      <Icon size={12} className="opacity-70" />
      {children}
    </label>
  );
}

function SelectField({ value, onChange, children, id }: {
  value: string; onChange: (v: string) => void; children: React.ReactNode; id: string;
}) {
  return (
    <div className="relative">
      <select
        id={id}
        value={value}
        onChange={e => onChange(e.target.value)}
        className="w-full appearance-none bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-xl px-3 py-1.5 pr-8 text-sm text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500/50 cursor-pointer transition-colors"
      >
        {children}
      </select>
      <ChevronDown size={14} className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none" />
    </div>
  );
}

function RouteOptionCard({
  option, index, onSave, saved
}: {
  option: RouteOption; index: number; onSave: (option: RouteOption) => Promise<void>; saved: boolean;
}) {
  const colors = ['from-blue-600 to-indigo-600', 'from-teal-600 to-emerald-600', 'from-violet-600 to-purple-600', 'from-rose-600 to-orange-500'];
  const gradient = colors[index % colors.length];
  const [saving, setSaving] = useState(false);
  const [toast, setToast] = useState<'idle' | 'success' | 'error'>('idle');

  const handleClick = async () => {
    if (saved || saving) return;
    setSaving(true);
    try {
      await onSave(option);
      setToast('success');
      setTimeout(() => setToast('idle'), 3000);
    } catch {
      setToast('error');
      setTimeout(() => setToast('idle'), 3000);
    } finally {
      setSaving(false);
    }
  };

  return (
    <motion.div
      initial={{ opacity: 0, y: 12 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ delay: index * 0.08 }}
      className="bg-white dark:bg-gray-800/60 border border-gray-100 dark:border-gray-700/60 rounded-2xl overflow-hidden shadow-sm"
    >
      {/* Header */}
      <div className={`bg-gradient-to-r ${gradient} px-2.5 py-1.5`}>
        <div className="flex items-start justify-between gap-3">
          <div>
            <h3 className="text-white font-bold text-xs">{option.routeName}</h3>
            <p className="text-white/80 text-[10px] mt-0.5 leading-relaxed">{option.selectionReason}</p>
          </div>
          <span className="shrink-0 bg-white/20 text-white text-[10px] font-semibold px-2 py-0.5 rounded-full whitespace-nowrap mt-0.5">
            {option.estimatedBudgetRange}
          </span>
        </div>
      </div>

      {/* Stops */}
      <div className="divide-y divide-gray-50 dark:divide-gray-700/50">
        {option.stops.map((stop, si) => (
          <StopRow key={si} stop={stop} index={si} />
        ))}
      </div>

      {/* Save footer */}
      <div className="px-2.5 py-1.5 bg-gray-50 dark:bg-gray-800/40 border-t border-gray-100 dark:border-gray-700/40 flex items-center justify-between gap-3">
        {/* Toast message */}
        <AnimatePresence>
          {toast === 'success' && (
            <motion.span
              key="ok"
              initial={{ opacity: 0, x: -6 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0 }}
              className="text-xs text-emerald-600 dark:text-emerald-400 font-medium flex items-center gap-1"
            >
              <CheckCircle2 size={12} /> Route saved to your profile!
            </motion.span>
          )}
          {toast === 'error' && (
            <motion.span
              key="err"
              initial={{ opacity: 0, x: -6 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0 }}
              className="text-xs text-red-500 font-medium"
            >
              Save failed — is the API running?
            </motion.span>
          )}
          {toast === 'idle' && <span />}
        </AnimatePresence>

        <button
          onClick={handleClick}
          disabled={saved || saving}
          className={`flex items-center gap-1.5 text-[10px] uppercase tracking-wide font-bold px-2.5 py-1 rounded-md transition-all shrink-0 ${saved
            ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-500/20 dark:text-emerald-300 cursor-default'
            : saving
              ? 'bg-blue-50 text-blue-400 dark:bg-blue-500/10 cursor-wait opacity-70 border border-blue-200 dark:border-blue-500/30'
              : 'bg-blue-50 text-blue-600 dark:bg-blue-500/10 dark:text-blue-400 hover:bg-blue-100 dark:hover:bg-blue-500/20 border border-blue-200 dark:border-blue-500/30'
            }`}
        >
          {saved ? <><CheckCircle2 size={13} /> Saved!</> : saving ? <>Saving...</> : <><BookmarkPlus size={13} /> Save This Route</>}
        </button>
      </div>
    </motion.div>
  );
}

function StopRow({ stop, index }: { stop: RouteStop; index: number }) {
  return (
    <div className="flex items-start gap-2.5 px-2.5 py-2">
      {/* Order badge */}
      <span className="w-5 h-5 rounded-full bg-gray-100 dark:bg-gray-700 flex items-center justify-center text-[10px] font-bold text-gray-500 dark:text-gray-300 shrink-0 mt-0.5">
        {index + 1}
      </span>
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 flex-wrap">
          <span className="text-xs font-bold text-gray-900 dark:text-white flex items-center gap-1.5">
            <ReactCountryFlag countryCode={stop.countryCode} svg style={{ width: '1.2em', height: '1.2em', borderRadius: '2px' }} title={stop.countryCode} />
            {stop.city}
          </span>
          <span className="text-[10px] text-gray-400 dark:text-gray-500 uppercase tracking-wide font-semibold mt-0.5">{stop.country}</span>
          <span className={`text-[9px] font-bold px-1.5 py-0.5 rounded-sm uppercase tracking-wide mt-0.5 ${COST_LEVEL_BADGE[stop.costLevel] ?? 'bg-gray-100 text-gray-600'}`}>
            {stop.costLevel}
          </span>
        </div>
        <div className="flex items-center gap-2.5 mt-0.5 flex-wrap">
          <span className="text-[10px] text-gray-500 dark:text-gray-400 flex items-center gap-1 font-medium">
            <Calendar size={10} /> {stop.recommendedDays} days
          </span>
          <span className="text-[10px] text-gray-500 dark:text-gray-400 flex items-center gap-1 font-medium">
            <Wallet size={10} /> {stop.dailyBudgetRange}
          </span>
          <span className="text-[10px] text-blue-600 dark:text-blue-400 font-bold uppercase tracking-wide">
            {stop.visaStatus}
          </span>
        </div>
        {stop.stopReason && (
          <p className="text-[10px] text-gray-400 dark:text-gray-500 mt-0.5 leading-snug">
            {stop.stopReason}
          </p>
        )}
      </div>
    </div>
  );
}

function EliminationCard({ elim, index }: { elim: EliminationSummary; index: number }) {
  const [open, setOpen] = useState(false);
  const meta = REASON_META[elim.reason] ?? { label: elim.reason, color: 'gray', icon: XCircle };
  const Icon = meta.icon;

  const colorMap: Record<string, string> = {
    red: 'bg-red-50 dark:bg-red-500/10 border-red-100 dark:border-red-500/20',
    orange: 'bg-orange-50 dark:bg-orange-500/10 border-orange-100 dark:border-orange-500/20',
    yellow: 'bg-yellow-50 dark:bg-yellow-500/10 border-yellow-100 dark:border-yellow-500/20',
    slate: 'bg-slate-50 dark:bg-slate-500/10 border-slate-200 dark:border-slate-500/20',
    gray: 'bg-gray-50 dark:bg-gray-800 border-gray-100 dark:border-gray-700',
  };

  const iconColorMap: Record<string, string> = {
    red: 'text-red-500', orange: 'text-orange-500', yellow: 'text-yellow-500',
    slate: 'text-slate-500', gray: 'text-gray-400',
  };

  const badgeColorMap: Record<string, string> = {
    red: 'bg-red-100 text-red-700 dark:bg-red-500/20 dark:text-red-300',
    orange: 'bg-orange-100 text-orange-700 dark:bg-orange-500/20 dark:text-orange-300',
    yellow: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-500/20 dark:text-yellow-300',
    slate: 'bg-slate-100 text-slate-700 dark:bg-slate-500/20 dark:text-slate-300',
    gray: 'bg-gray-100 text-gray-600 dark:bg-gray-700 dark:text-gray-300',
  };

  return (
    <motion.div
      initial={{ opacity: 0, x: -6 }}
      animate={{ opacity: 1, x: 0 }}
      transition={{ delay: index * 0.04 }}
      className={`border rounded-xl overflow-hidden ${colorMap[meta.color]}`}
    >
      <button
        onClick={() => setOpen(o => !o)}
        className="w-full flex items-center gap-2.5 px-2.5 py-1.5 text-left"
        aria-expanded={open}
      >
        <Icon size={12} className={`shrink-0 ${iconColorMap[meta.color]}`} />
        <div className="flex-1 min-w-0">
          <span className="text-xs font-bold text-gray-900 dark:text-white">
            {elim.city}, <span className="text-gray-500 text-[10px] uppercase font-semibold">{elim.country}</span>
          </span>
        </div>
        <span className={`text-[9px] font-bold px-1.5 py-0.5 rounded-sm shrink-0 uppercase tracking-wide ${badgeColorMap[meta.color]}`}>
          {meta.label}
        </span>
        <ChevronRight size={12} className={`shrink-0 text-gray-400 transition-transform ${open ? 'rotate-90' : ''}`} />
      </button>
      <AnimatePresence>
        {open && (
          <motion.div
            initial={{ height: 0, opacity: 0 }}
            animate={{ height: 'auto', opacity: 1 }}
            exit={{ height: 0, opacity: 0 }}
            transition={{ duration: 0.2 }}
            className="overflow-hidden"
          >
            <p className="px-2.5 pb-1.5 pl-8 text-[10px] text-gray-600 dark:text-gray-300 leading-snug">
              {elim.explanation}
            </p>
          </motion.div>
        )}
      </AnimatePresence>
    </motion.div>
  );
}

// ── Dashboard ──

function Dashboard() {
  useTheme();
  const { user } = useAuth();
  // Passports are account-level — read from AuthContext, never editable here
  const citizenPassports = Array.isArray(user?.passports) ? user.passports : ['TR'];

  const [form, setForm] = useState<RouteRequest>({
    passports: citizenPassports,
    budgetBracket: 'Budget',
    totalBudgetUsd: 1500,
    durationDays: 10,
    regionPreference: 'Any',
    hasSchengenVisa: false,
    hasUsVisa: false,
    hasUkVisa: false,
  });

  // Hydrate local form state if AuthContext sets/loads passport array late
  useEffect(() => {
    if (Array.isArray(user?.passports) && user.passports.length > 0) {
      setForm(prev => ({ ...prev, passports: user.passports }));
    }
  }, [user?.passports]);

  const [result, setResult] = useState<RouteResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  // Track saved route option names to show per-card saved state
  const [savedRouteNames, setSavedRouteNames] = useState<Set<string>>(new Set());

  const setField = <K extends keyof RouteRequest>(key: K, value: RouteRequest[K]) =>
    setForm(prev => ({ ...prev, [key]: value }));

  const handleGenerate = async () => {
    if (form.totalBudgetUsd <= 0 || form.durationDays <= 0) return;
    setLoading(true);
    setError(null);
    setResult(null);
    setSavedRouteNames(new Set());
    try {
      const data = await generateRoutes(form);
      setResult(data);
    } catch (err) {
      console.error(err);
      setError('Could not reach the route engine. Make sure the backend is running.');
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async (option: RouteOption) => {
    const payload = {
      userId: 1,
      routeName: option.routeName,
      passports: form.passports,
      budgetBracket: form.budgetBracket,
      totalBudgetUsd: form.totalBudgetUsd,
      durationDays: form.durationDays,
      regionPreference: form.regionPreference,
      hasSchengenVisa: form.hasSchengenVisa,
      hasUsVisa: form.hasUsVisa,
      hasUkVisa: form.hasUkVisa,
      selectionReason: option.selectionReason,
      stops: option.stops.map((s, i) => ({
        city: s.city,
        countryCode: s.countryCode,
        recommendedDays: s.recommendedDays,
        stopOrder: i + 1,
        costLevel: s.costLevel,
        stopReason: s.stopReason,
      })),
    };
    console.log('[routiq] Save Route Payload:', JSON.stringify(payload, null, 2));
    await saveRoute(payload);
    setSavedRouteNames(prev => new Set([...prev, option.routeName]));
  };

  const hasResult = result !== null;
  const hasOptions = hasResult && result.options.length > 0;
  const hasEliminations = hasResult && result.eliminations.length > 0;

  return (
    <main className="max-w-[1600px] w-[96%] mx-auto px-4 sm:px-6 py-4 min-h-[calc(100vh-4rem)] xl:h-[calc(100vh-4rem)] flex flex-col">
      {/* Page header */}
      <motion.div initial={{ opacity: 0, y: -8 }} animate={{ opacity: 1, y: 0 }} className="mb-4">
        <div className="flex items-center gap-3 mb-1">
          <div className="w-8 h-8 rounded-xl bg-gradient-to-br from-blue-500 to-indigo-600 flex items-center justify-center">
            <Zap size={16} className="text-white" />
          </div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white tracking-tight">
            Decision Engine
          </h1>
        </div>
        <p className="text-sm text-gray-500 dark:text-gray-400 ml-11">
          Deterministic, explainable route generation — no guesswork, no fake flights.
        </p>
      </motion.div>

      {/* 2-column layout */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-4 items-stretch flex-1 min-h-0">

        {/* ═══ LEFT: Input Engine ═══ */}
        <motion.div
          initial={{ opacity: 0, x: -12 }}
          animate={{ opacity: 1, x: 0 }}
          transition={{ duration: 0.4 }}
          className="lg:col-span-4 lg:sticky lg:top-20 lg:h-[calc(100vh-10rem)] mb-8 flex flex-col"
        >
          <div className="bg-white dark:bg-gray-800/60 border border-gray-100 dark:border-gray-700/60 rounded-xl p-5 shadow-sm space-y-4 h-full flex-1 overflow-y-auto custom-scrollbar">

            <div className="flex items-center gap-2 mb-0.5">
              <div className="w-1.5 h-1.5 rounded-full bg-blue-500" />
              <h2 className="text-xs font-bold text-gray-900 dark:text-white uppercase tracking-wide">
                Input Engine
              </h2>
            </div>

            {/* Read-Only Citizen Identity */}
            <div className="bg-gray-50 dark:bg-gray-800/80 border border-gray-200 dark:border-gray-700 rounded-lg px-3 py-2">
              <p className="text-[10px] font-semibold text-gray-400 dark:text-gray-500 uppercase tracking-wide mb-1">Logged-in Citizen of</p>
              <div className="flex flex-wrap gap-2">
                {(!user || !user.passports) ? (
                  <span className="text-xs text-gray-400 italic">Loading profile...</span>
                ) : Array.isArray(citizenPassports) && citizenPassports.map(code => {
                  if (!code) return null;
                  const name = countryNames[code] || code;
                  return (
                    <span key={code} className="inline-flex items-center justify-center gap-1.5 text-[11px] font-semibold bg-blue-50 dark:bg-blue-500/10 border border-blue-200 dark:border-blue-500/30 text-blue-700 dark:text-blue-300 px-2.5 py-1 rounded-full">
                      <ReactCountryFlag countryCode={code} svg style={{ width: '1.2em', height: '1.2em', borderRadius: '2px', display: 'flex', alignItems: 'center' }} title={name} />
                      <span>{name}</span>
                    </span>
                  )
                })}
              </div>
            </div>

            {/* Budget Bracket */}
            <div>
              <FormLabel icon={Wallet}>Budget Bracket</FormLabel>
              <SelectField id="budgetBracket" value={form.budgetBracket} onChange={v => setField('budgetBracket', v as BudgetBracket)}>
                {BUDGET_BRACKETS.map(b => (
                  <option key={b.value} value={b.value}>{b.label} — {b.sub}</option>
                ))}
              </SelectField>
            </div>

            {/* Side-by-side: Budget Cap & Duration */}
            <div className="grid grid-cols-2 gap-3">
              <div>
                <FormLabel icon={Wallet}>Total Budget (USD)</FormLabel>
                <input
                  id="totalBudget"
                  type="number"
                  min={0}
                  step={100}
                  value={form.totalBudgetUsd}
                  onChange={e => setField('totalBudgetUsd', Number(e.target.value))}
                  className="w-full bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-xl px-2.5 py-1.5 text-sm text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500/50 transition-colors"
                  placeholder="e.g. 1500"
                />
              </div>

              <div>
                <FormLabel icon={Calendar}>Duration (Days)</FormLabel>
                <input
                  id="duration"
                  type="number"
                  min={1}
                  max={90}
                  value={form.durationDays}
                  onChange={e => setField('durationDays', Number(e.target.value))}
                  className="w-full bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-xl px-2.5 py-1.5 text-sm text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500/50 transition-colors"
                  placeholder="e.g. 10"
                />
              </div>
            </div>

            {/* Region */}
            <div>
              <FormLabel icon={MapPin}>Region Preference</FormLabel>
              <SelectField id="region" value={form.regionPreference} onChange={v => setField('regionPreference', v as RegionPreference)}>
                {REGION_OPTIONS.map(r => (
                  <option key={r.value} value={r.value}>{r.label}</option>
                ))}
              </SelectField>
            </div>

            {/* Visa flags */}
            <div>
              <FormLabel icon={CheckSquare}>Visas You Hold</FormLabel>
              <div className="flex flex-wrap gap-2">
                {([
                  { key: 'hasUsVisa', code: 'US', label: 'US Visa' },
                  { key: 'hasUkVisa', code: 'GB', label: 'UK Visa' },
                  { key: 'hasSchengenVisa', code: 'EU', label: 'Schengen Visa' },
                ] as const).map(({ key, code, label }) => (
                  <label
                    key={key}
                    htmlFor={key}
                    className="flex items-center gap-2.5 px-2.5 py-1.5 rounded-xl border border-gray-200 dark:border-gray-700 cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-700/40 transition-colors"
                  >
                    <input
                      id={key}
                      type="checkbox"
                      checked={form[key]}
                      onChange={e => setField(key, e.target.checked)}
                      className="w-3.5 h-3.5 rounded text-blue-600 focus:ring-blue-500 cursor-pointer"
                    />
                    <span className="text-sm text-gray-700 dark:text-gray-300 flex items-center gap-1.5">
                      <ReactCountryFlag countryCode={code} svg style={{ width: '1.2em', height: '1.2em', borderRadius: '2px' }} title={label} />
                      <span className="mt-0.5">{label}</span>
                    </span>
                  </label>
                ))}
              </div>
            </div>

            {/* Generate button */}
            <button
              id="generate-route-btn"
              onClick={handleGenerate}
              disabled={loading}
              className="w-full flex items-center justify-center gap-2 bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-700 hover:to-indigo-700 disabled:opacity-60 text-white font-semibold rounded-xl px-4 py-2 text-sm shadow-md shadow-blue-500/20 hover:shadow-blue-500/30 transition-all duration-200 mt-2"
            >
              {loading ? (
                <><Loader2 size={16} className="animate-spin" /> Generating Route...</>
              ) : (
                <><Zap size={15} /> Generate Logical Route</>
              )}
            </button>

          </div>
        </motion.div>

        {/* ═══ RIGHT: Output & Explanation ═══ */}
        <div className="lg:col-span-8 lg:h-[calc(100vh-10rem)] mb-8">
          <div className="bg-white dark:bg-gray-800/60 border border-gray-100 dark:border-gray-700/60 rounded-xl p-5 shadow-sm h-full overflow-y-auto custom-scrollbar">
            <div className="space-y-4">

              <AnimatePresence mode="wait">
                {/* Loading state */}
                {loading && (
                  <motion.div
                    key="loading"
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    exit={{ opacity: 0 }}
                    className="flex flex-col items-center justify-center py-24 bg-white dark:bg-gray-800/40 border border-gray-100 dark:border-gray-700/60 rounded-2xl"
                  >
                    <Loader2 size={36} className="text-blue-500 animate-spin mb-4" />
                    <p className="text-gray-600 dark:text-gray-300 font-medium">Running deterministic filters...</p>
                    <p className="text-xs text-gray-400 dark:text-gray-500 mt-1">Visa rules → Budget → Days</p>
                  </motion.div>
                )}

                {/* Error state */}
                {!loading && error && (
                  <motion.div
                    key="error"
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    className="flex items-start gap-3 p-5 bg-red-50 dark:bg-red-500/10 border border-red-100 dark:border-red-500/20 rounded-2xl"
                  >
                    <XCircle size={18} className="text-red-500 shrink-0 mt-0.5" />
                    <div>
                      <p className="text-sm font-semibold text-red-700 dark:text-red-300">Engine Error</p>
                      <p className="text-xs text-red-600 dark:text-red-400 mt-0.5">{error}</p>
                    </div>
                  </motion.div>
                )}

                {/* Empty / initial state */}
                {!loading && !error && !hasResult && (
                  <motion.div
                    key="empty"
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    className="flex flex-col items-center justify-center py-24 bg-white dark:bg-gray-800/40 border border-dashed border-gray-200 dark:border-gray-700 rounded-2xl"
                  >
                    <div className="w-14 h-14 rounded-2xl bg-gradient-to-br from-blue-100 to-indigo-100 dark:from-blue-500/20 dark:to-indigo-500/20 flex items-center justify-center mb-4">
                      <Zap size={26} className="text-blue-500" />
                    </div>
                    <p className="text-gray-700 dark:text-gray-200 font-semibold text-base">Ready to generate</p>
                    <p className="text-xs text-gray-400 dark:text-gray-500 mt-1.5 text-center max-w-xs">
                      Configure your constraints on the left, then click <strong>Generate Logical Route</strong>.
                    </p>
                  </motion.div>
                )}

                {/* Results */}
                {!loading && hasResult && (
                  <motion.div key="results" initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="space-y-4">

                    {/* ── Route Options ── */}
                    {hasOptions ? (
                      <section>
                        <div className="flex items-center gap-2 mb-4 flex-wrap">
                          <CheckCircle2 size={16} className="text-emerald-500" />
                          <h2 className="text-sm font-bold text-gray-900 dark:text-white uppercase tracking-wide">
                            Generated Routes
                          </h2>
                          <span className="text-[10px] uppercase tracking-wide bg-emerald-100 dark:bg-emerald-500/20 text-emerald-700 dark:text-emerald-300 font-bold px-2 py-0.5 rounded-sm">
                            {result!.options.length} option{result!.options.length > 1 ? 's' : ''}
                          </span>
                          <span className="flex items-center gap-1.5 ml-auto">
                            {Array.isArray(form.passports) && form.passports.map(c => {
                              if (!c) return null;
                              const name = countryNames[c] || c;
                              return (
                                <span key={c} className="inline-flex items-center justify-center gap-1">
                                  <ReactCountryFlag countryCode={c} svg style={{ width: '1.2em', height: '1.2em', borderRadius: '2px', display: 'flex', alignItems: 'center' }} title={name} />
                                  <span className="mt-0.5 text-[10px] uppercase font-bold text-gray-700 dark:text-gray-300">{c}</span>
                                </span>
                              )
                            })}
                          </span>
                        </div>
                        <div className="space-y-2">
                          {result!.options.map((opt, i) => (
                            <RouteOptionCard
                              key={i}
                              option={opt}
                              index={i}
                              onSave={handleSave}
                              saved={savedRouteNames.has(opt.routeName)}
                            />
                          ))}
                        </div>
                      </section>
                    ) : (
                      <section className="flex items-start gap-3 p-5 bg-amber-50 dark:bg-amber-500/10 border border-amber-100 dark:border-amber-500/20 rounded-2xl">
                        <AlertTriangle size={18} className="text-amber-500 shrink-0 mt-0.5" />
                        <div>
                          <p className="text-sm font-semibold text-amber-800 dark:text-amber-300">No viable routes found</p>
                          <p className="text-xs text-amber-700 dark:text-amber-400 mt-0.5">
                            All destinations were eliminated by the engine's rules. See the explanations below.
                          </p>
                        </div>
                      </section>
                    )}

                    {/* ── Why NOT Section ── */}
                    {hasEliminations && (
                      <section>
                        <div className="flex items-center gap-2 mb-4">
                          <XCircle size={16} className="text-red-400" />
                          <h2 className="text-sm font-bold text-gray-900 dark:text-white uppercase tracking-wide">
                            Why NOT These Destinations?
                          </h2>
                          <span className="text-[10px] uppercase tracking-wide bg-red-100 dark:bg-red-500/20 text-red-700 dark:text-red-300 font-bold px-2 py-0.5 rounded-sm">
                            {result!.eliminations.length} eliminated
                          </span>
                        </div>
                        <div className="space-y-1.5">
                          {result!.eliminations.map((elim, i) => (
                            <EliminationCard key={i} elim={elim} index={i} />
                          ))}
                        </div>
                        <p className="text-[11px] text-gray-400 dark:text-gray-500 mt-3 text-center">
                          Click any row to see the full engine explanation.
                        </p>
                      </section>
                    )}

                  </motion.div>
                )}
              </AnimatePresence>

            </div>
          </div>
        </div>
      </div>
    </main>
  );
}

// ── App ──

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
