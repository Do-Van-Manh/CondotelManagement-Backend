import axiosClient from "./axiosClient";

export interface LocationCreateUpdateDTO {
  name: string;
  description?: string;
  imageUrl?: string;
  ward?: string; // Xã/Phường
  district?: string; // Quận/Huyện
}

export interface LocationDTO {
  locationId: number;
  name: string; // Tên tỉnh/thành phố
  description?: string;
  imageUrl?: string;
  ward?: string; // Xã/Phường
  district?: string; // Quận/Huyện
}

const locationAPI = {
  // ========== PUBLIC ENDPOINTS (không cần auth) ==========
  
  // GET /api/tenant/locations - Lấy tất cả locations (public, không cần đăng nhập)
  getAllPublic: async (): Promise<LocationDTO[]> => {
    try {
      const response = await axiosClient.get<any>("/tenant/locations");
      const data = response.data;
      
      // Normalize response - handle both array and object with data property
      const locations = Array.isArray(data) ? data : (data.data || []);
      
      // Normalize locations - handle both PascalCase and camelCase
      return locations.map((loc: any) => ({
        locationId: loc.LocationId || loc.locationId || loc.Id || loc.id,
        name: loc.Name || loc.name || loc.LocationName || loc.locationName || "",
        description: loc.Description || loc.description || "",
        imageUrl: loc.ImageUrl || loc.imageUrl || loc.ImageURL || loc.imageURL || "",
        ward: loc.Ward || loc.ward || "",
        district: loc.District || loc.district || "",
      }));
    } catch (error: any) {
      console.error("Error loading locations:", error);
      throw error;
    }
  },

  // GET /api/tenant/locations/{id} - Lấy location theo ID (public, không cần đăng nhập)
  getByIdPublic: async (id: number): Promise<LocationDTO> => {
    try {
      const response = await axiosClient.get<any>(`/tenant/locations/${id}`);
      const data = response.data;
      
      // Normalize response - handle both PascalCase and camelCase
      return {
        locationId: data.LocationId || data.locationId || data.Id || data.id,
        name: data.Name || data.name || data.LocationName || data.locationName || "",
        description: data.Description || data.description || "",
        imageUrl: data.ImageUrl || data.imageUrl || data.ImageURL || data.imageURL || "",
        ward: data.Ward || data.ward || "",
        district: data.District || data.district || "",
      };
    } catch (error: any) {
      console.error("Error loading location:", error);
      throw error;
    }
  },

  // GET /api/tenant/locations/search?keyword=abc - Search locations theo keyword (public, không cần đăng nhập)
  searchPublic: async (keyword: string): Promise<LocationDTO[]> => {
    try {
      const response = await axiosClient.get<any>("/tenant/locations/search", {
        params: { keyword },
      });
      const data = response.data;
      
      // Normalize response - handle both array and object with data property
      const locations = Array.isArray(data) ? data : (data.data || []);
      
      // Normalize locations - handle both PascalCase and camelCase
      return locations.map((loc: any) => ({
        locationId: loc.LocationId || loc.locationId || loc.Id || loc.id,
        name: loc.Name || loc.name || loc.LocationName || loc.locationName || "",
        description: loc.Description || loc.description || "",
        imageUrl: loc.ImageUrl || loc.imageUrl || loc.ImageURL || loc.imageURL || "",
        ward: loc.Ward || loc.ward || "",
        district: loc.District || loc.district || "",
      }));
    } catch (error: any) {
      console.error("Error searching locations:", error);
      throw error;
    }
  },

  // GET /api/tenant/locations/{locationId}/wards - Lấy danh sách xã/phường theo locationId (public, không cần đăng nhập)
  getWardsByLocationIdPublic: async (locationId: number): Promise<string[]> => {
    try {
      const response = await axiosClient.get<any>(`/tenant/locations/${locationId}/wards`);
      const data = response.data;
      
      // Backend có thể trả về { success, data, message } hoặc array trực tiếp
      const wards = data.success && data.data ? data.data : (Array.isArray(data) ? data : (data.data || []));
      
      return Array.isArray(wards) ? wards.map((w: any) => w.Ward || w.ward || w.Name || w.name || w) : [];
    } catch (error: any) {
      console.error("Error loading wards:", error);
      // Nếu API không tồn tại, trả về mảng rỗng
      return [];
    }
  },

  // GET /api/tenant/locations/{locationId}/districts - Lấy danh sách quận/huyện theo locationId (public, không cần đăng nhập)
  getDistrictsByLocationIdPublic: async (locationId: number): Promise<string[]> => {
    try {
      const response = await axiosClient.get<any>(`/tenant/locations/${locationId}/districts`);
      const data = response.data;
      
      // Backend có thể trả về { success, data, message } hoặc array trực tiếp
      const districts = data.success && data.data ? data.data : (Array.isArray(data) ? data : (data.data || []));
      
      return Array.isArray(districts) ? districts.map((d: any) => d.District || d.district || d.Name || d.name || d) : [];
    } catch (error: any) {
      console.error("Error loading districts:", error);
      // Nếu API không tồn tại, trả về mảng rỗng
      return [];
    }
  },

  // ========== HOST ENDPOINTS (cần role Host) ==========
  
  // POST /api/host/location - Tạo location mới (host only)
  create: async (dto: LocationCreateUpdateDTO): Promise<LocationDTO> => {
    const res = await axiosClient.post<LocationDTO>("/host/location", dto);
    return res.data;
  },

  // GET /api/host/location - Lấy tất cả locations của host (host only)
  getAll: async (): Promise<LocationDTO[]> => {
    const res = await axiosClient.get<LocationDTO[]>("/host/location");
    return res.data;
  },

  // GET /api/host/location/{id} - Lấy location theo ID (host only)
  getById: async (id: number): Promise<LocationDTO> => {
    const res = await axiosClient.get<LocationDTO>(`/host/location/${id}`);
    return res.data;
  },

  // ========== ADMIN ENDPOINTS (cần role Admin) ==========
  
  // GET /api/admin/location/all - Lấy tất cả locations (admin only)
  getAllAdmin: async (): Promise<LocationDTO[]> => {
    const response = await axiosClient.get<any>("/admin/location/all");
    const data = response.data;
    
    // Backend trả về { success, data, message }
    const locations = data.success && data.data ? data.data : (Array.isArray(data) ? data : (data.data || []));
    
    return locations.map((loc: any) => ({
      locationId: loc.LocationId || loc.locationId || loc.Id || loc.id,
      name: loc.Name || loc.name || loc.LocationName || loc.locationName || "",
      description: loc.Description || loc.description || "",
      imageUrl: loc.ImageUrl || loc.imageUrl || loc.ImageURL || loc.imageURL || "",
      ward: loc.Ward || loc.ward || "",
      district: loc.District || loc.district || "",
    }));
  },

  // GET /api/admin/location/{id} - Lấy location theo ID (admin only)
  getByIdAdmin: async (id: number): Promise<LocationDTO> => {
    const response = await axiosClient.get<any>(`/admin/location/${id}`);
    const data = response.data;
    
    // Backend trả về { success, data, message }
    const location = data.success && data.data ? data.data : data;
    
    return {
      locationId: location.LocationId || location.locationId || location.Id || location.id,
      name: location.Name || location.name || location.LocationName || location.locationName || "",
      description: location.Description || location.description || "",
      imageUrl: location.ImageUrl || location.imageUrl || location.ImageURL || location.imageURL || "",
      ward: location.Ward || location.ward || "",
      district: location.District || location.district || "",
    };
  },

  // POST /api/admin/location - Tạo location mới (admin only)
  createAdmin: async (dto: LocationCreateUpdateDTO): Promise<LocationDTO> => {
    const response = await axiosClient.post<any>("/admin/location", dto);
    const data = response.data;
    
    // Backend trả về { success, data, message }
    const location = data.success && data.data ? data.data : data;
    
    return {
      locationId: location.LocationId || location.locationId,
      name: location.Name || location.name || location.LocationName || location.locationName || "",
      description: location.Description || location.description || "",
      imageUrl: location.ImageUrl || location.imageUrl || location.ImageURL || location.imageURL || "",
      ward: location.Ward || location.ward || "",
      district: location.District || location.district || "",
    };
  },

  // PUT /api/admin/location/{id} - Cập nhật location (admin only)
  updateAdmin: async (id: number, dto: LocationCreateUpdateDTO): Promise<LocationDTO> => {
    const response = await axiosClient.put<any>(`/admin/location/${id}`, dto);
    const data = response.data;
    
    // Backend trả về { success, data, message }
    const location = data.success && data.data ? data.data : data;
    
    return {
      locationId: location.LocationId || location.locationId,
      name: location.Name || location.name || location.LocationName || location.locationName || "",
      description: location.Description || location.description || "",
      imageUrl: location.ImageUrl || location.imageUrl || location.ImageURL || location.imageURL || "",
      ward: location.Ward || location.ward || "",
      district: location.District || location.district || "",
    };
  },

  // DELETE /api/admin/location/{id} - Xóa location (admin only)
  deleteAdmin: async (id: number): Promise<void> => {
    await axiosClient.delete(`/admin/location/${id}`);
  },

  // GET /api/admin/location/{locationId}/wards - Lấy danh sách xã/phường theo locationId (admin only)
  getWardsByLocationId: async (locationId: number): Promise<string[]> => {
    try {
      const response = await axiosClient.get<any>(`/admin/location/${locationId}/wards`);
      const data = response.data;
      
      // Backend có thể trả về { success, data, message } hoặc array trực tiếp
      const wards = data.success && data.data ? data.data : (Array.isArray(data) ? data : (data.data || []));
      
      return Array.isArray(wards) ? wards.map((w: any) => w.Ward || w.ward || w.Name || w.name || w) : [];
    } catch (error: any) {
      console.error("Error loading wards:", error);
      // Nếu API không tồn tại, trả về mảng rỗng
      return [];
    }
  },

  // GET /api/admin/location/{locationId}/districts - Lấy danh sách quận/huyện theo locationId (admin only)
  getDistrictsByLocationId: async (locationId: number): Promise<string[]> => {
    try {
      const response = await axiosClient.get<any>(`/admin/location/${locationId}/districts`);
      const data = response.data;
      
      // Backend có thể trả về { success, data, message } hoặc array trực tiếp
      const districts = data.success && data.data ? data.data : (Array.isArray(data) ? data : (data.data || []));
      
      return Array.isArray(districts) ? districts.map((d: any) => d.District || d.district || d.Name || d.name || d) : [];
    } catch (error: any) {
      console.error("Error loading districts:", error);
      // Nếu API không tồn tại, trả về mảng rỗng
      return [];
    }
  },
};

export default locationAPI;












