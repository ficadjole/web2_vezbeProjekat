import { useState } from "react";
import Counter from "./Counter";
import { buyBookAPI } from "../API/BuyBookAPI";
import type { BuyBookRequestDTO } from "../DTOs/BuyBookRequestDTO";
import UserDropDownList from "./UserDropDownList";
import BookDropDownList from "./BookDropDownList";
import { Users } from "../data/usersList";
import { Books } from "../data/booksList";

export default function BuyBookForm() {
  const [buyBookFormData, setBuyBookFormData] = useState({
    bookId: "",
    quantity: 1,
    userId: 0,
  });

  const [message, setMessage] = useState<{
    text: string;
    type: "success" | "error";
  } | null>(null);

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setMessage(null); // Resetujemo prethodnu poruku

    if (buyBookFormData.bookId === "") {
      setMessage({ text: "Molimo izaberite knjigu iz liste.", type: "error" });
      return;
    }

    if (buyBookFormData.userId === 0) {
      setMessage({ text: "Molimo izaberite korisnika.", type: "error" });
      return;
    }

    if (buyBookFormData.quantity <= 0) {
      setMessage({
        text: "Nije moguće kupiti 0 ili manje knjiga.",
        type: "error",
      });
      return;
    }

    const selectedBook = Books.find((b) => b.id === buyBookFormData.bookId);
    const selectedUser = Users.find(
      (u) => u.userId === Number(buyBookFormData.userId),
    );

    if (!selectedBook || !selectedUser) {
      setMessage({
        text: "Greška: Izabrana knjiga ili korisnik ne postoje.",
        type: "error",
      });
      return;
    }

    try {
      const dto: BuyBookRequestDTO = {
        id: selectedBook.id,
        title: selectedBook.title,
        author: selectedBook.author,
        price: selectedBook.price,
        quantity: buyBookFormData.quantity,
        userId: selectedUser.userId,
        email: selectedUser.email,
      };

      console.log("Slanje DTO objekta na bekhend:", dto);
      const response = await buyBookAPI.buyBook(dto);

      if (response) {
        setMessage({
          text: `Uspešno naručeno! Kupljeno je ${buyBookFormData.quantity}x "${selectedBook.title}" za korisnika ${selectedUser.name}.`,
          type: "success",
        });

        setBuyBookFormData({ bookId: "", quantity: 1, userId: 0 });
      } else {
        setMessage({
          text: "Došlo je do greške na serveru prilikom obrade porudžbine.",
          type: "error",
        });
      }
    } catch (error) {
      console.error("Greška pri kupovini:", error);
      setMessage({
        text: "Došlo je do greške prilikom komunikacije sa serverom.",
        type: "error",
      });
    }
  };

  const handleQuantityChange = (newCount: number) => {
    setBuyBookFormData((prev) => ({ ...prev, quantity: newCount }));
  };

  const handleUserChange = (newUserId: number) => {
    setBuyBookFormData((prev) => ({ ...prev, userId: newUserId }));
  };

  const handleBookChange = (newBookId: string) => {
    setBuyBookFormData((prev) => ({ ...prev, bookId: newBookId }));
  };

  return (
    <form
      onSubmit={handleSubmit}
      className="bg-slate-800 text-white w-full max-w-md p-6 rounded-xl shadow-lg space-y-4 mx-auto"
    >
      <h2 className="text-xl font-bold text-center mb-2 text-blue-400">
        Naručivanje knjige
      </h2>

      {message && (
        <div
          className={`p-3 rounded-lg text-sm font-medium text-center transition-all ${
            message.type === "success"
              ? "bg-emerald-900/80 border border-emerald-500 text-emerald-200"
              : "bg-rose-900/80 border border-rose-500 text-rose-200"
          }`}
        >
          {message.text}
        </div>
      )}

      <div className="space-y-4">
        <BookDropDownList
          selectedBookId={buyBookFormData.bookId}
          setSelectedBookId={handleBookChange}
        />

        <UserDropDownList
          selectedValue={buyBookFormData.userId}
          setSelectedValue={handleUserChange}
        />

        <div>
          <label className="block text-sm font-medium mb-1">Količina</label>
          <Counter
            count={buyBookFormData.quantity}
            onCountChange={handleQuantityChange}
          />
        </div>
      </div>

      <button
        type="submit"
        className="w-full mt-6 bg-blue-600 hover:bg-blue-500 transition-colors p-2.5 rounded-lg font-bold shadow-md cursor-pointer"
      >
        POTVRDI KUPOVINU
      </button>
    </form>
  );
}
