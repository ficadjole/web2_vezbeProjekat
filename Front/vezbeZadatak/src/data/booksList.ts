export interface BookItem {
  id: number;
  title: string;
  author: string;
  price: number;
}

export const Books: BookItem[] = [
  {
    id: 1,
    title: "Dekaton: Pocetak rata",
    author: "Vladimir Martic",
    price: 2000,
  },
  { id: 2, title: "Crni obelisk", author: "C.M. Remark", price: 1899 },
  { id: 3, title: "20000 milja pod morem", author: "Zil Vern", price: 1000 },
];
