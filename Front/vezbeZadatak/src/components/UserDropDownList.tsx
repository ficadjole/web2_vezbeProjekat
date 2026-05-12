import { Users } from "../data/usersList";

export default function UserDropDownList({ selectedValue, setSelectedValue }) {
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
