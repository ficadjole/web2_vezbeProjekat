interface CounterProps {
  count: number;
  onCountChange: (newCount: number) => void;
}

export default function Counter({ count, onCountChange }: CounterProps) {
  return (
    <div className="flex items-center space-x-3 bg-slate-900 p-2 rounded-lg border border-slate-600 w-full">
      <div className="bg-slate-800 text-white text-center font-bold px-4 py-1.5 rounded-md min-w-[50px]">
        {count}
      </div>
      <div className="flex space-x-2 flex-1 justify-end">
        <button
          type="button"
          className="bg-emerald-600 hover:bg-emerald-500 text-white px-3 py-1.5 rounded-md font-bold transition-colors cursor-pointer"
          onClick={() => onCountChange(count + 1)}
        >
          +
        </button>
        <button
          type="button"
          className="bg-rose-600 hover:bg-rose-500 text-white px-3 py-1.5 rounded-md font-bold transition-colors cursor-pointer"
          onClick={() => onCountChange(Math.max(0, count - 1))}
        >
          -
        </button>
        <button
          type="button"
          className="bg-slate-700 hover:bg-slate-600 text-slate-200 px-3 py-1.5 rounded-md text-xs font-bold transition-colors cursor-pointer"
          onClick={() => onCountChange(1)}
        >
          Reset
        </button>
      </div>
    </div>
  );
}