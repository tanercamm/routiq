import { useRef, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { Canvas, useFrame } from '@react-three/fiber';
import { EffectComposer, Bloom } from '@react-three/postprocessing';
import * as THREE from 'three';

const SpaghettifiedSphere = () => {
  const meshRef = useRef<THREE.Mesh>(null);
  
  const shaderMaterial = useMemo(() => {
    return new THREE.ShaderMaterial({
      uniforms: {
        uTime: { value: 0 },
        uColorPrimary: { value: new THREE.Color("#ff3300") },
        uColorSecondary: { value: new THREE.Color("#ffaa00") }
      },
      vertexShader: `
        uniform float uTime;
        varying vec2 vUv;
        varying float vDisplacement;
        
        // Simplex noise function
        vec3 mod289(vec3 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
        vec4 mod289(vec4 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
        vec4 permute(vec4 x) { return mod289(((x*34.0)+1.0)*x); }
        vec4 taylorInvSqrt(vec4 r) { return 1.79284291400159 - 0.85373472095314 * r; }
        float snoise(vec3 v) {
          const vec2 C = vec2(1.0/6.0, 1.0/3.0);
          const vec4 D = vec4(0.0, 0.5, 1.0, 2.0);
          vec3 i  = floor(v + dot(v, C.yyy));
          vec3 x0 = v - i + dot(i, C.xxx);
          vec3 g = step(x0.yzx, x0.xyz);
          vec3 l = 1.0 - g;
          vec3 i1 = min(g.xyz, l.zxy);
          vec3 i2 = max(g.xyz, l.zxy);
          vec3 x1 = x0 - i1 + C.xxx;
          vec3 x2 = x0 - i2 + C.yyy;
          vec3 x3 = x0 - D.yyy;
          i = mod289(i);
          vec4 p = permute(permute(permute(
                    i.z + vec4(0.0, i1.z, i2.z, 1.0))
                  + i.y + vec4(0.0, i1.y, i2.y, 1.0))
                  + i.x + vec4(0.0, i1.x, i2.x, 1.0));
          float n_ = 0.142857142857;
          vec3 ns = n_ * D.wyz - D.xzx;
          vec4 j = p - 49.0 * floor(p * ns.z * ns.z);
          vec4 x_ = floor(j * ns.z);
          vec4 y_ = floor(j - 7.0 * x_);
          vec4 x = x_ *ns.x + ns.yyyy;
          vec4 y = y_ *ns.x + ns.yyyy;
          vec4 h = 1.0 - abs(x) - abs(y);
          vec4 b0 = vec4(x.xy, y.xy);
          vec4 b1 = vec4(x.zw, y.zw);
          vec4 s0 = floor(b0)*2.0 + 1.0;
          vec4 s1 = floor(b1)*2.0 + 1.0;
          vec4 sh = -step(h, vec4(0.0));
          vec4 a0 = b0.xzyw + s0.xzyw*sh.xxyy;
          vec4 a1 = b1.xzyw + s1.xzyw*sh.zzww;
          vec3 p0 = vec3(a0.xy,h.x);
          vec3 p1 = vec3(a0.zw,h.y);
          vec3 p2 = vec3(a1.xy,h.z);
          vec3 p3 = vec3(a1.zw,h.w);
          vec4 norm = taylorInvSqrt(vec4(dot(p0,p0), dot(p1,p1), dot(p2, p2), dot(p3,p3)));
          p0 *= norm.x;
          p1 *= norm.y;
          p2 *= norm.z;
          p3 *= norm.w;
          vec4 m = max(0.5 - vec4(dot(x0,x0), dot(x1,x1), dot(x2,x2), dot(x3,x3)), 0.0);
          m = m * m;
          return 42.0 * dot(m*m, vec4(dot(p0,x0), dot(p1,x1), dot(p2,x2), dot(p3,x3)));
        }

        void main() {
          vUv = uv;
          vec3 pos = position;
          
          // Spaghettification effect: stretch heavily along the Y axis
          float stretch = 1.0 + sin(uTime * 0.8) * 0.6;
          pos.y *= stretch * 2.5;
          
          // Pinch effect near the poles to simulate singularity pull
          float pinch = smoothstep(0.0, 1.0, abs(pos.y));
          pos.x *= (1.0 - pinch * 0.85);
          pos.z *= (1.0 - pinch * 0.85);

          // Add displacement noise for cracking crust
          float noise = snoise(pos * 3.0 + uTime * 0.4);
          pos += normal * noise * 0.2;
          vDisplacement = noise;

          gl_Position = projectionMatrix * modelViewMatrix * vec4(pos, 1.0);
        }
      `,
      fragmentShader: `
        uniform float uTime;
        uniform vec3 uColorPrimary;
        uniform vec3 uColorSecondary;
        varying vec2 vUv;
        varying float vDisplacement;

        void main() {
          // Base darkness of the devoured planet
          vec3 baseColor = vec3(0.01, 0.0, 0.02);
          
          // Calculate magma intensity based on displacement and time
          float magmaIntensity = smoothstep(-0.1, 0.5, vDisplacement);
          magmaIntensity += sin(uTime * 3.0 + vUv.y * 20.0) * 0.2;
          
          vec3 magmaColor = mix(uColorPrimary, uColorSecondary, vUv.x + sin(uTime)*0.5);
          
          // Final color mixes dark crust with glowing magma
          vec3 color = mix(baseColor, magmaColor, clamp(magmaIntensity, 0.0, 1.0));
          
          gl_FragColor = vec4(color, 1.0);
        }
      `,
      wireframe: false,
    });
  }, []);

  useFrame((state) => {
    if (meshRef.current) {
      meshRef.current.rotation.y += 0.008;
      meshRef.current.rotation.z += 0.004;
      shaderMaterial.uniforms.uTime.value = state.clock.getElapsedTime();
    }
  });

  return (
    <mesh ref={meshRef} material={shaderMaterial}>
      <sphereGeometry args={[1.5, 128, 128]} />
    </mesh>
  );
};

export default function NotFound() {
  const navigate = useNavigate();

  return (
    <div className="relative w-full h-screen bg-[#020106] text-white overflow-hidden flex flex-col items-center justify-center font-sans">
      
      {/* 3D Canvas Background */}
      <div className="absolute inset-0 z-0 pointer-events-none">
        <Canvas camera={{ position: [0, 0, 6], fov: 45 }}>
          <ambientLight intensity={0.1} />
          <SpaghettifiedSphere />
          <EffectComposer>
            <Bloom luminanceThreshold={0.1} luminanceSmoothing={0.9} height={300} intensity={2.5} mipmapBlur />
          </EffectComposer>
        </Canvas>
      </div>

      {/* Cinematic UI Overlay */}
      <div className="relative z-10 flex flex-col items-center text-center mt-[30vh]">
        <div className="inline-block px-3 py-1 mb-4 border border-red-500/20 bg-red-500/5 backdrop-blur-sm rounded-full">
          <p className="text-[10px] tracking-[0.3em] uppercase text-red-500 font-bold">
            Critical Navigational Failure
          </p>
        </div>

        <h1 className="text-6xl md:text-8xl font-black tracking-tighter text-transparent bg-clip-text bg-gradient-to-b from-white via-red-200 to-red-600 mb-2 leading-none drop-shadow-[0_0_30px_rgba(255,51,0,0.6)]">
          404
        </h1>
        
        <h2 className="text-xl md:text-2xl font-bold text-red-400 tracking-widest uppercase mb-4 drop-shadow-[0_0_10px_rgba(255,51,0,0.8)]">
          Sector Devoured
        </h2>

        <p className="text-slate-400 text-sm max-w-md mx-auto mb-10 leading-relaxed font-light px-6">
          The routing endpoint you attempted to reach has collapsed into a singularity. 
          Space-time distortion is critical. Immediate evacuation recommended.
        </p>

        <button
          onClick={() => navigate("/")}
          className="group relative flex items-center justify-center px-8 py-3.5 bg-transparent overflow-hidden rounded-xl border border-red-500/40 hover:border-red-400 transition-all duration-300"
        >
          {/* Glitch/Neon background hover effect */}
          <div className="absolute inset-0 w-full h-full bg-red-500/10 group-hover:bg-red-500/20 transition-all duration-300 backdrop-blur-md" />
          
          <span className="relative z-10 text-sm font-semibold tracking-widest uppercase text-red-300 group-hover:text-white transition-colors flex items-center gap-2">
            Return to Base
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="w-4 h-4 group-hover:-translate-x-1 transition-transform">
              <path d="M19 12H5M12 19l-7-7 7-7" />
            </svg>
          </span>
        </button>
      </div>

      {/* Cinematic Vignette */}
      <div className="absolute inset-0 z-20 pointer-events-none" style={{
        background: "radial-gradient(circle at center, transparent 20%, #020106 100%)"
      }} />
    </div>
  );
}
