export default function UserDropDownList({ selectedValue, setSelectedValue }) {
  const Users = [
    {
      userId: 1,
      name: "Filip",
      email: "velemirfilip@gmail.com",
      city: "Novi Sad",
    },
    {
      userId: 2,
      name: "Katarina",
      email: "kalauzkatarina39@gmail.com",
      city: "Kishegyes",
    },
    {
      userId: 3,
      name: "Tamara",
      email: "etamara71@gmail.com",
      city: "Bijeljina",
    },
  ];

  return (
    <>
      <label htmlFor="select">Choose an user: </label>
      <select
        id="select"
        value={selectedValue}
        onChange={(e) => setSelectedValue(e.target.value)}
      >
        <option value="">-- Select --</option>
        {Users.map((user) => (
          <option key={user.userId} value={user.userId}>
            {user.name + " " + user.email}
          </option>
        ))}
      </select>
    </>
  );
}
