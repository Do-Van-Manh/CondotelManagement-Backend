import axiosClient from "./axiosClient";

// DTOs từ backend Promotion
export interface PromotionDTO {
  promotionId: number;
  condotelId: number;
  condotelName?: string;
  title: string;
  description?: string;
  discountPercentage?: number;
  discountAmount?: number;
  startDate: string;
  endDate: string;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreatePromotionDTO {
  condotelId: number;
  title: string;
  description?: string;
  discountPercentage?: number;
  discountAmount?: number;
  startDate: string;
  endDate: string;
  isActive?: boolean;
}

export interface UpdatePromotionDTO {
  title?: string;
  description?: string;
  discountPercentage?: number;
  discountAmount?: number;
  startDate?: string;
  endDate?: string;
  isActive?: boolean;
}

// API Calls
export const promotionAPI = {
  // GET /api/host/promotion - Lấy tất cả promotions của host
  getAll: async (): Promise<PromotionDTO[]> => {
    const response = await axiosClient.get<PromotionDTO[]>("/host/promotion");
    return response.data;
  },

  // GET /api/host/promotion/{id} - Lấy promotion theo ID
  getById: async (id: number): Promise<PromotionDTO> => {
    const response = await axiosClient.get<PromotionDTO>(`/host/promotion/${id}`);
    return response.data;
  },

  // POST /api/host/promotion - Tạo promotion mới
  create: async (promotion: CreatePromotionDTO): Promise<PromotionDTO> => {
    const response = await axiosClient.post<PromotionDTO>("/host/promotion", promotion);
    return response.data;
  },

  // PUT /api/host/promotion/{id} - Cập nhật promotion
  update: async (
    id: number,
    promotion: UpdatePromotionDTO
  ): Promise<PromotionDTO> => {
    const response = await axiosClient.put<PromotionDTO>(`/host/promotion/${id}`, promotion);
    return response.data;
  },

  // DELETE /api/host/promotion/{id} - Xóa promotion
  delete: async (id: number): Promise<void> => {
    await axiosClient.delete(`/host/promotion/${id}`);
  },
};

export default promotionAPI;







