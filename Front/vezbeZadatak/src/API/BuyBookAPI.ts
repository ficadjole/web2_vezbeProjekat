import axios from "axios";
import type { BuyBookRequestDTO } from "../DTOs/BuyBookRequestDTO";
import type { IBuyBookAPI } from "./IBuyBookAPI";

export const buyBookAPI: IBuyBookAPI = {
  async buyBook(dto: BuyBookRequestDTO): Promise<boolean> {
    try {
      const res = await axios.post<boolean>("/api/Values", {
        book: {
          id: dto.id,
          title: dto.title,
          author: dto.author,
          price: dto.price,
          quantity: dto.quantity,
        },
        userId: dto.userId,
      });

      return res.data;
    } catch (error) {
      if (axios.isAxiosError(error)) {
        alert("Greksa u API: " + error.message);
      }
      return false;
    }
  },
};
