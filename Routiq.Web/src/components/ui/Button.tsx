import clsx from 'clsx';
import type { ButtonHTMLAttributes } from 'react';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
    variant?: 'primary' | 'secondary' | 'outline';
}

export const Button = ({ children, className, variant = 'primary', ...props }: ButtonProps) => {
    const baseStyles = "px-5 py-2.5 rounded-lg font-medium transition-colors duration-150 text-sm cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed";
    const variants = {
        primary: "bg-teal-600 hover:bg-teal-500 text-white",
        secondary: "bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-200 hover:bg-gray-200 dark:hover:bg-gray-600",
        outline: "bg-transparent border border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-800",
    };

    return (
        <button
            className={clsx(baseStyles, variants[variant], className)}
            {...props}
        >
            {children}
        </button>
    );
};
