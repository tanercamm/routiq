// @ts-nocheck
import React, { useRef, useMemo } from 'react';
import { Canvas, useFrame, extend } from '@react-three/fiber';
import { OrbitControls, Effects } from '@react-three/drei';
import { UnrealBloomPass } from 'three-stdlib';
import { useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { useAuth } from '../context/AuthContext';
import * as THREE from 'three';

extend({ UnrealBloomPass });

// ─────────────────────────────────────────────────────────────────────────────
//  PARTICLE SWARM — Zodiac Wheel, Routsky edition
//
//  Changes from original Casberry export:
//  · Full green palette (#00ff88 → teal → deep space) — no orange/yellow
//  · Core sun → bright green-white pulse
//  · Zodiac clusters → cool green-teal gradient per sign
//  · Orbital dust → faint teal connecting arcs
//  · Deep stars → cold blue-green twinkle field
//  · Larger particle geometry for more impact
//  · Slower auto-rotate — calmer, more premium feel
//  · Bloom tuned for green wavelength (higher radius, lower threshold)
// ─────────────────────────────────────────────────────────────────────────────
const ParticleSwarm = () => {
  const meshRef = useRef<THREE.InstancedMesh>(null);
  const count = 4000;
  const dummy = useMemo(() => new THREE.Object3D(), []);
  const target = useMemo(() => new THREE.Vector3(), []);
  const pColor = useMemo(() => new THREE.Color(), []);
  const color  = pColor;

  const positions = useMemo(() => {
    const pos: THREE.Vector3[] = [];
    for (let i = 0; i < count; i++)
      pos.push(new THREE.Vector3(
        (Math.random() - 0.5) * 100,
        (Math.random() - 0.5) * 100,
        (Math.random() - 0.5) * 100,
      ));
    return pos;
  }, []);

  const material = useMemo(() => new THREE.MeshBasicMaterial({ color: 0xffffff }), []);
  // Minimal geometry for reduced visual noise
  const geometry = useMemo(() => new THREE.TetrahedronGeometry(0.15), []);

  // Fixed params — no interactive sliders on landing
  const SPREAD = 110;
  const SPEED  = 0.05;   // fluid, slow motion
  const GLOW   = 0.75;
  const DRIFT  = 0.12;

  useFrame(({ clock }) => {
    if (!meshRef.current) return;
    const time = clock.getElapsedTime();
    const ti   = time * SPEED;
    const PI2  = Math.PI * 2;

    for (let i = 0; i < count; i++) {
      const t     = i / count;
      const seed  = (i * 1.6180339887) % 1.0;
      const seed2 = (i * 0.7548776662) % 1.0;
      const seed3 = (i * 0.3819660113) % 1.0;

      let px = 0, py = 0, pz = 0;
      let h = 0, s = 0, l = 0;

      // ── 0–2 %  CORE NODE — bright green-white pulse ──────────────────
      if (t < 0.02) {
        const coreR = 3.5 + seed * 5;
        const coreA = seed2 * PI2 + ti * 0.9;
        px = Math.cos(coreA) * coreR * (1 + Math.sin(ti * 2.1 + seed * 10) * 0.25);
        pz = Math.sin(coreA) * coreR * (1 + Math.cos(ti * 1.8 + seed * 8)  * 0.25);
        py = (seed3 - 0.5) * 2.5 + Math.sin(ti * 3 + seed * 5) * 1.2;

        // Green-white core: hue 0.36–0.40, high lightness
        h = 0.37 + Math.sin(ti + seed * 4) * 0.02;
        s = 0.85;
        l = 0.70 + Math.sin(ti * 2.5 + seed * 7) * 0.18;

      // ── 2–30 %  ZODIAC RING — 12 clusters, teal-to-green gradient ────
      } else if (t < 0.30) {
        const localT     = (t - 0.02) / 0.28;
        const signIndex  = Math.floor(localT * 12) % 12;
        const baseAngle  = (signIndex / 12) * PI2;

        // Tight cluster around each node
        const clusterR = 2.5 + seed * 4.5;
        const clusterA = seed2 * PI2;
        const offsetX  = Math.cos(clusterA) * clusterR;
        const offsetZ  = Math.sin(clusterA) * clusterR;

        const ringAngle = baseAngle + ti * 0.12;
        const ringR     = SPREAD * 0.68;

        px = Math.cos(ringAngle) * ringR + offsetX;
        pz = Math.sin(ringAngle) * ringR + offsetZ;
        py = (seed3 - 0.5) * 3.5 + Math.sin(ti * 0.5 + seed * 3) * DRIFT * 10;

        // Each node steps from green (0.36) to teal (0.50) around the ring
        h = 0.36 + (signIndex / 12) * 0.14;
        s = 0.80 + seed * 0.15;
        l = 0.42 + Math.sin(ti + signIndex * 0.52 + seed * 2) * 0.13;

      // ── 30–55 %  ORBITAL DUST — faint teal arcs between nodes ────────
      } else if (t < 0.55) {
        const localT   = (t - 0.30) / 0.25;
        const orbitAng = localT * PI2 * 3 + ti * 0.09;
        const orbitR   = SPREAD * (0.48 + localT * 0.34);
        const wobble   = Math.sin(localT * PI2 * 8 + ti) * DRIFT * 11;

        px = Math.cos(orbitAng) * orbitR + Math.sin(seed * PI2) * wobble;
        pz = Math.sin(orbitAng) * orbitR + Math.cos(seed * PI2) * wobble;
        py = (seed3 - 0.5) * 5 + Math.sin(ti * 0.3 + localT * 4) * DRIFT * 7;

        // Cool teal, faint — appears as connecting fabric
        h = 0.48 + Math.sin(localT * 3 + ti * 0.2) * 0.06;
        s = 0.55 + GLOW * 0.12;
        l = 0.18 + GLOW * 0.13 + Math.sin(ti + localT * 5) * 0.06;

      // ── 55–100 %  DEEP FIELD — cold star background ───────────────────
      } else {
        const localT = (t - 0.55) / 0.45;
        const phi    = Math.acos(2.0 * seed - 1.0);
        const theta  = seed2 * PI2 + ti * 0.012;
        const r      = SPREAD * (1.15 + localT * 1.6);
        const sinPhi = Math.sin(phi);

        px = sinPhi * Math.cos(theta) * r;
        pz = sinPhi * Math.sin(theta) * r;
        py = Math.cos(phi) * r * 0.14;

        // Blue-green twinkle
        const twinkle = Math.sin(ti * 2.8 + i * 0.31) * 0.5 + 0.5;
        h = 0.50 + seed3 * 0.12;      // teal → cyan
        s = 0.20 + GLOW * 0.10;
        l = (0.06 + twinkle * 0.22) * GLOW;
      }

      target.set(px, py, pz);

      // Clamp & apply color
      h = ((h % 1.0) + 1.0) % 1.0;
      s = Math.max(0.0, Math.min(1.0, s));
      l = Math.max(0.02, Math.min(0.95, l));
      color.setHSL(h, s, l);

      positions[i].lerp(target, 0.09);
      dummy.position.copy(positions[i]);
      dummy.updateMatrix();
      meshRef.current.setMatrixAt(i, dummy.matrix);
      meshRef.current.setColorAt(i, pColor);
    }

    meshRef.current.instanceMatrix.needsUpdate = true;
    if (meshRef.current.instanceColor)
      meshRef.current.instanceColor.needsUpdate = true;
  });

  return <instancedMesh ref={meshRef} args={[geometry, material, count]} />;
};

// ─────────────────────────────────────────────────────────────────────────────
//  LANDING PAGE
// ─────────────────────────────────────────────────────────────────────────────
export default function LandingPage() {
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();

  // Authenticated users skip to dashboard immediately
  React.useEffect(() => {
    if (isAuthenticated) navigate('/home', { replace: true });
  }, [isAuthenticated, navigate]);

  const fadeUp = (delay = 0) => ({
    initial:    { opacity: 0, y: 20 },
    animate:    { opacity: 1, y: 0  },
    transition: { duration: 0.65, delay, ease: [0.16, 1, 0.3, 1] },
  });

  return (
    <div className="relative w-screen h-screen overflow-hidden bg-black">

      {/* ── Three.js Canvas ────────────────────────────────────────────── */}
      <div className="absolute inset-0 z-0">
        <Canvas
          camera={{ position: [0, 55, 85], fov: 55 }}
          gl={{ antialias: true, alpha: false }}
        >
          {/* Very long fog — keeps stars visible but fades hard edges */}
          <fog attach="fog" args={['#000000', 100, 260]} />
          <ParticleSwarm />
          <OrbitControls
            autoRotate
            autoRotateSpeed={0.18}
            enableZoom={false}
            enablePan={false}
            enableRotate={false}
          />
          <Effects disableGamma>
            {/* Subtle bloom for minimalist look */}
            <unrealBloomPass threshold={0.2} strength={0.8} radius={0.4} />
          </Effects>
        </Canvas>
      </div>

      {/* ── Radial vignette — focus on center text ─────────────────────── */}
      <div
        className="absolute inset-0 z-10 pointer-events-none"
        style={{
          background: [
            'radial-gradient(ellipse 60% 55% at 50% 50%, transparent 0%, rgba(0,0,0,0.45) 50%, rgba(0,0,0,0.88) 100%)',
          ].join(','),
        }}
      />

      {/* ── Bottom gradient — grounds the footer ──────────────────────── */}
      <div
        className="absolute bottom-0 left-0 right-0 z-10 h-40 pointer-events-none"
        style={{ background: 'linear-gradient(to top, rgba(0,0,0,0.85), transparent)' }}
      />

      {/* ── NAV ───────────────────────────────────────────────────────── */}
      <nav className="absolute top-0 left-0 right-0 z-20 flex items-center justify-between px-8 py-5 backdrop-blur-xl bg-white/[0.01] border-b border-white/[0.04]">
        {/* Replace inner content with your existing <Logo /> component if you have one */}
        <div className="flex items-center gap-2.5">
          <div className="w-7 h-7 rounded-full bg-white/5 border border-white/10 flex items-center justify-center shrink-0">
            <svg viewBox="0 0 24 24" className="w-3.5 h-3.5 fill-white/80">
              <path d="M21 15c0 4.418-4.03 8-9 8s-9-3.582-9-8M3 8l9-7 9 7M12 1v14" />
            </svg>
          </div>
          <div className="leading-none">
            <p className="text-white font-medium tracking-[-0.02em] text-[15px] leading-none">
              Rout<span className="text-white/50">sky</span>
            </p>
            <p className="text-[9px] tracking-[0.16em] text-white/25 uppercase mt-0.5">
              Orchestrating the world
            </p>
          </div>
        </div>

        <button
          onClick={() => navigate('/login')}
          className="text-white/40 hover:text-white/80 text-sm transition-colors duration-200"
        >
          Sign In
        </button>
      </nav>

      {/* ── HERO ──────────────────────────────────────────────────────── */}
      <div className="absolute inset-0 z-20 flex flex-col items-center justify-center text-center px-6 pointer-events-none">

        {/* Status pill */}
        <motion.div {...fadeUp(0.1)} className="inline-flex items-center gap-2.5 mb-10 pointer-events-auto px-3.5 py-1.5 rounded-full border border-white/[0.05] bg-white/[0.02] backdrop-blur-md">
          <span className="relative flex h-1.5 w-1.5">
            <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-[#00e5ff] opacity-50" />
            <span className="relative inline-flex rounded-full h-1.5 w-1.5 bg-[#00e5ff]/80" />
          </span>
          <span className="text-white/60 text-[10px] tracking-[0.2em] uppercase font-medium">
            System Online
          </span>
        </motion.div>

        {/* Main headline */}
        <motion.h1
          {...fadeUp(0.2)}
          className="font-medium leading-[1.0] tracking-[-0.05em] text-white mb-8"
          style={{ fontSize: 'clamp(3.2rem, 7.5vw, 6.5rem)' }}
        >
          Orchestrate the
          <br />
          <span
            style={{
              background: 'linear-gradient(180deg, #ffffff 0%, rgba(255, 255, 255, 0.4) 100%)',
              WebkitBackgroundClip: 'text',
              WebkitTextFillColor: 'transparent',
              backgroundClip: 'text',
            }}
          >
            World.
          </span>
        </motion.h1>

        {/* Subline */}
        <motion.p
          {...fadeUp(0.3)}
          className="text-white/40 leading-relaxed max-w-[480px] mb-14 font-light tracking-wide"
          style={{ fontSize: 'clamp(0.95rem, 1.4vw, 1.1rem)' }}
        >
          Deterministic route generation across 150 countries.
          Visa intelligence, cost analysis, and real-time safety
          in one decision engine.
        </motion.p>

        {/* CTAs */}
        <motion.div
          {...fadeUp(0.4)}
          className="flex items-center gap-5 pointer-events-auto"
        >
          <button
            onClick={() => navigate('/register')}
            className="px-8 py-3 bg-[#0a0a0a] border border-white/10 text-white/90 text-[13px] tracking-wide font-medium rounded-full shadow-[0_0_15px_rgba(0,229,255,0.05)] hover:bg-[#111111] hover:border-white/20 hover:shadow-[0_0_25px_rgba(0,229,255,0.15)] transition-all duration-300 active:scale-[0.98] backdrop-blur-md"
          >
            Get Started
          </button>
          <button
            onClick={() => navigate('/login')}
            className="px-8 py-3 border border-transparent text-white/50 text-[13px] tracking-wide font-medium rounded-full hover:text-white/90 transition-all duration-300"
          >
            Sign In →
          </button>
        </motion.div>
      </div>

      {/* ── FOOTER STATS ──────────────────────────────────────────────── */}
      <motion.div
        {...fadeUp(0.6)}
        className="absolute bottom-0 left-0 right-0 z-20 flex items-end justify-between px-8 py-6 pointer-events-none backdrop-blur-xl bg-white/[0.01] border-t border-white/[0.04]"
      >
        <p className="text-white/18 text-[10px] tracking-[0.14em] uppercase">
          © 2026 Routsky Inc.
        </p>
        <div className="flex gap-7">
          {[
            { num: '320+', label: 'Active Nodes'    },
            { num: '150',  label: 'Countries'       },
            { num: '7',    label: 'Regions'         },
          ].map((s) => (
            <div key={s.label} className="text-right">
              <p className="text-[10px] tracking-[0.1em] uppercase text-white/22">{s.label}</p>
              <p className="text-white/80 text-base font-bold leading-tight">{s.num}</p>
            </div>
          ))}
        </div>
      </motion.div>
    </div>
  );
}
