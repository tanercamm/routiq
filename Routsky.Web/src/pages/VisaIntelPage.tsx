import { useTheme } from '../context/ThemeContext';
import { VisaWorldMap } from '../components/VisaWorldMap';

export function VisaIntelPage() {
  const { theme } = useTheme();
  const isLight = theme === 'light';
  return (
    <div className={`flex flex-col h-[calc(100vh-4rem)] overflow-hidden px-4 pb-6 pt-4 sm:px-6 lg:px-8 transition-colors duration-700 ${isLight ? 'bg-[#F5F5F7]' : 'bg-[#020308]'}`}>
      <VisaWorldMap />
    </div>
  );
}
