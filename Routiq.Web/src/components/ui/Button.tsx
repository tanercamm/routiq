import clsx from 'clsx';
import type { ButtonHTMLAttributes } from 'react';
import { motion } from 'framer-motion';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
    variant?: 'primary' | 'secondary' | 'outline';
}

export const Button = ({ children, className, variant = 'primary', ...props }: ButtonProps) => {
    const baseStyles = "px-6 py-3 rounded-lg font-semibold transition-all shadow-lg active:scale-95";
    const variants = {
        primary: "bg-gradient-to-r from-teal-400 to-blue-500 text-white hover:shadow-teal-500/30 border border-transparent",
        secondary: "bg-white/10 text-white hover:bg-white/20 border border-white/20",
        outline: "bg-transparent text-teal-400 border border-teal-500/50 hover:bg-teal-500/10",
    };

    return (
        <motion.button
            whileHover={{ scale: 1.05 }}
            whileTap={{ scale: 0.95 }}
            className={clsx(baseStyles, variants[variant], className)}
            {...props as any}
        >
            {children}
        </motion.button>
    );
};
