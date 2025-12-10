import axiosClient from "./axiosClient";

export interface AmenityDTO {
  amenityId: number;
  name: string;
  description?: string;
  category?: string;
}

export interface AmenityRequestDTO {
  name: string;
  description?: string;
  category?: string;
}

// Helper function to normalize amenity response
const normalizeAmenity = (item: any): AmenityDTO => ({
  amenityId: item.AmenityId || item.amenityId,
  name: item.Name || item.name,
  description: item.Description || item.description,
  category: item.Category || item.category,
});

const amenityAPI = {
  // ========== PUBLIC ENDPOINTS (không cần auth) ==========
  
  // GET /api/amenities - Xem tất cả amenities (public)
  getAllPublic: async (): Promise<AmenityDTO[]> => {
    const response = await axiosClient.get<any>("/amenities");
    const data = response.data;
    
    // Normalize response (handle both array and object with data property)
    let amenities: any[] = [];
    if (Array.isArray(data)) {
      amenities = data;
    } else if (data && Array.isArray(data.data)) {
      amenities = data.data;
    } else if (data && typeof data === 'object') {
      // If response.data is a single object, wrap it in array
      amenities = [data];
    }
    
    if (!Array.isArray(amenities)) {
      console.warn("getAllPublic amenities: response.data is not an array:", data);
      return [];
    }
    
    return amenities.map(normalizeAmenity);
  },

  // GET /api/amenities/{id} - Xem chi tiết amenity (public)
  getByIdPublic: async (id: number): Promise<AmenityDTO> => {
    const response = await axiosClient.get<any>(`/amenities/${id}`);
    return normalizeAmenity(response.data);
  },

  // ========== HOST ENDPOINTS (cần role Host) ==========
  
  // GET /api/host/amenities - Lấy tất cả amenities (host only)
  getAll: async (): Promise<AmenityDTO[]> => {
    const response = await axiosClient.get<any>("/host/amenities");
    const data = response.data;
    
    // Normalize response (handle both array and object with data property)
    let amenities: any[] = [];
    if (Array.isArray(data)) {
      amenities = data;
    } else if (data && Array.isArray(data.data)) {
      amenities = data.data;
    } else if (data && typeof data === 'object') {
      // If response.data is a single object, wrap it in array
      amenities = [data];
    }
    
    if (!Array.isArray(amenities)) {
      console.warn("getAll amenities: response.data is not an array:", data);
      return [];
    }
    
    return amenities.map(normalizeAmenity);
  },

  // GET /api/host/amenities/{id} - Lấy amenity theo ID (host only)
  getById: async (id: number): Promise<AmenityDTO> => {
    const response = await axiosClient.get<any>(`/host/amenities/${id}`);
    return normalizeAmenity(response.data);
  },

  // GET /api/host/amenities/by-category/{category} - Lấy theo category (host only)
  getByCategory: async (category: string): Promise<AmenityDTO[]> => {
    const response = await axiosClient.get<any>(`/host/amenities/by-category/${category}`);
    const data = response.data;
    
    // Normalize response (handle both array and object with data property)
    let amenities: any[] = [];
    if (Array.isArray(data)) {
      amenities = data;
    } else if (data && Array.isArray(data.data)) {
      amenities = data.data;
    } else if (data && typeof data === 'object') {
      // If response.data is a single object, wrap it in array
      amenities = [data];
    }
    
    if (!Array.isArray(amenities)) {
      console.warn("getByCategory amenities: response.data is not an array:", data);
      return [];
    }
    
    return amenities.map(normalizeAmenity);
  },

  // POST /api/host/amenities - Tạo amenity mới (host only)
  // Yêu cầu: Authorize(Roles = "Host")
  create: async (amenity: AmenityRequestDTO): Promise<AmenityDTO> => {
    const response = await axiosClient.post<AmenityDTO>("/host/amenities", amenity);
    return normalizeAmenity(response.data);
  },

  // PUT /api/host/amenities/{id} - Cập nhật amenity (host only)
  // Yêu cầu: Authorize(Roles = "Host")
  update: async (id: number, amenity: AmenityRequestDTO): Promise<AmenityDTO> => {
    const response = await axiosClient.put<AmenityDTO>(`/host/amenities/${id}`, amenity);
    return normalizeAmenity(response.data);
  },

  // DELETE /api/host/amenities/{id} - Xóa amenity (host only)
  // Yêu cầu: Authorize(Roles = "Host")
  // Kiểm tra amenity đang được sử dụng trước khi xóa (từ service)
  delete: async (id: number): Promise<void> => {
    await axiosClient.delete(`/host/amenities/${id}`);
  },
};

export default amenityAPI;



