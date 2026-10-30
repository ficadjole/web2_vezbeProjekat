import type { UserCardProsp } from "../props/UserCardProps";

export default function UserCard({ name, email, city }: UserCardProsp) {
  return (
    <div className="bg-slate-800 text-white p-4 rounded-xl shadow-lg border border-slate-700 max-w-sm">
      <table className="w-full text-left border-collapse">
        <thead>
          <tr className="border-b border-slate-700 text-slate-400 text-sm">
            <th className="pb-2">Ime</th>
            <th className="pb-2">Email</th>
            <th className="pb-2">Grad</th>
          </tr>
        </thead>
        <tbody>
          <tr className="text-sm">
            <td className="pt-2 font-medium">{name}</td>
            <td className="pt-2 text-slate-300">{email}</td>
            <td className="pt-2 text-slate-300">{city}</td>
          </tr>
        </tbody>
      </table>
    </div>
  );
}
