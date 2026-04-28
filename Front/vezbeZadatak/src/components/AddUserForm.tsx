import { useEffect, useState } from "react";

export default function AddUserForm({ Users }) {
  const [formData, setFormData] = useState({
    name: "",
    email: "",
    city: "",
  });
  const handleSumbmit = (event) => {
    event.preventDefault();

    if (formData.name.trim().length < 3) {
      console.error("Nije dovoljno dugacko ime: ", formData.name);
    }

    Users.push({
      name: formData.name,
      email: formData.email,
      city: formData.city,
    });

    console.log("Poslednji user: ", Users[Users.length - 1].name);
  };
  useEffect(() => {
    console.log("Dodali ste novog usera");
  }, [Users]);
  const handleChange = (event) => {
    const { name, value } = event.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  return (
    <>
      <form onSubmit={handleSumbmit}>
        <input name="name" value={formData.name} onChange={handleChange} />
        <input name="city" value={formData.city} onChange={handleChange} />
        <input name="email" value={formData.email} onChange={handleChange} />

        <button type="submit">Posalji</button>
      </form>
    </>
  );
}
