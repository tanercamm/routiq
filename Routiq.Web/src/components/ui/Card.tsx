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
            whileHover={hoverEffect ? { scale: 1.02, y: -5 } : {}}
            className={clsx(
                "bg-white/10 backdrop-blur-md border border-white/20 rounded-2xl p-6 shadow-xl",
                "text-white",
                className
            )}
        >
            {children}
        </motion.div>
    );
};
