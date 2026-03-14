import { useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export const AuthCallbackPage = () => {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const { login } = useAuth();

    useEffect(() => {
        const token = searchParams.get('token');
        const userStr = searchParams.get('user');

        if (token && userStr) {
            try {
                const user = JSON.parse(decodeURIComponent(userStr));
                login(token, user);
                navigate('/');
            } catch (err) {
                console.error("Failed to process social login callback:", err);
                navigate('/login?error=callback_failed');
            }
        } else {
            navigate('/login?error=missing_params');
        }
    }, [searchParams, login, navigate]);

    return (
        <div className="w-full h-screen bg-[#020308] flex items-center justify-center">
            <div className="flex flex-col items-center gap-4">
                <div className="w-12 h-12 border-4 border-white/10 border-t-white/80 rounded-full animate-spin" />
                <span className="text-white/50 text-sm font-medium tracking-widest uppercase">Completing Secure Login...</span>
            </div>
        </div>
    );
};
