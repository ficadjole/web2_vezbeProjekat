import { Users } from "../data/usersList";

interface UserDropDownListProps {
  selectedValue: string;
  setSelectedValue: (id: string) => void;
}

export default function UserDropDownList({
  selectedValue,
  setSelectedValue,
}: UserDropDownListProps) {
  return (
    <div>
      <label htmlFor="select" className="block text-sm font-medium mb-1">
        Izaberi korisnika:
      </label>
      <select
        id="select"
        className="w-full bg-slate-900 border border-slate-600 text-white p-2 rounded-lg focus:outline-none focus:border-blue-500"
        value={selectedValue}
        onChange={(e) => setSelectedValue(e.target.value)}
      >
        <option value={0}>-- Izaberite korisnika --</option>
        {Users.map((user) => (
          <option key={user.userId} value={user.userId}>
            {user.name} ({user.email})
          </option>
        ))}
      </select>
    </div>
  );
}
