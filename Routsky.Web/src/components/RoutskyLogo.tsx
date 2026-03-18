// RoutskyLogo.tsx
// Kullanım:
//   <RoutskyLogo size="sm" />   — navbar
//   <RoutskyLogo size="lg" />   — login / register sayfası
//   <RoutskyLogo iconOnly />    — sadece ikon

import { useTheme } from "../context/ThemeContext";

type Size = "xs" | "sm" | "md" | "lg";

interface RoutskyLogoProps {
  size?: Size;
  iconOnly?: boolean;
  wordmarkOnly?: boolean;
  className?: string;
}

const SIZES: Record<Size, { icon: number; fontSize: number; slogan: number; gap: number }> = {
  xs: { icon: 20, fontSize: 14, slogan: 0,  gap: 6  },
  sm: { icon: 28, fontSize: 18, slogan: 0,  gap: 8  },
  md: { icon: 36, fontSize: 22, slogan: 9,  gap: 10 },
  lg: { icon: 52, fontSize: 30, slogan: 10, gap: 14 },
};

const COLORS = {
  light: {
    circle:      "#1D4ED8",
    plane:       "#F4F4F5",
    textPrimary: "#111827",
    textAccent:  "#1D4ED8",
    slogan:      "#9CA3AF",
  },
  dark: {
    circle:      "#3B82F6",
    plane:       "#0C111D",
    textPrimary: "#F9FAFB",
    textAccent:  "#60A5FA",
    slogan:      "#4B5563",
  },
};

function PlaneIcon({
  size,
  circleColor,
  planeColor,
}: {
  size: number;
  circleColor: string;
  planeColor: string;
}) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 100 100"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      aria-hidden="true"
    >
      <circle cx="50" cy="50" r="48" fill={circleColor} />
      <rect x="18" y="45" width="52" height="10" rx="5" fill={planeColor} />
      <polygon points="30,45 18,20 50,45" fill={planeColor} />
      <polygon points="30,55 18,78 48,55" fill={planeColor} opacity="0.88" />
      <rect x="60" y="33" width="8" height="13" rx="4" fill={planeColor} />
    </svg>
  );
}

export function RoutskyLogo({
  size = "md",
  iconOnly = false,
  wordmarkOnly = false,
  className = "",
}: RoutskyLogoProps) {
  const { theme } = useTheme();

  const colors = COLORS[theme];
  const dims = SIZES[size];

  if (iconOnly) {
    return (
      <PlaneIcon
        size={dims.icon}
        circleColor={colors.circle}
        planeColor={colors.plane}
      />
    );
  }

  return (
    <div
      className={`routsky-logo ${className}`}
      style={{ display: "flex", alignItems: "center", gap: dims.gap }}
    >
      {!wordmarkOnly && (
        <PlaneIcon
          size={dims.icon}
          circleColor={colors.circle}
          planeColor={colors.plane}
        />
      )}
      <div style={{ display: "flex", flexDirection: "column", gap: 0 }}>
        <span
          style={{
            fontSize: dims.fontSize,
            fontWeight: 700,
            letterSpacing: "-0.3px",
            lineHeight: 1,
            color: colors.textPrimary,
          }}
        >
          <span style={{ color: colors.textAccent }}>Rout</span>sky
        </span>
        {dims.slogan > 0 && (
          <span
            style={{
              fontSize: dims.slogan,
              fontWeight: 400,
              letterSpacing: "1.6px",
              color: colors.slogan,
              textTransform: "uppercase" as const,
              marginTop: 3,
              lineHeight: 1,
            }}
          >
            Orchestrating the World
          </span>
        )}
      </div>
    </div>
  );
}