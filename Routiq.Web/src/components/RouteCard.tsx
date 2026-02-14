import { motion } from 'framer-motion';
import type { RouteOption } from '../types';
import { Card } from './ui/Card';
import { MapPin, Calendar, Sun, CheckCircle } from 'lucide-react';

interface RouteCardProps {
    option: RouteOption;
    index: number;
    onViewItinerary: (option: RouteOption) => void;
}

export const RouteCard = ({ option, index, onViewItinerary }: RouteCardProps) => {
    return (
        <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: index * 0.1 }}
        >
            <Card hoverEffect className="h-full flex flex-col justify-between border-t-4 border-t-teal-400">
                <div>
                    <div className="flex justify-between items-start mb-4">
                        <h3 className="text-xl font-bold bg-gradient-to-r from-teal-200 to-white bg-clip-text text-transparent">
                            {option.routeType}
                        </h3>
                        <span className="bg-teal-500/20 text-teal-200 text-xs px-2 py-1 rounded-full border border-teal-500/30">
                            Recommended
                        </span>
                    </div>

                    <p className="text-gray-300 mb-6 text-sm leading-relaxed">
                        {option.description}
                    </p>

                    <div className="space-y-4 mb-6">
                        {option.stops.map((stop, i) => (
                            <div key={i} className="flex items-start gap-3 p-3 bg-white/5 rounded-lg">
                                <div className="mt-1 text-teal-400">
                                    <MapPin size={16} />
                                </div>
                                <div>
                                    <p className="font-semibold text-white">{stop.city}, {stop.country}</p>
                                    <div className="flex gap-3 text-xs text-gray-400 mt-1">
                                        <span className="flex items-center gap-1"><Calendar size={12} /> {stop.days} days</span>
                                        <span className="flex items-center gap-1"><Sun size={12} /> {stop.climate}</span>
                                        <span className="flex items-center gap-1 text-green-400"><CheckCircle size={12} /> {stop.visaStatus}</span>
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>
                </div>

                <div className="pt-4 border-t border-white/10 flex justify-between items-end">
                    <div>
                        <span className="text-xs text-gray-400 block mb-1">Total Est. Cost</span>
                        <span className="text-3xl font-bold text-white flex items-center md:text-2xl lg:text-3xl tracking-tight">
                            <span className="text-teal-400 mr-0.5">$</span>
                            {option.totalEstimatedCost.toLocaleString()}
                        </span>
                    </div>
                    <button
                        onClick={() => onViewItinerary(option)}
                        className="text-sm font-semibold text-teal-300 hover:text-white transition-all bg-teal-500/10 hover:bg-teal-500 rounded-lg px-4 py-2 border border-teal-500/20 hover:border-transparent"
                    >
                        View Itinerary
                    </button>
                </div>
            </Card>
        </motion.div>
    );
};

