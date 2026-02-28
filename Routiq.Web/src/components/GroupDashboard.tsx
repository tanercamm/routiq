import { useState } from 'react';

const GroupDashboard = ({ group, onBack, deleteGroup, currentUser }: any) => {
    const [isCalculating, setIsCalculating] = useState(false);
    const [showResults, setShowResults] = useState(false);

    // CRASH PROTECTION: If group is missing, render nothing or a safe error
    if (!group) return <div className="p-20 text-white">Loading workspace...</div>;

    // 2. Safety check: Force members to be an array (PREVENTS THE MAP ERROR)
    let safeMembers = Array.isArray(group.members) ? group.members : [];

    // 3. Force Hydration: If the group is empty, ensure the creator is placed inside it
    if (safeMembers.length === 0 && currentUser) {
        safeMembers = [
            {
                id: currentUser.id,
                name: currentUser.name || "Group Creator",
                avatar: currentUser.avatarUrl,
                origin: currentUser.country || "Not set",
                budget: null
            }
        ];
    }

    return (
        <div className="min-h-screen bg-white dark:bg-[#0F172A] p-8 transition-colors duration-300">
            <div className="max-w-6xl mx-auto">
                {/* TOP BAR */}
                <div className="flex justify-between items-center mb-10 border-b border-gray-200 dark:border-gray-800 pb-6">
                    <button onClick={onBack} className="text-gray-500 hover:text-blue-500 transition-colors">
                        &larr; Back to Workspaces
                    </button>

                    {/* THE MANDATORY DELETE BUTTON */}
                    {group.ownerId === currentUser?.id && (
                        <button
                            onClick={() => { onBack(); setTimeout(() => deleteGroup(group.id), 100); }}
                            className="px-4 py-2 bg-red-600 hover:bg-red-700 text-white text-xs font-bold rounded shadow-lg transition-all"
                        >
                            DELETE WORKSPACE
                        </button>
                    )}
                </div>

                {/* HEADER */}
                <div className="mb-12">
                    <h1 className="text-4xl font-black text-gray-900 dark:text-white uppercase tracking-tight">
                        {group.name}
                    </h1>
                    <p className="text-gray-500 font-mono mt-2">{group.inviteCode || group.code}</p>
                </div>

                <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
                    {/* LEFT: AUTH-SYNCED MEMBERS */}
                    <div className="bg-gray-50 dark:bg-gray-900/50 p-6 rounded-2xl border border-gray-200 dark:border-gray-800">
                        <h3 className="font-bold text-gray-900 dark:text-white mb-6">Group Members</h3>
                        <div className="space-y-4">
                            {safeMembers.length > 0 ? safeMembers.map((member: any) => (
                                <div key={member.id} className="flex items-center gap-3">
                                    <img
                                        src={member.id === currentUser?.id ? currentUser.avatarUrl : (member.avatar || 'https://via.placeholder.com/40')}
                                        className="w-10 h-10 rounded-full border-2 border-white dark:border-gray-700 shadow-sm object-cover"
                                        alt={member.name}
                                        onError={(e) => { (e.target as HTMLImageElement).src = `https://ui-avatars.com/api/?name=${encodeURIComponent(member.name)}&background=random&color=fff`; }}
                                    />
                                    <div className="flex flex-col">
                                        <span className="font-bold text-gray-900 dark:text-white text-sm">{member.name}</span>
                                        {member.id === group.ownerId && <span className="text-[9px] text-orange-500 font-black uppercase">Admin</span>}
                                    </div>
                                </div>
                            )) : (
                                <p className="text-gray-500 text-xs italic text-center">No members found. Try re-adding.</p>
                            )}
                        </div>
                    </div>

                    {/* MIDDLE & RIGHT COLUMN: The Intersection Engine & Saved Routes */}
                    <div className="lg:col-span-2 space-y-6 flex flex-col">

                        {/* Intersection Engine Area */}
                        <div className="bg-gradient-to-br from-indigo-50 to-blue-50 dark:from-indigo-950/20 dark:to-blue-900/10 p-6 rounded-xl border border-indigo-100 dark:border-indigo-500/20 shadow-sm flex-1 flex flex-col justify-center min-h-[250px] relative overflow-hidden transition-all duration-500">

                            {!showResults ? (
                                // Default Engine State
                                <div className="text-center z-10">
                                    <div className="text-indigo-600 dark:text-indigo-400 text-4xl mb-3">⚡</div>
                                    <h3 className="text-lg font-bold text-gray-900 dark:text-white mb-2">Intersection Engine</h3>
                                    <p className="text-sm text-gray-500 dark:text-gray-400 max-w-sm mx-auto">
                                        Ready to calculate the optimal meeting point based on all members' origins, budgets, and visa constraints.
                                    </p>
                                    <button
                                        onClick={() => {
                                            setIsCalculating(true);
                                            setTimeout(() => {
                                                setIsCalculating(false);
                                                setShowResults(true); // TRIGGER RESULTS UI
                                            }, 2500);
                                        }}
                                        disabled={isCalculating}
                                        className={`mt-6 px-8 py-3 font-bold rounded-lg transition-all shadow-md text-sm flex items-center justify-center gap-2 mx-auto ${isCalculating ? 'bg-indigo-800 text-white cursor-wait opacity-90' : 'bg-indigo-600 hover:bg-indigo-700 text-white'
                                            }`}
                                    >
                                        {isCalculating ? (
                                            <>
                                                <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                                                </svg>
                                                Scanning 14,000 combinations...
                                            </>
                                        ) : (
                                            '⚡ Run Intersection Engine'
                                        )}
                                    </button>
                                </div>
                            ) : (
                                // The Results UI
                                <div className="text-left z-10 animate-in fade-in slide-in-from-bottom-4 duration-700">
                                    <div className="flex justify-between items-start mb-4">
                                        <div>
                                            <div className="text-[10px] font-bold text-emerald-600 dark:text-emerald-400 tracking-widest uppercase mb-1 flex items-center gap-1">
                                                <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse"></span> Match Found
                                            </div>
                                            <h3 className="text-2xl font-black text-gray-900 dark:text-white">Sarajevo <span className="text-indigo-600 dark:text-indigo-400">(SJJ)</span></h3>
                                        </div>
                                        <button onClick={() => setShowResults(false)} className="text-xs text-gray-500 underline hover:text-gray-900 dark:hover:text-white">Recalculate</button>
                                    </div>

                                    <div className="grid grid-cols-2 gap-4 mb-6">
                                        <div className="bg-white/60 dark:bg-slate-900/50 p-3 rounded-lg border border-indigo-100 dark:border-indigo-500/20">
                                            <div className="text-[10px] text-gray-500 uppercase tracking-wide">Avg. Flight Time</div>
                                            <div className="font-bold text-gray-900 dark:text-white">2h 45m</div>
                                        </div>
                                        <div className="bg-white/60 dark:bg-slate-900/50 p-3 rounded-lg border border-indigo-100 dark:border-indigo-500/20">
                                            <div className="text-[10px] text-gray-500 uppercase tracking-wide">Total Est. Cost</div>
                                            <div className="font-bold text-emerald-600 dark:text-emerald-400">Under Budget</div>
                                        </div>
                                    </div>

                                    <button className="w-full py-2.5 bg-gray-900 hover:bg-gray-800 dark:bg-white dark:hover:bg-gray-100 dark:text-gray-900 text-white font-bold rounded-lg transition-colors text-sm shadow-md">
                                        View Flight Details for All Members
                                    </button>
                                </div>
                            )}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default GroupDashboard;
