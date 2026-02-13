import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, Cell } from 'recharts';
import { motion } from 'framer-motion';
import type { RouteOption } from '../types';

interface CostVsDurationChartProps {
    data: RouteOption[];
}

export const CostVsDurationChart = ({ data }: CostVsDurationChartProps) => {
    // Transform data for chart if needed, but RouteOption has totalCost
    const chartData = data.map(route => ({
        name: route.destination,
        cost: route.totalCost,
        popularity: route.popularityScore,
    }));

    const CustomTooltip = ({ active, payload, label }: any) => {
        if (active && payload && payload.length) {
            return (
                <div className="bg-slate-800 border border-slate-700 p-3 rounded-lg shadow-xl">
                    <p className="font-bold text-gray-200">{label}</p>
                    <p className="text-teal-400 font-medium">
                        Cost: ${payload[0].value.toLocaleString()}
                    </p>
                    <p className="text-gray-400 text-xs mt-1">
                        Popularity: {payload[0].payload.popularity}/10
                    </p>
                </div>
            );
        }
        return null;
    };

    return (
        <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.3 }}
            className="w-full h-[300px] mb-8 bg-slate-800/30 backdrop-blur-md rounded-2xl border border-white/5 p-6"
        >
            <h3 className="text-lg font-semibold text-gray-300 mb-4 px-2">Cost Comparison</h3>
            <ResponsiveContainer width="100%" height="100%">
                <BarChart data={chartData}>
                    <XAxis
                        dataKey="name"
                        stroke="#94a3b8"
                        fontSize={12}
                        tickLine={false}
                        axisLine={false}
                    />
                    <YAxis
                        stroke="#94a3b8"
                        fontSize={12}
                        tickLine={false}
                        axisLine={false}
                        tickFormatter={(value) => `$${value}`}
                    />
                    <Tooltip content={<CustomTooltip />} cursor={{ fill: 'rgba(255,255,255,0.05)' }} />
                    <Bar dataKey="cost" radius={[4, 4, 0, 0]}>
                        {chartData.map((entry, index) => (
                            <Cell key={`cell-${index}`} fill={entry.popularity > 8 ? '#2dd4bf' : '#3b82f6'} />
                        ))}
                    </Bar>
                </BarChart>
            </ResponsiveContainer>
        </motion.div>
    );
};
