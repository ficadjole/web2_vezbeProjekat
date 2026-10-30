// src/components/BookDropDownList.tsx
import { Books } from "../data/booksList";

interface BookDropDownListProps {
  selectedBookId: string;
  setSelectedBookId: (id: string) => void;
}

export default function BookDropDownList({
  selectedBookId,
  setSelectedBookId,
}: BookDropDownListProps) {
  return (
    <div>
      <label htmlFor="bookSelect" className="block text-sm font-medium mb-1">
        Izaberi knjigu:
      </label>
      <select
        id="bookSelect"
        className="w-full bg-slate-900 border border-slate-600 text-white p-2 rounded-lg focus:outline-none focus:border-blue-500"
        value={selectedBookId}
        onChange={(e) => setSelectedBookId(e.target.value)}
      >
        <option value={0}>-- Izaberite knjigu --</option>
        {Books.map((book) => (
          <option key={book.id} value={book.id}>
            {book.title} - {book.author} ({book.price} RSD)
          </option>
        ))}
      </select>
    </div>
  );
}
