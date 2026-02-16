import type { ReactNode } from 'react';
import clsx from 'clsx';
import { motion } from 'framer-motion';

interface CardProps {
    children: ReactNode;
    className?: string;
    hoverEffect?: boolean;
}

export const Card = ({ children, className, hoverEffect = false }: CardProps) => {
    return (
        <motion.div
            whileHover={hoverEffect ? { y: -2 } : {}}
            className={clsx(
                "bg-white dark:bg-gray-800 border border-gray-100 dark:border-gray-700/50 rounded-2xl p-6 shadow-[0_1px_3px_rgba(0,0,0,0.04)]",
                "transition-colors duration-200",
                className
            )}
        >
            {children}
        </motion.div>
    );
};
