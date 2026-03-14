import { Outlet } from 'react-router-dom';
import { Navbar } from './Navbar';

export const AppLayout = () => {
    return (
        <div className="min-h-screen transition-colors duration-500 light:bg-[#F5F5F7] dark:bg-[#020308]">
            <Navbar />
            <Outlet />
        </div>
    );
};
