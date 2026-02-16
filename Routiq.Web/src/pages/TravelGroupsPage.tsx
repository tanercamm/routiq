import { motion } from 'framer-motion';
import { Users } from 'lucide-react';

export const TravelGroupsPage = () => {
    return (
        <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
            <motion.div
                initial={{ opacity: 0, y: -10 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.4 }}
                className="text-center py-20"
            >
                <div className="w-16 h-16 rounded-2xl bg-emerald-50 dark:bg-emerald-500/10 flex items-center justify-center mx-auto mb-6">
                    <Users size={28} className="text-emerald-600 dark:text-emerald-400" />
                </div>
                <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-3 tracking-tight">
                    Travel Groups
                </h1>
                <p className="text-base text-gray-400 dark:text-gray-500 max-w-md mx-auto">
                    Collaborate with friends and plan group trips together. This feature is coming soon.
                </p>
            </motion.div>
        </main>
    );
};
