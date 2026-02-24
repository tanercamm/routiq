import { useState } from 'react';
import { Search, Globe } from 'lucide-react';
import { Card } from './ui/Card';
import { Button } from './ui/Button';
import { Input } from './ui/Input';

// V1-compatible local type — HeroInput is no longer used by the V2 Dashboard
interface V1RouteRequest { passportCountry: string; totalBudget: number; durationDays: number; }

interface HeroInputProps {
    onSearch: (request: V1RouteRequest) => void;
    loading: boolean;
    initialBudget?: number;
    initialDays?: number;
}

export const HeroInput = ({ onSearch, loading, initialBudget, initialDays }: HeroInputProps) => {
    const [passport, setPassport] = useState('Turkey');
    const [budget, setBudget] = useState(initialBudget ?? 1000);
    const [days, setDays] = useState(initialDays ?? 7);

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        onSearch({
            passportCountry: passport,
            totalBudget: Number(budget),
            durationDays: Number(days),
        });
    };

    return (
        <Card className="w-full max-w-4xl mx-auto">
            <form onSubmit={handleSubmit} className="flex flex-col md:flex-row gap-4 items-end">
                <div className="flex-1 w-full">
                    <label className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5 flex items-center gap-2">
                        <Globe size={14} /> Passport
                    </label>
                    <select
                        value={passport}
                        onChange={(e) => setPassport(e.target.value)}
                        className="w-full bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg px-4 py-2.5 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-teal-500/40 focus:border-teal-500 transition-colors"
                    >
                        <option value="Turkey">Turkey</option>
                        <option value="Germany">Germany</option>
                        <option value="USA">USA</option>
                    </select>
                </div>

                <div className="flex-1 w-full">
                    <Input
                        label="Total Budget ($)"
                        type="number"
                        value={budget}
                        onChange={(e) => setBudget(Number(e.target.value))}
                        min={100}
                    />
                </div>

                <div className="flex-1 w-full">
                    <Input
                        label="Duration (Days)"
                        type="number"
                        value={days}
                        onChange={(e) => setDays(Number(e.target.value))}
                        min={1}
                        max={90}
                    />
                </div>

                <Button type="submit" disabled={loading} className="w-full md:w-auto h-[42px] flex items-center justify-center gap-2">
                    {loading ? 'Searching...' : <><Search size={16} /> Find Routes</>}
                </Button>
            </form>
        </Card>
    );
};
