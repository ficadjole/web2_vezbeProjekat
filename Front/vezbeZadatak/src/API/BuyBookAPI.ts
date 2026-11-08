import axios from "axios";
import type { BuyBookRequestDTO } from "../DTOs/BuyBookRequestDTO";
import type { IBuyBookAPI } from "./IBuyBookAPI";
import axiosRetry from "axios-retry";

export const buyBookAPI: IBuyBookAPI = {
  async buyBook(dto: BuyBookRequestDTO): Promise<boolean> {
    const idempotencyKey = crypto.randomUUID();

    axiosRetry(axios, { retries: 3, retryDelay: axiosRetry.exponentialDelay });

    try {
      const res = await axios.post<boolean>(
        "http://localhost:8453/api/Purchases",
        {
          BookId: dto.id,
          Quantity: dto.quantity,
          UserId: dto.userId,
        },
        {
          headers: {
            "Idempotency-Key": idempotencyKey,
          },
        },
      );

      return res.data;
    } catch (error) {
      if (axios.isAxiosError(error)) {
        alert("Greksa u API: " + error.message);
      }
      return false;
    }
  },
};
