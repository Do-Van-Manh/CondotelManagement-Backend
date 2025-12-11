import axiosClient from "./axiosClient";

export interface UtilityDTO {
  utilityId: number;
  name: string;
  description?: string;
  category?: string; // Category: Recreation, Service, etc.
  hostId?: number; // For admin utilities, HostId = 0 means system utility
}

export interface UtilityCreateUpdateDTO {
  name: string;
  description?: string;
  category?: string; // Category: Recreation, Service, etc.
  hostId?: number; // For admin: HostId = 0 for system utilities
}

// Helper function to normalize utility response
const normalizeUtility = (item: any): UtilityDTO => ({
  utilityId: item.UtilityId || item.utilityId,
  name: item.Name || item.name,
  description: item.Description || item.description,
  category: item.Category || item.category,
  hostId: item.HostId || item.hostId,
});

const utilityAPI = {
  // ========== HOST ENDPOINTS ==========
  
  // GET /api/host/utility - Lấy danh sách utilities của host hiện tại
  getAll: async (): Promise<UtilityDTO[]> => {
    const response = await axiosClient.get<any>("/host/utility");
    const data = response.data;
    
    // Normalize response (handle both array and object with data property)
    const utilities = Array.isArray(data) 
      ? data 
      : (data.data || []);
    
    return utilities.map(normalizeUtility);
  },

  // GET /api/host/utility/resort/{resortId} - Lấy danh sách utilities theo resort
  getByResort: async (resortId: number): Promise<UtilityDTO[]> => {
    const response = await axiosClient.get<any>(`/host/utility/resort/${resortId}`);
    const data = response.data;
    
    // Normalize response (handle both array and object with data property)
    const utilities = Array.isArray(data) 
      ? data 
      : (data.data || []);
    
    return utilities.map(normalizeUtility);
  },

  // ========== ADMIN ENDPOINTS ==========
  
  // GET /api/admin/utility/all - Lấy tất cả utilities (admin only)
  getAllAdmin: async (): Promise<UtilityDTO[]> => {
    const response = await axiosClient.get<any>("/admin/utility/all");
    const data = response.data;
    
    // Backend có thể trả về { success, data, message } hoặc array trực tiếp
    const utilities = data.success && data.data 
      ? data.data 
      : (Array.isArray(data) ? data : (data.data || []));
    
    return utilities.map(normalizeUtility);
  },

  // GET /api/admin/utility/{utilityId} - Lấy utility theo ID (admin only)
  getByIdAdmin: async (utilityId: number): Promise<UtilityDTO> => {
    const response = await axiosClient.get<any>(`/admin/utility/${utilityId}`);
    const data = response.data;
    
    // Backend có thể trả về { success, data, message } hoặc object trực tiếp
    const utility = data.success && data.data ? data.data : data;
    return normalizeUtility(utility);
  },

  // POST /api/admin/utility - Tạo utility mới (admin only)
  // Body: UtilityRequestDTO { name, description, category }
  createAdmin: async (utility: UtilityCreateUpdateDTO): Promise<UtilityDTO> => {
    const requestData: any = {
      name: utility.name,
    };
    
    if (utility.description) {
      requestData.description = utility.description;
    }
    
    if (utility.category) {
      requestData.category = utility.category;
    }
    
    const response = await axiosClient.post<any>("/admin/utility", requestData);
    const data = response.data;
    
    // Backend có thể trả về { success, data, message } hoặc object trực tiếp
    const createdUtility = data.success && data.data ? data.data : data;
    return normalizeUtility(createdUtility);
  },

  // PUT /api/admin/utility/{utilityId} - Cập nhật utility (admin only)
  // Body: UtilityRequestDTO { name, description, category }
  updateAdmin: async (utilityId: number, utility: UtilityCreateUpdateDTO): Promise<UtilityDTO> => {
    const requestData: any = {
      name: utility.name,
    };
    
    if (utility.description !== undefined) {
      requestData.description = utility.description;
    }
    
    if (utility.category !== undefined) {
      requestData.category = utility.category;
    }
    
    const response = await axiosClient.put<any>(`/admin/utility/${utilityId}`, requestData);
    const data = response.data;
    
    // Backend có thể trả về { success, data, message } hoặc object trực tiếp
    const updatedUtility = data.success && data.data ? data.data : data;
    return normalizeUtility(updatedUtility);
  },

  // DELETE /api/admin/utility/{utilityId} - Xóa utility (admin only)
  deleteAdmin: async (utilityId: number): Promise<void> => {
    await axiosClient.delete(`/admin/utility/${utilityId}`);
  },
};

export default utilityAPI;







