import clsx from 'clsx';
import type { InputHTMLAttributes } from 'react';

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
    label: string;
}

export const Input = ({ label, className, ...props }: InputProps) => {
    return (
        <div className="flex flex-col gap-1.5">
            <label className="text-sm font-medium text-gray-700 dark:text-gray-300">{label}</label>
            <input
                className={clsx(
                    "bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg px-4 py-2.5 text-sm",
                    "text-gray-900 dark:text-gray-100 placeholder-gray-400 dark:placeholder-gray-500",
                    "focus:outline-none focus:ring-2 focus:ring-teal-500/40 focus:border-teal-500 transition-colors",
                    className
                )}
                {...props}
            />
        </div>
    );
};
