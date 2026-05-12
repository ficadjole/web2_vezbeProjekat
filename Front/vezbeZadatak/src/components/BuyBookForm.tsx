import { useState } from "react";
import Counter from "./Counter";
import { buyBookAPI } from "../API/BuyBookAPI";
import type { BuyBookRequestDTO } from "../DTOs/BuyBookRequestDTO";
import UserDropDownList from "./UserDropDownList";
import { Users } from "../data/usersList";

export default function BuyBookForm() {
  const [buyBookFormData, setBuyBookFormData] = useState({
    title: "",
    author: "",
    price: 0,
    quantity: 0,
    userId: 0,
  });

  const handleSubmit = async (event) => {
    event.preventDefault();

    if (buyBookFormData.quantity <= 0) {
      alert("Nije moguce kupiti 0 ili manje knjiga");
      return;
    }

    var user = Users.find((u) => u.userId === Number(buyBookFormData.userId));

    console.log("Podaci za slanje:", buyBookFormData);
    try {
      const dto: BuyBookRequestDTO = {
        id: 2,
        title: buyBookFormData.title,
        author: buyBookFormData.author,
        price: buyBookFormData.price,
        quantity: buyBookFormData.quantity,
        userId: buyBookFormData.userId,
        email: user!.email,
      };

      const response = await buyBookAPI.buyBook(dto);

      if (response) {
        alert(
          `Kupljeno: ${buyBookFormData.quantity}x ${buyBookFormData.title}`,
        );
      } else {
        alert("Doslo je do greske");
      }
    } catch {}
  };

  const handleChange = (event) => {
    const { name, value } = event.target;

    setBuyBookFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleQuantityChange = (newCount: number) => {
    setBuyBookFormData((prev) => ({
      ...prev,
      quantity: newCount,
    }));
  };

  const handleUserChange = (newUserId: number) => {
    setBuyBookFormData((prev) => ({
      ...prev,
      userId: newUserId,
    }));
  };

  return (
    <>
      <form onSubmit={handleSubmit} className="bg-slate-700 w-96 p-5">
        <div className="grid grid-cols-3 gap-4">
          <label className="self-center">Title</label>
          <input
            className="col-span-2 text-black p-1 rounded"
            name="title"
            value={buyBookFormData.title}
            onChange={handleChange}
            required
          />

          <label className="self-center">Author</label>
          <input
            className="col-span-2 text-black p-1 rounded"
            name="author"
            value={buyBookFormData.author}
            onChange={handleChange}
            required
          />

          <label className="self-center" typeof="number">
            Price
          </label>
          <input
            className="col-span-2 text-black p-1 rounded"
            name="price"
            value={buyBookFormData.price}
            onChange={handleChange}
            required
          />

          <label className="self-center">Quantity</label>
          <div className="col-span-2">
            <Counter
              count={buyBookFormData.quantity}
              onCountChange={handleQuantityChange}
            />
          </div>
        </div>
        <div>
          <UserDropDownList
            selectedValue={buyBookFormData.userId}
            setSelectedValue={handleUserChange}
          />
        </div>
        <button
          type="submit"
          className="w-full mt-6 bg-blue-500 hover:bg-blue-600 p-2 rounded font-bold"
        >
          ORDER BOOK
        </button>
      </form>
    </>
  );
}
