import type { BuyBookRequestDTO } from "../DTOs/BuyBookRequestDTO";

export interface IBuyBookAPI {
  buyBook(dto: BuyBookRequestDTO): Promise<boolean>;
}
