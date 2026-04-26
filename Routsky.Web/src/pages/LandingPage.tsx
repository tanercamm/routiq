import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { motion } from "framer-motion";
import { useAuth } from "../context/AuthContext";

const fadeUpVariants: any = {
  hidden: { opacity: 0, y: 30 },
  visible: { opacity: 1, y: 0, transition: { duration: 0.8, ease: [0.16, 1, 0.3, 1] } },
};

const staggerContainer: any = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: {
      staggerChildren: 0.15,
      delayChildren: 0.2,
    },
  },
};

export default function LandingPage() {
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();

  useEffect(() => {
    if (isAuthenticated) {
      navigate("/home", { replace: true });
    }
  }, [isAuthenticated, navigate]);

  return (
    <div className="relative min-h-screen bg-black text-white selection:bg-white/20 overflow-hidden flex flex-col">
      <div className="absolute inset-0 pointer-events-none">
        <div className="absolute left-1/2 top-1/2 h-[34rem] w-[34rem] -translate-x-1/2 -translate-y-1/2 rounded-full bg-white/15 blur-[180px]" />
      </div>

      {/* Navigation Bar */}
      <nav className="relative z-20 flex items-center justify-between px-8 py-7 max-w-7xl mx-auto w-full">
        <div className="flex items-center gap-3">
          <span className="font-medium text-sm tracking-[0.2em] uppercase text-gray-200">
            Routsky
          </span>
        </div>
        <div className="flex items-center gap-4">
          <button
            onClick={() => navigate("/login")}
            className="text-sm tracking-wide font-medium text-gray-400 hover:text-white transition-colors"
          >
            Sign In
          </button>
        </div>
      </nav>

      {/* Main Content */}
      <main className="relative z-10 flex-1 flex flex-col items-center justify-center px-6 text-center">
        <motion.div
          variants={staggerContainer}
          initial="hidden"
          animate="visible"
          className="max-w-4xl mx-auto flex flex-col items-center"
        >
          <motion.h1 
            variants={fadeUpVariants}
            className="text-5xl md:text-7xl lg:text-8xl font-semibold tracking-tighter text-transparent bg-clip-text bg-gradient-to-b from-white to-gray-500 mb-6 leading-[1.1]"
          >
            Orchestrate the World.
          </motion.h1>

          <motion.p 
            variants={fadeUpVariants}
            className="text-lg md:text-xl text-gray-400 max-w-2xl mb-12 leading-relaxed font-light"
          >
            Advanced AI-driven route intelligence and travel group synchronization. 
            Experience real-time visa analytics, deterministic routing, and global node orchestration in one unified interface.
          </motion.p>

          <motion.div variants={fadeUpVariants} className="flex flex-col sm:flex-row items-center gap-4 w-full justify-center">
            <button
              onClick={() => navigate("/home")}
              className="w-full sm:w-auto bg-white text-black rounded-full px-6 py-2 font-medium hover:bg-gray-200 transition-all"
            >
              Launch Dashboard
            </button>
            <button
              onClick={() => navigate("/visa-intel")}
              className="w-full sm:w-auto bg-transparent border border-gray-800 text-gray-300 rounded-full px-6 py-2 hover:border-gray-600 transition-all"
            >
              Explore Visa Intel
            </button>
          </motion.div>
        </motion.div>
      </main>

      <footer className="relative z-10 w-full border-t border-gray-900 bg-black/80 backdrop-blur-sm">
        <div className="max-w-7xl mx-auto px-8 py-6 flex flex-col sm:flex-row items-center justify-between gap-4">
          <p className="text-xs text-gray-600 uppercase tracking-[0.2em] font-medium">
            &copy; {new Date().getFullYear()} Routsky Inc.
          </p>
          <div className="flex gap-8">
            <div className="flex flex-col items-end">
              <span className="text-[10px] text-gray-500 uppercase tracking-[0.18em]">Active Nodes</span>
              <span className="text-sm font-mono text-gray-300">320+</span>
            </div>
            <div className="flex flex-col items-end">
              <span className="text-[10px] text-gray-500 uppercase tracking-[0.18em]">Global Coverage</span>
              <span className="text-sm font-mono text-gray-300">150 Countries</span>
            </div>
          </div>
        </div>
      </footer>
    </div>
  );
}
