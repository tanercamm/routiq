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
            initial={{ opacity: 0, y: 15 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: index * 0.08 }}
        >
            <Card hoverEffect className="h-full flex flex-col justify-between border-t-2 border-t-teal-500">
                <div>
                    <div className="flex justify-between items-start mb-4">
                        <h3 className="text-lg font-bold text-gray-900 dark:text-white">
                            {option.routeType}
                        </h3>
                        <span className="bg-teal-50 dark:bg-teal-500/10 text-teal-700 dark:text-teal-300 text-xs px-2 py-0.5 rounded-full border border-teal-200 dark:border-teal-500/20 font-medium">
                            Recommended
                        </span>
                    </div>

                    <p className="text-sm text-gray-600 dark:text-gray-400 mb-5 leading-relaxed">
                        {option.description}
                    </p>

                    <div className="space-y-3 mb-5">
                        {option.stops.map((stop, i) => (
                            <div key={i} className="flex items-start gap-3 p-3 bg-gray-50 dark:bg-gray-700/30 rounded-lg border border-gray-100 dark:border-gray-700/40">
                                <div className="mt-0.5 text-teal-500">
                                    <MapPin size={14} />
                                </div>
                                <div>
                                    <p className="font-medium text-sm text-gray-900 dark:text-white">{stop.city}, {stop.country}</p>
                                    <div className="flex gap-3 text-xs text-gray-500 dark:text-gray-400 mt-1">
                                        <span className="flex items-center gap-1"><Calendar size={11} /> {stop.days} days</span>
                                        <span className="flex items-center gap-1"><Sun size={11} /> {stop.climate}</span>
                                        <span className="flex items-center gap-1 text-green-600 dark:text-green-400"><CheckCircle size={11} /> {stop.visaStatus}</span>
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>
                </div>

                <div className="pt-4 border-t border-gray-200 dark:border-gray-700/60 flex justify-between items-end">
                    <div>
                        <span className="text-xs text-gray-500 block mb-0.5">Total Est. Cost</span>
                        <span className="text-2xl font-bold text-gray-900 dark:text-white tracking-tight">
                            <span className="text-teal-500">$</span>
                            {option.totalEstimatedCost.toLocaleString()}
                        </span>
                    </div>
                    <button
                        onClick={() => onViewItinerary(option)}
                        className="text-sm font-medium text-teal-600 dark:text-teal-400 hover:text-white hover:bg-teal-600 dark:hover:bg-teal-500 rounded-lg px-4 py-2 border border-teal-200 dark:border-teal-500/30 hover:border-transparent transition-all"
                    >
                        View Itinerary
                    </button>
                </div>
            </Card>
        </motion.div>
    );
};
