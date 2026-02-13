import clsx from 'clsx';
import type { InputHTMLAttributes } from 'react';

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
    label: string;
}

export const Input = ({ label, className, ...props }: InputProps) => {
    return (
        <div className="flex flex-col gap-2">
            <label className="text-sm font-medium text-gray-200">{label}</label>
            <input
                className={clsx(
                    "bg-white/5 border border-white/10 rounded-lg px-4 py-2 text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-teal-400 transition-all",
                    className
                )}
                {...props}
            />
        </div>
    );
};
