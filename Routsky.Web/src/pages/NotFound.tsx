import { useMemo, useRef } from "react";
import { useNavigate } from "react-router-dom";
import { Canvas, useFrame } from "@react-three/fiber";
import { Bloom, EffectComposer } from "@react-three/postprocessing";
import * as THREE from "three";

const EclipseAura = () => {
  const auraRef = useRef<THREE.Mesh>(null);

  const auraMaterial = useMemo(
    () =>
      new THREE.ShaderMaterial({
        uniforms: {
          uTime: { value: 0 },
        },
        transparent: true,
        depthWrite: false,
        blending: THREE.AdditiveBlending,
        vertexShader: `
          varying vec2 vUv;
          void main() {
            vUv = uv;
            gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
          }
        `,
        fragmentShader: `
          uniform float uTime;
          varying vec2 vUv;

          void main() {
            vec2 uv = vUv - 0.5;
            uv.y *= 0.3;

            float radius = length(uv);
            float ring = exp(-pow((radius - 0.29) / 0.06, 2.0));

            float angle = atan(uv.y, uv.x);
            float turbulence = 0.6 + 0.4 * sin(angle * 8.0 + uTime * 1.6);
            float shimmer = 0.8 + 0.2 * sin(uTime * 4.5 + radius * 24.0);
            float alpha = ring * turbulence * shimmer;

            vec3 inner = vec3(1.0, 0.53, 0.15);
            vec3 outer = vec3(1.0, 0.28, 0.08);
            vec3 color = mix(inner, outer, smoothstep(0.18, 0.42, radius));

            gl_FragColor = vec4(color, alpha);
          }
        `,
      }),
    []
  );

  useFrame((state) => {
    auraMaterial.uniforms.uTime.value = state.clock.getElapsedTime();
    if (auraRef.current) {
      auraRef.current.rotation.z += 0.0018;
    }
  });

  return (
    <mesh ref={auraRef} position={[0, 0, -0.8]}>
      <planeGeometry args={[8, 8]} />
      <primitive object={auraMaterial} attach="material" />
    </mesh>
  );
};

const EclipseCore = () => (
  <mesh>
    <sphereGeometry args={[1.42, 128, 128]} />
    <meshBasicMaterial color="#000000" />
  </mesh>
);

const EclipseScene = () => (
  <>
    <EclipseAura />
    <EclipseCore />
  </>
);

export default function NotFound() {
  const navigate = useNavigate();

  return (
    <div className="relative w-full h-screen bg-black text-white overflow-hidden flex flex-col items-center justify-center">
      <div className="absolute inset-0 z-0 pointer-events-none">
        <Canvas camera={{ position: [0, 0, 5.6], fov: 42 }}>
          <EclipseScene />
          <EffectComposer>
            <Bloom
              luminanceThreshold={0.05}
              luminanceSmoothing={0.8}
              intensity={2.8}
              mipmapBlur
            />
          </EffectComposer>
        </Canvas>
      </div>

      <div className="relative z-10 flex flex-col items-center text-center mt-[32vh] px-6">
        <p className="text-[11px] md:text-xs tracking-[0.42em] text-white/90 uppercase font-medium">
          404 | Sector Devoured
        </p>

        <button
          onClick={() => navigate("/")}
          className="mt-8 bg-transparent border border-white/20 text-white/80 rounded-full px-6 py-2 hover:border-white/40 hover:text-white transition-all text-sm tracking-[0.16em] uppercase"
        >
          Return to Base
        </button>
      </div>

      <div
        className="absolute inset-0 z-20 pointer-events-none"
        style={{
          background:
            "radial-gradient(circle at center, rgba(0,0,0,0) 22%, rgba(0,0,0,0.95) 100%)",
        }}
      />
    </div>
  );
}
