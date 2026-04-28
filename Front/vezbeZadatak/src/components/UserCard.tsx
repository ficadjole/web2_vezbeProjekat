import type { UserCardProsp } from "../props/UserCardProps";

export default function UserCard({ name, email, city }: UserCardProsp) {
  return (
    <>
      <table>
        <tr>
          <td>Name</td>
          <td>Email</td>
          <td>City</td>
        </tr>
        <tr>
          <td>{name}</td>
          <td>{email}</td>
          <td>{city}</td>
        </tr>
      </table>
    </>
  );
}
