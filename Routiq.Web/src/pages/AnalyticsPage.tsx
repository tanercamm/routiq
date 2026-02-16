import { motion } from 'framer-motion';
import { BarChart3 } from 'lucide-react';

export const AnalyticsPage = () => {
    return (
        <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
            <motion.div
                initial={{ opacity: 0, y: -10 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.4 }}
                className="text-center py-20"
            >
                <div className="w-16 h-16 rounded-2xl bg-blue-50 dark:bg-blue-500/10 flex items-center justify-center mx-auto mb-6">
                    <BarChart3 size={28} className="text-blue-600 dark:text-blue-400" />
                </div>
                <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-3 tracking-tight">
                    Analytics
                </h1>
                <p className="text-base text-gray-400 dark:text-gray-500 max-w-md mx-auto">
                    Advanced travel analytics and insights are coming soon. Track spending trends, popular destinations, and more.
                </p>
            </motion.div>
        </main>
    );
};
