import { useState } from 'react';
import { Search, Globe } from 'lucide-react';
import { Card } from './ui/Card';
import { Button } from './ui/Button';
import { Input } from './ui/Input';
import type { RouteRequest } from '../types';

interface HeroInputProps {
    onSearch: (request: RouteRequest) => void;
    loading: boolean;
}

export const HeroInput = ({ onSearch, loading }: HeroInputProps) => {
    const [passport, setPassport] = useState('Turkey');
    const [budget, setBudget] = useState(1000);
    const [days, setDays] = useState(7);

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        onSearch({
            passportCountry: passport,
            totalBudget: Number(budget),
            durationDays: Number(days),
        });
    };

    return (
        <Card className="w-full max-w-4xl mx-auto bg-white/10 backdrop-blur-xl border-white/20">
            <form onSubmit={handleSubmit} className="flex flex-col md:flex-row gap-4 items-end">
                <div className="flex-1 w-full">
                    <label className="text-sm font-medium text-gray-300 mb-2 flex items-center gap-2">
                        <Globe size={16} /> Passport
                    </label>
                    <select
                        value={passport}
                        onChange={(e) => setPassport(e.target.value)}
                        className="w-full bg-white/5 border border-white/10 rounded-lg px-4 py-2.5 text-white focus:outline-none focus:ring-2 focus:ring-teal-400"
                    >
                        <option value="Turkey" className="text-black">Turkey</option>
                        <option value="Germany" className="text-black">Germany</option>
                        <option value="USA" className="text-black">USA</option>
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
                    {loading ? 'Searching...' : <><Search size={18} /> Find Routes</>}
                </Button>
            </form>
        </Card>
    );
};
