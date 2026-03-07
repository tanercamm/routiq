import { Outlet } from 'react-router-dom';
import { Navbar } from './Navbar';

export const AppLayout = () => {
    return (
        <div className="min-h-screen bg-white dark:bg-gray-950 transition-colors duration-200">
            <Navbar />
            <Outlet />
        </div>
    );
};
