import { useRef, useState, useEffect, useCallback, useMemo } from 'react';
import { createPortal } from 'react-dom';
import { useNavigate } from 'react-router-dom';
import Globe, { type GlobeMethods } from 'react-globe.gl';
import { motion, AnimatePresence } from 'framer-motion';
import { Zap, Shield, DollarSign, Calendar, X } from 'lucide-react';
import {
  SUPPORTED_CITIES,
  WORLD_CAPITALS,
  formatBestMonths,
  getSafetyColor,
  getSafetyLabel,
  type CityPoint,
} from '../data/globeData';

const GLOBE_IMAGE = '//unpkg.com/three-globe/example/img/earth-night.jpg';
const BUMP_IMAGE = '//unpkg.com/three-globe/example/img/earth-topology.png';

export function HomePage() {
  const globeRef = useRef<GlobeMethods | undefined>(undefined);
  const navigate = useNavigate();
  const [selected, setSelected] = useState<CityPoint | null>(null);
  const [dimensions, setDimensions] = useState({ width: 0, height: 0 });
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const update = () => {
      if (containerRef.current) {
        setDimensions({
          width: containerRef.current.offsetWidth,
          height: containerRef.current.offsetHeight,
        });
      }
    };
    update();
    window.addEventListener('resize', update);
    return () => window.removeEventListener('resize', update);
  }, []);

  useEffect(() => {
    if (!globeRef.current) return;
    const controls = globeRef.current.controls();
    if (controls) {
      controls.autoRotate = true;
      controls.autoRotateSpeed = 0.4;
      controls.enableZoom = true;
      controls.minDistance = 180;
      controls.maxDistance = 600;
    }
    globeRef.current.pointOfView({ lat: 35, lng: 30, altitude: 2.2 }, 0);
  }, [dimensions]);

  const handlePointClick = useCallback((point: object | null) => {
    if (!point) return;
    const city = point as CityPoint;
    // Defer the state update so the Three.js render loop isn't interrupted
    requestAnimationFrame(() => setSelected(city));
  }, []);

  const handleDismiss = useCallback(() => {
    setSelected(null);
    // Re-enable auto-rotate in case orbit controls paused it
    const controls = globeRef.current?.controls();
    if (controls) controls.autoRotate = true;
  }, []);

  const supportedRings = useMemo(() =>
    SUPPORTED_CITIES.map(c => ({ ...c, maxR: 3, propagationSpeed: 2, repeatPeriod: 1400 })),
    []
  );

  const supportedArcs = useMemo(() => {
    const istanbul = { lat: 41.0082, lng: 28.9784 };
    return SUPPORTED_CITIES
      .filter((_, i) => i % 3 === 0)
      .map(c => ({
        startLat: istanbul.lat,
        startLng: istanbul.lng,
        endLat: c.lat,
        endLng: c.lng,
        color: ['rgba(0, 200, 255, 0.25)', 'rgba(100, 100, 255, 0.08)'],
      }));
  }, []);

  return (
    <div ref={containerRef} className="relative w-full h-[calc(100vh-4rem)] overflow-hidden bg-[#050a18]">
      {/* Ambient glow behind globe */}
      <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
        <div className="w-[700px] h-[700px] rounded-full bg-blue-500/[0.04] blur-[100px]" />
      </div>

      {/* Scan lines overlay */}
      <div className="absolute inset-0 pointer-events-none z-10 opacity-[0.03]"
        style={{ backgroundImage: 'repeating-linear-gradient(0deg, transparent, transparent 2px, rgba(255,255,255,0.05) 2px, rgba(255,255,255,0.05) 4px)' }}
      />

      {/* Globe */}
      {dimensions.width > 0 && (
        <Globe
          ref={globeRef}
          width={dimensions.width}
          height={dimensions.height}
          globeImageUrl={GLOBE_IMAGE}
          bumpImageUrl={BUMP_IMAGE}
          backgroundImageUrl="//unpkg.com/three-globe/example/img/night-sky.png"
          atmosphereColor="#1e90ff"
          atmosphereAltitude={0.18}

          // Supported cities: bright cyan points
          pointsData={SUPPORTED_CITIES}
          pointLat="lat"
          pointLng="lng"
          pointColor={() => '#00e5ff'}
          pointAltitude={0.01}
          pointRadius={0.45}
          pointsMerge={false}
          onPointClick={handlePointClick}

          // World capitals: dim small labels
          labelsData={WORLD_CAPITALS}
          labelLat="lat"
          labelLng="lng"
          labelText="name"
          labelSize={0.4}
          labelDotRadius={0.15}
          labelColor={() => 'rgba(200, 200, 255, 0.2)'}
          labelResolution={2}
          labelAltitude={0.005}

          // Pulse rings on supported cities
          ringsData={supportedRings}
          ringLat="lat"
          ringLng="lng"
          ringColor={() => (t: number) => `rgba(0, 229, 255, ${1 - t})`}
          ringMaxRadius="maxR"
          ringPropagationSpeed="propagationSpeed"
          ringRepeatPeriod="repeatPeriod"

          // Arcs from Istanbul hub
          arcsData={supportedArcs}
          arcStartLat="startLat"
          arcStartLng="startLng"
          arcEndLat="endLat"
          arcEndLng="endLng"
          arcColor="color"
          arcDashLength={0.4}
          arcDashGap={0.2}
          arcDashAnimateTime={4000}
          arcStroke={0.3}

          animateIn={true}
        />
      )}

      {/* Top-left HUD */}
      <div className="absolute top-6 left-6 z-20 pointer-events-none">
        <motion.div initial={{ opacity: 0, x: -20 }} animate={{ opacity: 1, x: 0 }} transition={{ delay: 0.3 }}>
          <div className="flex items-center gap-2 mb-2">
            <div className="w-2 h-2 rounded-full bg-cyan-400 animate-pulse" />
            <span className="text-[10px] font-bold tracking-[0.25em] text-cyan-400/80 uppercase">
              Routiq Mission Control
            </span>
          </div>
          <h1 className="text-3xl sm:text-4xl font-black text-white tracking-tight leading-none">
            Global Route
            <br />
            <span className="bg-gradient-to-r from-cyan-400 to-blue-500 bg-clip-text text-transparent">
              Intelligence
            </span>
          </h1>
          <p className="text-sm text-gray-400/80 mt-3 max-w-xs leading-relaxed">
            Real-time agentic route analysis across {SUPPORTED_CITIES.length} destinations.
            Click any node for live intel.
          </p>
        </motion.div>
      </div>

      {/* Bottom-left stats */}
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.6 }}
        className="absolute bottom-6 left-6 z-20 pointer-events-none"
      >
        <div className="flex gap-4">
          {[
            { label: 'ACTIVE NODES', value: SUPPORTED_CITIES.length.toString(), color: 'text-cyan-400' },
            { label: 'COUNTRIES', value: new Set(SUPPORTED_CITIES.map(c => c.country)).size.toString(), color: 'text-blue-400' },
            { label: 'REGIONS', value: '7', color: 'text-purple-400' },
          ].map(s => (
            <div key={s.label} className="border border-white/[0.06] bg-black/40 backdrop-blur-sm rounded-lg px-4 py-2.5">
              <div className={`text-xl font-black ${s.color} tabular-nums`}>{s.value}</div>
              <div className="text-[9px] font-bold tracking-[0.2em] text-gray-500 uppercase">{s.label}</div>
            </div>
          ))}
        </div>
      </motion.div>

      {/* Top-right network status */}
      <motion.div
        initial={{ opacity: 0, x: 20 }}
        animate={{ opacity: 1, x: 0 }}
        transition={{ delay: 0.5 }}
        className="absolute top-6 right-6 z-20 pointer-events-none"
      >
        <div className="border border-white/[0.06] bg-black/40 backdrop-blur-sm rounded-lg px-4 py-3 min-w-[180px]">
          <div className="flex items-center gap-2 mb-2">
            <div className="w-1.5 h-1.5 rounded-full bg-green-400 animate-pulse" />
            <span className="text-[10px] font-bold tracking-[0.2em] text-green-400/80 uppercase">System Online</span>
          </div>
          <div className="space-y-1">
            {['Visa Engine', 'Cost Analyzer', 'Flight Scanner', 'Safety Monitor'].map((s, i) => (
              <div key={s} className="flex items-center justify-between">
                <span className="text-[10px] text-gray-500">{s}</span>
                <span className={`text-[10px] font-bold ${i < 3 ? 'text-green-500' : 'text-cyan-500'}`}>
                  {i < 3 ? 'READY' : 'ACTIVE'}
                </span>
              </div>
            ))}
          </div>
        </div>
      </motion.div>

      {/* Center CTA */}
      <motion.div
        initial={{ opacity: 0, scale: 0.9 }}
        animate={{ opacity: 1, scale: 1 }}
        transition={{ delay: 1, type: 'spring', stiffness: 100 }}
        className="absolute bottom-8 left-1/2 -translate-x-1/2 z-20"
      >
        <button
          onClick={() => navigate('/find-route')}
          className="group relative cursor-pointer"
        >
          <div className="absolute -inset-1 rounded-2xl bg-gradient-to-r from-cyan-500 via-blue-500 to-purple-500 opacity-60 blur-lg group-hover:opacity-100 transition-opacity duration-500" />
          <div className="relative flex items-center gap-3 bg-[#0a1628]/90 border border-cyan-500/30 group-hover:border-cyan-400/60 rounded-2xl px-8 py-4 backdrop-blur-sm transition-all duration-300">
            <Zap size={20} className="text-cyan-400 group-hover:text-cyan-300" />
            <div className="text-left">
              <div className="text-sm font-black text-white tracking-wide group-hover:text-cyan-100 transition-colors">
                Initialize Agentic Search
              </div>
              <div className="text-[10px] text-gray-400 tracking-wider uppercase">
                Launch Decision Engine
              </div>
            </div>
            <div className="ml-2 w-px h-8 bg-white/10" />
            <div className="text-cyan-400 group-hover:translate-x-1 transition-transform">→</div>
          </div>
        </button>
      </motion.div>

      {/* Mobile bottom bar for very small screens */}
      <div className="absolute bottom-0 left-0 right-0 h-16 bg-gradient-to-t from-[#050a18] to-transparent pointer-events-none z-10 sm:hidden" />

      {/* Intelligence Card — rendered via portal so the Three.js canvas keeps running */}
      {createPortal(
        <AnimatePresence>
          {selected && (
            <motion.div
              key={selected.name}
              initial={{ opacity: 0, y: 24, scale: 0.96 }}
              animate={{ opacity: 1, y: 0, scale: 1 }}
              exit={{ opacity: 0, y: 16, scale: 0.96 }}
              transition={{ type: 'spring', stiffness: 350, damping: 28 }}
              className="fixed bottom-6 right-6 z-[9999] w-[320px]"
            >
              <div className="bg-[#0c1a30]/95 border border-cyan-500/30 backdrop-blur-xl rounded-xl p-4 shadow-2xl shadow-cyan-500/10">
                {/* Header + Close */}
                <div className="flex items-start justify-between mb-3">
                  <div className="min-w-0">
                    <div className="flex items-center gap-2">
                      <div className="w-2 h-2 rounded-full bg-cyan-400 animate-pulse shrink-0" />
                      <h3 className="text-sm font-black text-white tracking-tight truncate">{selected.name}</h3>
                      {selected.isSupported && (
                        <span className="text-[9px] font-black tracking-widest bg-cyan-500/20 text-cyan-300 px-2 py-0.5 rounded-md border border-cyan-500/30 uppercase shrink-0">
                          Live
                        </span>
                      )}
                    </div>
                    <p className="text-[10px] text-gray-400 ml-4 uppercase tracking-wider">{selected.country}</p>
                  </div>
                  <button
                    onClick={handleDismiss}
                    className="p-1 -m-1 rounded-lg text-gray-500 hover:text-white hover:bg-white/10 transition-colors shrink-0 ml-2"
                    aria-label="Close intelligence card"
                  >
                    <X size={16} />
                  </button>
                </div>

                {selected.isSupported ? (
                  <div className="space-y-2">
                    {/* Safety */}
                    <div className="flex items-center justify-between bg-white/[0.03] rounded-lg px-3 py-2">
                      <div className="flex items-center gap-2">
                        <Shield size={13} className="text-gray-500" />
                        <span className="text-[11px] text-gray-400">Safety</span>
                      </div>
                      <div className="flex items-center gap-2">
                        <div className="h-1 w-16 bg-gray-800 rounded-full overflow-hidden">
                          <div
                            className="h-full rounded-full transition-all"
                            style={{
                              width: `${selected.safetyIndex}%`,
                              backgroundColor: getSafetyColor(selected.safetyIndex),
                            }}
                          />
                        </div>
                        <span className="text-[11px] font-bold" style={{ color: getSafetyColor(selected.safetyIndex) }}>
                          {getSafetyLabel(selected.safetyIndex)}
                        </span>
                      </div>
                    </div>

                    {/* Cost */}
                    <div className="flex items-center justify-between bg-white/[0.03] rounded-lg px-3 py-2">
                      <div className="flex items-center gap-2">
                        <DollarSign size={13} className="text-gray-500" />
                        <span className="text-[11px] text-gray-400">Avg Meal</span>
                      </div>
                      <span className="text-[11px] font-bold text-green-400">${selected.avgMealCost}</span>
                    </div>

                    {/* Best Months */}
                    <div className="flex items-center justify-between bg-white/[0.03] rounded-lg px-3 py-2">
                      <div className="flex items-center gap-2">
                        <Calendar size={13} className="text-gray-500" />
                        <span className="text-[11px] text-gray-400">Best Months</span>
                      </div>
                      <span className="text-[11px] font-bold text-blue-400">{formatBestMonths(selected.bestMonths)}</span>
                    </div>

                    {/* Cost of living bar */}
                    <div className="pt-1">
                      <div className="flex items-center justify-between mb-1">
                        <span className="text-[10px] text-gray-500 uppercase tracking-wider">Cost of Living Index</span>
                        <span className="text-[11px] font-bold text-gray-300">{selected.costOfLivingIndex}/120</span>
                      </div>
                      <div className="h-1.5 bg-gray-800 rounded-full overflow-hidden">
                        <div
                          className="h-full bg-gradient-to-r from-green-500 via-yellow-500 to-red-500 rounded-full transition-all"
                          style={{ width: `${Math.min((selected.costOfLivingIndex || 0) / 1.2, 100)}%` }}
                        />
                      </div>
                    </div>
                  </div>
                ) : (
                  <div className="text-center py-2">
                    <p className="text-[11px] text-gray-500">Capital city — not yet in Routiq network</p>
                    <p className="text-[10px] text-gray-600 mt-1">Coming soon to Mission Control</p>
                  </div>
                )}

                {/* Footer */}
                <div className="mt-3 pt-2 border-t border-white/[0.05]">
                  <div className="flex items-center justify-between">
                    <span className="text-[9px] text-gray-600 tracking-wider">
                      {selected.lat.toFixed(2)}°{selected.lat >= 0 ? 'N' : 'S'}, {Math.abs(selected.lng).toFixed(2)}°{selected.lng >= 0 ? 'E' : 'W'}
                    </span>
                    <span className="text-[9px] text-cyan-500/50 tracking-wider uppercase">Routiq Intel</span>
                  </div>
                </div>
              </div>
            </motion.div>
          )}
        </AnimatePresence>,
        document.body,
      )}
    </div>
  );
}
