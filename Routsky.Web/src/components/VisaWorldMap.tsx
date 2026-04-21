import { useEffect, useMemo, useRef, useState } from 'react';
import type { MouseEvent as ReactMouseEvent } from 'react';
import { geoNaturalEarth1, geoPath } from 'd3-geo';
import type { Feature, FeatureCollection, Geometry, GeoJsonProperties } from 'geojson';
import { getGlobalVisaMap } from '../api/routskyApi';
import { useAuth } from '../context/AuthContext';
import type { GlobalVisaCountryStatus, VisaMapStatus } from '../types';

/**
 * Low-res Natural Earth (Admin 0) GeoJSON.
 * ~300 KB, cached by the CDN, has both ISO alpha-2 and alpha-3 in properties.
 * Properties of interest: ISO_A2_EH, ISO_A2, ISO_A3_EH, ISO_A3, NAME, ADMIN.
 */
const GEOJSON_URL =
  'https://cdn.jsdelivr.net/gh/nvkelso/natural-earth-vector@master/geojson/ne_110m_admin_0_countries.geojson';

const STATUS_COLORS: Record<VisaMapStatus, string> = {
  VisaFree: '#00E676',
  EVisaOrOnArrival: '#FFEA00',
  ConditionalOrTimeLimited: '#FF9100',
  VisaRequired: '#FF1744',
  BannedOrRefused: '#111111',
  Unknown: '#2E3A52',
};

const STATUS_LABELS: Record<VisaMapStatus, string> = {
  VisaFree: 'Visa-Free',
  EVisaOrOnArrival: 'e-Visa / On Arrival',
  ConditionalOrTimeLimited: 'Conditional',
  VisaRequired: 'Visa Required',
  BannedOrRefused: 'Banned / Refused',
  Unknown: 'Unknown',
};

type CountryFeature = Feature<Geometry, GeoJsonProperties>;

interface TooltipState {
  x: number;
  y: number;
  countryName: string;
  status: VisaMapStatus;
}

/** Minimal alpha-3 → alpha-2 fallback for countries where Natural Earth's ISO_A2 is missing/"-99". */
const ISO_A3_TO_A2_FALLBACK: Record<string, string> = {
  FRA: 'FR',
  NOR: 'NO',
  KOS: 'XK',
  SOL: 'SO',
  CYN: 'CY',
  SAH: 'EH',
};

/** Extract an uppercase ISO alpha-2 code from a GeoJSON feature, trying every known property shape. */
function getCountryCode(feature: CountryFeature): string {
  const p = (feature.properties ?? {}) as Record<string, unknown>;

  const directA2 = [
    p.ISO_A2_EH,
    p.ISO_A2,
    p.iso_a2,
    p.iso_a2_eh,
    p['ISO3166-1-Alpha-2'],
  ];
  for (const v of directA2) {
    if (typeof v === 'string') {
      const code = v.trim().toUpperCase();
      if (code.length === 2 && code !== '-9' && code !== '-99' && code !== 'NA') {
        return code;
      }
    }
  }

  const directA3 = [p.ISO_A3_EH, p.ISO_A3, p.iso_a3, p.iso_a3_eh, p['ISO3166-1-Alpha-3']];
  for (const v of directA3) {
    if (typeof v === 'string') {
      const code = v.trim().toUpperCase();
      if (code.length === 3 && code !== '-99' && ISO_A3_TO_A2_FALLBACK[code]) {
        return ISO_A3_TO_A2_FALLBACK[code];
      }
    }
  }
  return '';
}

function getCountryName(feature: CountryFeature): string {
  const p = (feature.properties ?? {}) as Record<string, unknown>;
  const candidates = [p.NAME, p.NAME_LONG, p.ADMIN, p.name, p.admin];
  for (const v of candidates) {
    if (typeof v === 'string' && v.trim()) return v.trim();
  }
  return 'Unknown';
}

export function VisaWorldMap() {
  const { user } = useAuth();
  const passportCode = (user?.passports?.[0] ?? 'TR').toUpperCase();

  const containerRef = useRef<HTMLDivElement>(null);
  const [size, setSize] = useState({ width: 1000, height: 520 });
  const [worldFeatures, setWorldFeatures] = useState<CountryFeature[]>([]);
  const [visaMap, setVisaMap] = useState<Record<string, GlobalVisaCountryStatus>>({});
  const [loading, setLoading] = useState(true);
  const [geoLoading, setGeoLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [tooltip, setTooltip] = useState<TooltipState | null>(null);

  // ── Measure the map container so the SVG fills the available space ──
  useEffect(() => {
    const el = containerRef.current;
    if (!el) return;
    const observer = new ResizeObserver(entries => {
      const rect = entries[0].contentRect;
      const width = Math.max(640, Math.floor(rect.width));
      const height = Math.max(360, Math.floor(rect.height));
      setSize({ width, height });
    });
    observer.observe(el);
    return () => observer.disconnect();
  }, []);

  // ── Load GeoJSON once ──
  useEffect(() => {
    let active = true;
    const load = async () => {
      try {
        const response = await fetch(GEOJSON_URL);
        if (!response.ok) throw new Error(`GeoJSON HTTP ${response.status}`);
        const geo = (await response.json()) as FeatureCollection<Geometry, GeoJsonProperties>;
        if (active) setWorldFeatures(geo.features);
      } catch (err) {
        console.error('[VisaWorldMap] Failed to load world GeoJSON', err);
        if (active) setError('Failed to load world map geometry.');
      } finally {
        if (active) setGeoLoading(false);
      }
    };
    load();
    return () => {
      active = false;
    };
  }, []);

  // ── Load visa status map whenever the user's primary passport changes ──
  useEffect(() => {
    let active = true;
    const loadVisaData = async () => {
      setLoading(true);
      setError(null);
      try {
        const response = await getGlobalVisaMap(passportCode);
        if (!active) return;
        const countries = response.countries ?? {};
        const normalized: Record<string, GlobalVisaCountryStatus> = {};
        for (const [code, value] of Object.entries(countries)) {
          normalized[code.trim().toUpperCase()] = value;
        }
        setVisaMap(normalized);
      } catch (err: unknown) {
        console.error('[VisaWorldMap] Failed to load visa map', err);
        const message =
          (err as { response?: { data?: { message?: string } } })?.response?.data?.message ??
          'Failed to load visa intelligence map.';
        if (active) setError(message);
      } finally {
        if (active) setLoading(false);
      }
    };

    loadVisaData();
    return () => {
      active = false;
    };
  }, [passportCode]);

  // ── Projection + path are recomputed when the container resizes ──
  const projection = useMemo(
    () =>
      geoNaturalEarth1()
        .translate([size.width / 2, size.height / 2])
        .scale(size.width / 6.2),
    [size.height, size.width],
  );
  const path = useMemo(() => geoPath(projection), [projection]);

  const statusForFeature = (feature: CountryFeature): VisaMapStatus => {
    const code = getCountryCode(feature);
    if (!code) return 'Unknown';
    return visaMap[code]?.status ?? 'Unknown';
  };

  const handleMouseMove = (event: ReactMouseEvent<SVGPathElement>, feature: CountryFeature) => {
    setTooltip({
      x: event.clientX + 14,
      y: event.clientY + 14,
      countryName: getCountryName(feature),
      status: statusForFeature(feature),
    });
  };

  const busy = geoLoading || loading;

  return (
    <div className="flex h-full w-full flex-col gap-3">
      {/* Header row */}
      <div className="flex shrink-0 items-center justify-between px-1">
        <div>
          <h3 className="text-sm font-black tracking-wide text-white">Visa Intel 2D</h3>
          <p className="text-[11px] text-gray-400">
            Passport: <span className="font-semibold text-blue-300">{passportCode}</span>
            <span className="ml-2 text-gray-500">
              · {Object.keys(visaMap).length} countries classified
            </span>
          </p>
        </div>
        {busy && <span className="text-[11px] text-blue-300">Loading visa intel...</span>}
      </div>

      {error && (
        <div className="shrink-0 rounded-lg bg-red-500/10 px-3 py-2 text-xs text-red-300">
          {error}
        </div>
      )}

      {/* Map fills remaining space */}
      <div
        ref={containerRef}
        className="relative flex-1 min-h-[360px] overflow-hidden rounded-2xl border border-slate-800/80 bg-[#0a1628] shadow-2xl"
      >
        <svg
          viewBox={`0 0 ${size.width} ${size.height}`}
          preserveAspectRatio="xMidYMid meet"
          className="absolute inset-0 h-full w-full"
        >
          <rect x={0} y={0} width={size.width} height={size.height} fill="#071124" />
          <g>
            {worldFeatures.map(feature => {
              const status = statusForFeature(feature);
              const d = path(feature);
              if (!d) return null;
              return (
                <path
                  key={String(feature.id ?? getCountryCode(feature) ?? getCountryName(feature))}
                  d={d}
                  fill={STATUS_COLORS[status]}
                  stroke="#0f172a"
                  strokeWidth={0.4}
                  onMouseMove={event => handleMouseMove(event, feature)}
                  onMouseLeave={() => setTooltip(null)}
                  style={{ transition: 'fill 160ms ease', cursor: 'pointer' }}
                />
              );
            })}
          </g>
        </svg>
      </div>

      {/* Tooltip: STRICTLY country name + visa status only. No safety/cost/etc. */}
      {tooltip && (
        <div
          className="pointer-events-none fixed z-[1100] min-w-[160px] rounded-lg border border-slate-700 bg-[#050a18]/95 px-3 py-2 text-xs text-white shadow-2xl backdrop-blur"
          style={{ top: tooltip.y, left: tooltip.x }}
        >
          <div className="font-bold">{tooltip.countryName}</div>
          <div className="flex items-center gap-2 text-gray-300">
            <span
              className="inline-block h-2 w-2 rounded-full"
              style={{ backgroundColor: STATUS_COLORS[tooltip.status] }}
            />
            {STATUS_LABELS[tooltip.status]}
          </div>
        </div>
      )}

      {/* Legend */}
      <div className="shrink-0 rounded-xl border border-slate-700/60 bg-[#071124]/80 p-3">
        <h4 className="mb-2 text-[10px] font-bold uppercase tracking-[0.2em] text-gray-400">
          Visa Legend
        </h4>
        <div className="grid grid-cols-2 gap-2 text-[11px] sm:grid-cols-3 lg:grid-cols-6">
          {(Object.keys(STATUS_COLORS) as VisaMapStatus[]).map(status => (
            <div
              key={status}
              className="flex items-center gap-2 rounded-lg border px-2.5 py-1.5"
              style={{
                borderColor: `${STATUS_COLORS[status]}40`,
                backgroundColor: `${STATUS_COLORS[status]}10`,
              }}
            >
              <span
                className="inline-block h-3 w-3 shrink-0 rounded-full"
                style={{
                  backgroundColor: STATUS_COLORS[status],
                  boxShadow: `0 0 6px ${STATUS_COLORS[status]}60`,
                }}
              />
              <span className="font-medium leading-tight text-gray-200">
                {STATUS_LABELS[status]}
              </span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
