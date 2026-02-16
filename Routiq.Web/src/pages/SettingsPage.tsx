import { motion } from 'framer-motion';
import { Settings } from 'lucide-react';

export const SettingsPage = () => {
    return (
        <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
            <motion.div
                initial={{ opacity: 0, y: -10 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.4 }}
                className="text-center py-20"
            >
                <div className="w-16 h-16 rounded-2xl bg-gray-100 dark:bg-gray-800 flex items-center justify-center mx-auto mb-6">
                    <Settings size={28} className="text-gray-600 dark:text-gray-400" />
                </div>
                <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-3 tracking-tight">
                    Settings
                </h1>
                <p className="text-base text-gray-400 dark:text-gray-500 max-w-md mx-auto">
                    Account preferences, notification controls, and privacy settings are coming soon.
                </p>
            </motion.div>
        </main>
    );
};
