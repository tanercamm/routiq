import { useEffect, useRef } from "react";
import { useNavigate } from "react-router-dom";
import { motion } from "framer-motion";
import Globe from "react-globe.gl";

export default function NotFound() {
  const navigate = useNavigate();
  const globeRef = useRef<any>(null);

  useEffect(() => {
    const globe = globeRef.current;
    if (!globe) return;

    // Slow, ominous inward spiral — no autoRotate, manual pull effect
    globe.controls().autoRotate = false;
    globe.controls().enableZoom = false;
    globe.controls().enableRotate = false;
    globe.pointOfView({ lat: 20, lng: 10, altitude: 2.5 });

    // Gradually zoom in — simulates being pulled into singularity
    let altitude = 2.5;
    let angle = 0;
    const interval = setInterval(() => {
      altitude = Math.max(0.8, altitude - 0.003);
      angle += 0.15;
      globe.pointOfView({
        lat: 20 + Math.sin(angle * 0.05) * 5,
        lng: 10 + angle * 0.3,
        altitude,
      });
    }, 50);

    return () => clearInterval(interval);
  }, []);

  return (
    <div className="relative min-h-screen bg-[#04010a] text-white overflow-hidden flex flex-col items-center justify-center">

      {/* Dark radial void center */}
      <div
        className="absolute inset-0 pointer-events-none"
        style={{
          background:
            "radial-gradient(ellipse 60% 60% at 50% 50%, rgba(0,0,0,0.95) 0%, rgba(20,0,40,0.7) 40%, transparent 70%)",
        }}
      />

      {/* Accretion disk glow — subtle orange halo */}
      <div
        className="absolute pointer-events-none"
        style={{
          width: 520,
          height: 520,
          top: "50%",
          left: "50%",
          transform: "translate(-50%, -50%)",
          borderRadius: "50%",
          background:
            "radial-gradient(ellipse at center, transparent 35%, rgba(180,60,0,0.08) 55%, rgba(255,100,20,0.15) 65%, rgba(180,60,0,0.06) 75%, transparent 85%)",
          animation: "pulse-ring 4s ease-in-out infinite",
        }}
      />

      {/* Globe */}
      <div
        className="absolute pointer-events-none select-none"
        style={{
          top: "50%",
          left: "50%",
          transform: "translate(-50%, -50%)",
          // Spaghettification: stretched vertically as it's "pulled in"
          animation: "spaghetti 7s ease-in-out infinite",
        }}
      >
        <Globe
          ref={globeRef}
          width={340}
          height={340}
          backgroundColor="rgba(0,0,0,0)"
          globeImageUrl="//unpkg.com/three-globe/example/img/earth-night.jpg"
          atmosphereColor="#ff4400"
          atmosphereAltitude={0.18}
          // No arcs, no points — stripped down, ominous
        />
      </div>

      {/* Lava cracks overlay on globe */}
      <div
        className="absolute pointer-events-none"
        style={{
          width: 340,
          height: 340,
          top: "50%",
          left: "50%",
          transform: "translate(-50%, -50%)",
          borderRadius: "50%",
          overflow: "hidden",
          animation: "spaghetti 7s ease-in-out infinite",
          mixBlendMode: "screen",
        }}
      >
        <svg viewBox="0 0 340 340" width="340" height="340" style={{ opacity: 0.6 }}>
          {/* Lava crack lines */}
          <path d="M140 100 Q165 130 155 160 Q145 190 170 210" stroke="#ff5500" strokeWidth="1.5" fill="none" opacity="0.7" />
          <path d="M170 90 Q185 120 175 145 Q162 168 180 195" stroke="#ff7700" strokeWidth="1" fill="none" opacity="0.5" />
          <path d="M110 155 Q135 165 145 185 Q155 205 140 225" stroke="#ff4400" strokeWidth="1" fill="none" opacity="0.6" />
          <path d="M195 130 Q210 155 200 175 Q190 195 205 215" stroke="#ff6600" strokeWidth="1" fill="none" opacity="0.4" />
          {/* Magma glow spots */}
          <circle cx="155" cy="160" r="4" fill="#ff5500" opacity="0.8">
            <animate attributeName="opacity" values="0.8;0.3;0.8" dur="2s" repeatCount="indefinite" />
          </circle>
          <circle cx="175" cy="195" r="3" fill="#ff7700" opacity="0.7">
            <animate attributeName="opacity" values="0.7;0.2;0.7" dur="2.5s" repeatCount="indefinite" />
          </circle>
          <circle cx="140" cy="185" r="2.5" fill="#ff4400" opacity="0.6">
            <animate attributeName="opacity" values="0.6;0.2;0.6" dur="1.8s" repeatCount="indefinite" />
          </circle>
          <circle cx="200" cy="175" r="2" fill="#ff6600" opacity="0.5">
            <animate attributeName="opacity" values="0.5;0.1;0.5" dur="3s" repeatCount="indefinite" />
          </circle>
        </svg>
      </div>

      {/* Content */}
      <div className="relative z-10 text-center mt-[420px]">
        <motion.p
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.2 }}
          className="text-[11px] tracking-[0.2em] uppercase text-orange-500/60 mb-3"
        >
          Routsky — Navigation Error
        </motion.p>

        <motion.h1
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3 }}
          className="text-[7rem] font-extrabold leading-none tracking-tighter text-transparent mb-2"
          style={{
            WebkitTextStroke: "1px rgba(255,100,30,0.4)",
          }}
        >
          404
        </motion.h1>

        <motion.p
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.4 }}
          className="text-white/50 text-sm mb-1"
        >
          Route not found
        </motion.p>

        <motion.p
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.5 }}
          className="text-white/20 text-xs tracking-widest uppercase mb-8"
        >
          This destination has been consumed by a singularity
        </motion.p>

        <motion.button
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.6 }}
          onClick={() => navigate("/")}
          className="px-6 py-2.5 text-sm font-medium border border-orange-500/30 text-orange-400/70 hover:border-orange-500/60 hover:text-orange-400 rounded-lg transition-all"
        >
          Return to Base
        </motion.button>
      </div>

      <style>{`
        @keyframes spaghetti {
          0%, 100% { transform: translate(-50%, -50%) scaleX(1) scaleY(1); filter: blur(0px); }
          30%       { transform: translate(-50%, -52%) scaleX(0.92) scaleY(1.06); filter: blur(0.3px); }
          60%       { transform: translate(-50%, -55%) scaleX(0.82) scaleY(1.15); filter: blur(0.8px); }
          80%       { transform: translate(-50%, -58%) scaleX(0.70) scaleY(1.25); filter: blur(1.5px); }
        }
        @keyframes pulse-ring {
          0%, 100% { opacity: 0.6; transform: translate(-50%,-50%) scale(1); }
          50%       { opacity: 1;   transform: translate(-50%,-50%) scale(1.04); }
        }
      `}</style>
    </div>
  );
}
