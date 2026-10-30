import { useState } from "react";

export default function AddUserForm({ Users }: { Users: any[] }) {
  const [formData, setFormData] = useState({
    name: "",
    email: "",
    city: "",
  });

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();

    if (formData.name.trim().length < 3) {
      alert("Ime mora imati najmanje 3 karaktera.");
      return;
    }

    Users.push({
      name: formData.name,
      email: formData.email,
      city: formData.city,
    });

    alert(`Uspješno dodat korisnik: ${formData.name}`);
    setFormData({ name: "", email: "", city: "" });
  };

  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = event.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  return (
    <form
      onSubmit={handleSubmit}
      className="bg-slate-800 text-white w-full max-w-md p-6 rounded-xl shadow-lg space-y-4 mx-auto"
    >
      <h2 className="text-xl font-bold text-center mb-4 text-emerald-400">
        Dodaj novog korisnika
      </h2>

      <div>
        <label className="block text-sm font-medium mb-1">Ime i prezime</label>
        <input
          className="w-full bg-slate-900 border border-slate-600 text-white p-2 rounded-lg focus:outline-none focus:border-emerald-500"
          name="name"
          placeholder="Unesite ime..."
          value={formData.name}
          onChange={handleChange}
          required
        />
      </div>

      <div>
        <label className="block text-sm font-medium mb-1">Grad</label>
        <input
          className="w-full bg-slate-900 border border-slate-600 text-white p-2 rounded-lg focus:outline-none focus:border-emerald-500"
          name="city"
          placeholder="Unesite grad..."
          value={formData.city}
          onChange={handleChange}
          required
        />
      </div>

      <div>
        <label className="block text-sm font-medium mb-1">Email adresa</label>
        <input
          type="email"
          className="w-full bg-slate-900 border border-slate-600 text-white p-2 rounded-lg focus:outline-none focus:border-emerald-500"
          name="email"
          placeholder="Unesite email..."
          value={formData.email}
          onChange={handleChange}
          required
        />
      </div>

      <button
        type="submit"
        className="w-full mt-4 bg-emerald-600 hover:bg-emerald-500 transition-colors p-2.5 rounded-lg font-bold shadow-md cursor-pointer"
      >
        SAČUVAJ KORISNIKA
      </button>
    </form>
  );
}