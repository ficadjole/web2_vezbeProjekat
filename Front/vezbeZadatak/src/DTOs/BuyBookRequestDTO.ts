export interface BuyBookRequestDTO {
  id: string;
  title: string;
  author: string;
  price: number;
  quantity: number;
  userId: number;
  email: string;
}
