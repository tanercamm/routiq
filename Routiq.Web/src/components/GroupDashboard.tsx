import { useState } from 'react';
import { ArrowLeft, Trash2, Copy, Check } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { countryNames } from '../utils/countryMapper';

const GroupDashboard = ({ allGroups, selectedGroupId, onBack, deleteGroup }: any) => {
    const [isCalculating, setIsCalculating] = useState(false);
    const [showResults, setShowResults] = useState(false);
    const [copied, setCopied] = useState(false);
    const { user: currentUser } = useAuth();

    const group = allGroups?.find((g: any) => g.id === selectedGroupId);

    // CRASH PROTECTION: If group is missing, render nothing or a safe error
    if (!group) return <div className="p-20 text-white text-center mt-20">Workspace not found (it may have been deleted).</div>;

    // 2. Safety check: Force members to be an array (PREVENTS THE MAP ERROR)
    let safeMembers = Array.isArray(group.members) ? group.members : [];

    // Filter out the ghost admin mock user
    safeMembers = safeMembers.filter((m: any) => m.name !== 'Admin Routiq');

    // 3. Force Hydration: If the group is empty, ensure the creator is placed inside it
    if (safeMembers.length === 0 && currentUser) {
        safeMembers = [
            {
                id: currentUser.id,
                name: currentUser.name || "User",
                avatar: currentUser.avatarUrl,
                origin: currentUser.passports?.[0] || "Not set",
                budget: null
            }
        ];
    }

    // Sort Members: ownerId first
    const sortedMembers = [...safeMembers].sort((a, b) => (a.id === group.ownerId ? -1 : (b.id === group.ownerId ? 1 : 0)));

    const getFlightData = (origin: string, destination: string) => {
        // Fallback or precise mapping
        const o = origin?.toUpperCase();
        if (destination === 'SIN') {
            if (o === 'SYD' || o === 'MEL') return { time: '8h 20m', cost: 700 };
            if (o === 'BER' || o === 'FRA') return { time: '12h 15m', cost: 1100 };
            if (o === 'IST') return { time: '9h 10m', cost: 850 };
            return { time: '10h 45m', cost: 1000 };
        }
        if (destination === 'SJJ') {
            if (o === 'SYD' || o === 'MEL') return { time: '22h 30m', cost: 1800 };
            if (o === 'BER' || o === 'FRA') return { time: '2h 10m', cost: 250 };
            if (o === 'IST') return { time: '2h 00m', cost: 200 };
            return { time: '6h 00m', cost: 500 };
        }
        if (destination === 'CMN') {
            if (o === 'SYD' || o === 'MEL') return { time: '26h 00m', cost: 2100 };
            if (o === 'BER' || o === 'FRA') return { time: '4h 30m', cost: 380 };
            if (o === 'IST') return { time: '5h 15m', cost: 450 };
            return { time: '8h 00m', cost: 600 };
        }
        return { time: '12h', cost: 1000 };
    };

    // 4. "WHAT-IF" DATA PREP
    const membersWithBudget = safeMembers.filter((m: any) => m.budget != null && m.budget > 0);
    const avgBudget = membersWithBudget.length > 0
        ? membersWithBudget.reduce((acc: number, m: any) => acc + m.budget, 0) / membersWithBudget.length
        : 850; // default fallback

    const destinationCost = (destInfo: any[]) => {
        if (!destInfo.length) return 0;
        return destInfo.reduce((acc, curr) => acc + curr.cost, 0) / destInfo.length;
    };

    const sinDetails = sortedMembers.map(m => {
        const data = getFlightData(m.origin, 'SIN');
        return { name: m.name.split(' ')[0], origin: m.origin, time: data.time, cost: data.cost };
    });
    const sjjDetails = sortedMembers.map(m => {
        const data = getFlightData(m.origin, 'SJJ');
        return { name: m.name.split(' ')[0], origin: m.origin, time: data.time, cost: data.cost };
    });
    const cmnDetails = sortedMembers.map(m => {
        const data = getFlightData(m.origin, 'CMN');
        return { name: m.name.split(' ')[0], origin: m.origin, time: data.time, cost: data.cost };
    });

    const comparisonData = {
        groupBudget: avgBudget,
        options: [
            { id: 'SJJ', city: 'Sarajevo', cost: Math.round(destinationCost(sjjDetails)), time: '8h 50m avg', memberDetails: sjjDetails },
            { id: 'CMN', city: 'Casablanca', cost: Math.round(destinationCost(cmnDetails)), time: '11h 50m avg', memberDetails: cmnDetails }
        ]
    };

    return (
        <div className="min-h-screen bg-white dark:bg-[#0F172A] p-8 transition-colors duration-300">
            <div className="max-w-6xl mx-auto">
                {/* HEADER RECONSTRUCTION (ALIGNMENT FIX) */}
                <div className="flex items-center justify-between mb-8 border-b border-gray-200 dark:border-gray-800 pb-6">
                    <div className="flex items-center gap-4">
                        <button
                            onClick={onBack}
                            title="Back to Workspaces"
                            className="p-2 bg-gray-100 hover:bg-gray-200 dark:bg-gray-800 dark:hover:bg-gray-700 text-gray-500 hover:text-blue-500 dark:text-gray-400 dark:hover:text-blue-400 rounded-full transition-colors flex items-center justify-center shrink-0"
                        >
                            <ArrowLeft size={20} className="stroke-[2.5]" />
                        </button>
                        <div className="flex flex-col">
                            <h1 className="text-2xl font-black text-gray-900 dark:text-white uppercase tracking-tight leading-none">
                                {group.name}
                            </h1>
                            <div className="flex items-center gap-2 mt-1.5">
                                <span className="bg-gray-100 dark:bg-gray-800 px-2 py-0.5 rounded text-xs font-mono tracking-widest text-gray-600 dark:text-gray-400">{group.inviteCode || group.code}</span>
                                <button
                                    onClick={() => {
                                        navigator.clipboard.writeText(group.inviteCode || group.code);
                                        setCopied(true);
                                        setTimeout(() => setCopied(false), 2000);
                                    }}
                                    className="p-1 hover:bg-gray-100 dark:hover:bg-gray-800 rounded text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition-colors"
                                    title="Copy Code"
                                >
                                    {copied ? <Check size={14} className="text-emerald-500" /> : <Copy size={14} />}
                                </button>
                            </div>
                        </div>
                    </div>

                    {/* THE MANDATORY DELETE BUTTON */}
                    {group.ownerId === currentUser?.id && (
                        <button
                            onClick={() => { onBack(); setTimeout(() => deleteGroup(group.id), 100); }}
                            className="p-2.5 text-gray-400 hover:text-red-600 hover:bg-red-50 dark:hover:bg-red-500/10 rounded-full transition-all flex items-center justify-center shrink-0"
                            title="Delete Workspace"
                        >
                            <Trash2 size={20} />
                        </button>
                    )}
                </div>

                <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                    {/* LEFT: AUTH-SYNCED MEMBERS */}
                    <div className="bg-gray-50 dark:bg-gray-900/50 p-6 rounded-2xl border border-gray-200 dark:border-gray-800 shadow-sm flex flex-col h-full">
                        <h3 className="font-bold text-gray-900 dark:text-white mb-6 flex items-center justify-between">
                            Group Members <span className="text-xs bg-gray-200 dark:bg-gray-800 text-gray-600 dark:text-gray-400 px-2 py-0.5 rounded-full">{sortedMembers.length}</span>
                        </h3>
                        <div className="space-y-3 flex-1">
                            {sortedMembers.length > 0 ? sortedMembers.map((member: any) => {
                                const isAdmin = member.id === group.ownerId;

                                // Resolve Avatar Link accurately (handle relative urls from backend)
                                const isCurrentUser = member.id === currentUser?.id;
                                const rawAvatarUrl = isCurrentUser ? currentUser?.avatarUrl : (member.avatar || member.avatarUrl);
                                const avatarSrc = rawAvatarUrl
                                    ? (rawAvatarUrl.startsWith('http') ? rawAvatarUrl : `http://localhost:5107${rawAvatarUrl}`)
                                    : 'https://via.placeholder.com/40';

                                return (
                                    <div key={member.id} className="flex items-center gap-3 bg-white dark:bg-gray-800/80 p-3 rounded-xl border border-gray-100 dark:border-gray-700/50 shadow-sm transition-all hover:border-indigo-100 dark:hover:border-indigo-500/30">
                                        <div className="relative">
                                            <img
                                                src={avatarSrc}
                                                className="w-10 h-10 rounded-full border border-gray-200 dark:border-gray-700 object-cover bg-gray-100 dark:bg-gray-800"
                                                alt={member.name}
                                                onError={(e) => { (e.target as HTMLImageElement).src = `https://ui-avatars.com/api/?name=${encodeURIComponent(member.name)}&background=random&color=fff`; }}
                                            />
                                        </div>
                                        <div className="flex flex-col justify-center flex-1">
                                            <div className="flex items-center justify-between">
                                                <span className="font-semibold text-gray-900 dark:text-white text-sm">{member.name}</span>
                                                {isAdmin && (
                                                    <span className="px-1.5 py-0.5 bg-gray-900 dark:bg-gray-100 text-white dark:text-gray-900 text-[8px] font-bold rounded uppercase tracking-wider">
                                                        Admin
                                                    </span>
                                                )}
                                            </div>
                                            <div className="flex items-center gap-2 mt-0.5">
                                                <span className={`text-xs ${member.origin === 'Not set' ? 'text-gray-400 dark:text-gray-500 italic' : 'text-gray-500 dark:text-gray-400'}`}>
                                                    {countryNames[member.origin] || member.origin || 'Not set'}
                                                </span>
                                            </div>
                                        </div>
                                    </div>
                                );
                            }) : (
                                <p className="text-gray-500 text-xs italic text-center">No members found. Try re-adding.</p>
                            )}
                        </div>
                    </div>

                    {/* MIDDLE & RIGHT COLUMN: The Intersection Engine & Saved Routes */}
                    <div className="lg:col-span-2 space-y-5 flex flex-col">

                        {/* Intersection Engine Area */}
                        <div className="bg-gradient-to-br from-indigo-50 to-blue-50 dark:from-indigo-950/20 dark:to-blue-900/10 p-5 rounded-xl border border-indigo-100 dark:border-indigo-500/20 shadow-sm flex-1 flex flex-col justify-center min-h-[200px] relative overflow-hidden transition-all duration-500">

                            {!showResults ? (
                                // Default Engine State
                                <div className="text-center z-10">
                                    <div className="text-indigo-600 dark:text-indigo-400 text-4xl mb-3">⚡</div>
                                    <h3 className="text-lg font-bold text-gray-900 dark:text-white mb-2">Intersection Engine</h3>
                                    <p className="text-sm text-gray-500 dark:text-gray-400 max-w-sm mx-auto mb-2">
                                        Ready to calculate the optimal meeting point based on all members' origins, budgets, and visa constraints.
                                    </p>
                                    <div className="flex flex-wrap items-center justify-center gap-1.5 mt-3 mb-2">
                                        <span className="text-[10px] uppercase font-bold text-emerald-600 dark:text-emerald-400 bg-emerald-50 dark:bg-emerald-900/40 px-2 py-0.5 rounded border border-emerald-200 dark:border-emerald-800/50 flex flex-center gap-1">
                                            <span>🛂</span> Passports Verified: {Array.from(new Set(sortedMembers.map((m: any) => m.origin).filter(o => o && o !== 'Not set'))).join(', ')}
                                        </span>
                                    </div>
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
                                                <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse"></span> Match Found & Visa Cleared
                                            </div>
                                            <h3 className="text-2xl font-black text-gray-900 dark:text-white">Singapore <span className="text-indigo-600 dark:text-indigo-400">(SIN)</span></h3>
                                        </div>
                                        <button onClick={() => setShowResults(false)} className="text-xs text-gray-500 underline hover:text-gray-900 dark:hover:text-white">Recalculate</button>
                                    </div>

                                    <div className="grid grid-cols-2 gap-4 mb-4">
                                        <div className="bg-white/60 dark:bg-slate-900/50 p-3 rounded-lg border border-indigo-100 dark:border-indigo-500/20">
                                            <div className="text-[10px] text-gray-500 uppercase tracking-wide">Avg. Flight Time</div>
                                            <div className="font-bold text-gray-900 dark:text-white">10h 10m</div>
                                        </div>
                                        <div className="bg-white/60 dark:bg-slate-900/50 p-3 rounded-lg border border-indigo-100 dark:border-indigo-500/20">
                                            <div className="text-[10px] text-gray-500 uppercase tracking-wide">Total Est. Cost</div>
                                            <div className="font-bold text-emerald-600 dark:text-emerald-400">Under Budget</div>
                                        </div>
                                    </div>

                                    <div className="space-y-2 mb-6">
                                        {sinDetails.map(m => {
                                            const memberObj = sortedMembers.find((sm: any) => sm.name.split(' ')[0] === m.name);
                                            const isCurrentUser = memberObj?.id === currentUser?.id;
                                            const rawAvatarUrl = isCurrentUser ? currentUser?.avatarUrl : (memberObj?.avatar || memberObj?.avatarUrl);
                                            const avatarSrc = rawAvatarUrl
                                                ? (rawAvatarUrl.startsWith('http') ? rawAvatarUrl : `http://localhost:5107${rawAvatarUrl}`)
                                                : `https://ui-avatars.com/api/?name=${encodeURIComponent(m.name)}`;

                                            return (
                                                <div key={m.name} className="flex justify-between items-center bg-white/40 dark:bg-slate-800/40 p-2.5 rounded-lg border border-indigo-100 dark:border-gray-700 shadow-sm">
                                                    <div className="flex items-center gap-2.5">
                                                        <img src={avatarSrc} className="w-6 h-6 rounded-full border border-gray-200 dark:border-gray-600" />
                                                        <span className="text-sm font-semibold text-gray-800 dark:text-gray-200">{m.name}</span>
                                                        <span className="text-xs text-gray-500 hidden sm:inline-block ml-2">({m.time})</span>
                                                    </div>
                                                    <div className="flex items-center gap-1.5 text-xs font-mono font-bold text-gray-600 dark:text-gray-300 bg-white/60 dark:bg-gray-800 px-2 py-1 rounded">
                                                        <span className="text-gray-900 dark:text-white">{m.origin}</span>
                                                        <span className="text-indigo-400">➔</span>
                                                        <span className="text-gray-900 dark:text-white">SIN</span>
                                                    </div>
                                                </div>
                                            );
                                        })}
                                    </div>

                                    <button className="w-full py-2.5 bg-gray-900 hover:bg-gray-800 dark:bg-white dark:hover:bg-gray-100 dark:text-gray-900 text-white font-bold rounded-lg transition-colors text-sm shadow-md">
                                        View Flight Details for All Members
                                    </button>
                                </div>
                            )}
                        </div>

                        {/* Decision Comparison (What-If) Placeholder */}
                        <div className="bg-white dark:bg-[#1E293B] border border-gray-200 dark:border-gray-800 p-5 rounded-xl shadow-sm mt-5">
                            <h3 className="font-bold text-gray-900 dark:text-white mb-1">Decision Comparison (What-If)</h3>
                            <div className="flex items-center gap-2 mb-4">
                                <span className="text-xs text-gray-500 uppercase tracking-widest font-semibold flex items-center gap-1">
                                    <span className="w-2 h-2 rounded-full bg-blue-500"></span> Group Avg Budget:
                                </span>
                                <strong className="text-gray-900 dark:text-gray-100 font-mono text-sm">${comparisonData.groupBudget.toFixed(0)} /person</strong>
                            </div>

                            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                                {comparisonData.options.map((option, idx) => {
                                    const isViable = option.cost <= comparisonData.groupBudget;
                                    return (
                                        <div key={option.id} className={`relative p-4 rounded-2xl transition-all border flex flex-col ${isViable ? 'border-emerald-200 dark:border-emerald-800/60 bg-gradient-to-br from-emerald-50/50 to-emerald-100/50 dark:from-emerald-950/20 dark:to-emerald-900/10 shadow-sm' : 'border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800/40'}`}>
                                            <div className="flex justify-between items-start mb-2">
                                                <h4 className="font-bold text-gray-900 dark:text-gray-100 text-base">Option {['A', 'B', 'C'][idx]}</h4>
                                                {isViable ? (
                                                    <span className="px-2 py-0.5 text-[10px] font-black uppercase tracking-wider text-emerald-700 dark:text-emerald-300 bg-emerald-100 dark:bg-emerald-900/40 border border-emerald-200 dark:border-emerald-800/50 rounded flex items-center gap-1 shadow-sm shrink-0">
                                                        <svg className="w-3 h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M5 13l4 4L19 7" /></svg>
                                                        Under Budget
                                                    </span>
                                                ) : (
                                                    <span className="px-2 py-0.5 text-[10px] font-black uppercase tracking-wider text-gray-600 dark:text-gray-400 bg-gray-200 dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded flex items-center gap-1 shadow-sm shrink-0">
                                                        Over Budget
                                                    </span>
                                                )}
                                            </div>
                                            <p className="font-medium text-gray-800 dark:text-gray-300 mb-4">{option.city} <span className="text-gray-500 font-normal">({option.id})</span></p>

                                            {/* Details per member */}
                                            <div className="space-y-2 mb-4 flex-1">
                                                {option.memberDetails.map((md, i) => (
                                                    <div key={i} className="flex justify-between items-center bg-white/60 dark:bg-black/20 px-2.5 py-2 rounded-lg border border-white dark:border-gray-700/50">
                                                        <span className="text-gray-900 dark:text-gray-300 text-xs font-semibold flex items-center gap-1">
                                                            {md.name}: <span className="text-gray-500 font-normal">{md.time} ({md.origin})</span>
                                                        </span>
                                                        <span className={`text-xs font-bold font-mono ${md.cost <= comparisonData.groupBudget ? 'text-emerald-600 dark:text-emerald-400' : 'text-rose-500 dark:text-rose-400'}`}>${md.cost}</span>
                                                    </div>
                                                ))}
                                            </div>

                                            <div className="pt-3 border-t border-gray-200/50 dark:border-gray-700/50 flex justify-between items-center mt-auto">
                                                <div className="flex flex-col">
                                                    <span className="text-[10px] text-gray-500 uppercase font-semibold">Avg Cost</span>
                                                    <span className={`text-sm font-bold font-mono ${isViable ? 'text-emerald-600 dark:text-emerald-400' : 'text-gray-700 dark:text-gray-300'}`}>${option.cost}</span>
                                                </div>
                                                <div className="flex flex-col text-right">
                                                    <span className="text-[10px] text-gray-500 uppercase font-semibold">Avg Time</span>
                                                    <span className="text-sm font-medium text-gray-700 dark:text-gray-300">{option.time}</span>
                                                </div>
                                            </div>
                                        </div>
                                    );
                                })}
                            </div>
                        </div>

                    </div>
                </div>
            </div>
        </div>
    );
};

export default GroupDashboard;
