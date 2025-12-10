import axiosClient from "./axiosClient";

// Sub DTOs for CondotelDetailDTO
export interface ImageDTO {
  imageId?: number; // Optional - không cần khi create
  imageUrl: string;
  caption?: string;
}

export interface PriceDTO {
  priceId: number;
  startDate: string; // DateOnly in C# = string in TypeScript
  endDate: string;
  basePrice: number;
  priceType: string;
  description: string;
}

export interface DetailDTO {
  buildingName?: string;
  roomNumber?: string;
  beds?: number; // Optional - có thể lấy từ condotel level
  bathrooms?: number; // Optional - có thể lấy từ condotel level
  safetyFeatures?: string;
  hygieneStandards?: string;
}

export interface AmenityDTO {
  amenityId: number;
  name: string;
}

export interface UtilityDTO {
  utilityId: number;
  name: string;
}

// Promotion DTOs - Promotion là một phần của Condotel
export interface PromotionDTO {
  promotionId: number;
  condotelId: number;
  condotelName?: string;
  name: string;
  description?: string;
  discountPercentage?: number;
  discountAmount?: number;
  startDate: string;
  endDate: string;
  isActive: boolean;
  status?: string; // Optional compatibility with backends using string status
  createdAt?: string;
  updatedAt?: string;
}

export interface CreatePromotionDTO {
  condotelId: number;
  name: string;
  description?: string;
  discountPercentage?: number;
  discountAmount?: number;
  startDate: string;
  endDate: string;
  isActive?: boolean;
  status?: string; // Optional
}

export interface UpdatePromotionDTO {
  condotelId?: number;
  name?: string;
  description?: string;
  discountPercentage?: number;
  discountAmount?: number;
  startDate?: string;
  endDate?: string;
  isActive?: boolean;
  status?: string; // Optional
}

// CondotelDTO - Simplified version for list view
export interface CondotelDTO {
  condotelId: number;
  name: string;
  pricePerNight: number;
  beds: number;
  bathrooms: number;
  status: string;
  thumbnailUrl?: string;
  resortName?: string;
  hostName?: string;
  activePromotion?: PromotionDTO | null; // Promotion đang active (nếu có)
  activePrice?: PriceDTO | null; // Price đang active (nếu có)
}

// CondotelDetailDTO - Full details for detail/update
export interface CondotelDetailDTO {
  condotelId: number;
  hostId: number;
  resortId?: number;
  name: string;
  description?: string;
  pricePerNight: number;
  beds: number;
  bathrooms: number;
  status: string;

  // Host info (nếu backend trả về)
  hostName?: string;
  hostImageUrl?: string;

  // Resort info (nếu backend trả về)
  resortName?: string;
  resortAddress?: string;

  // Liên kết 1-n
  images?: ImageDTO[];
  prices?: PriceDTO[];
  details?: DetailDTO[];

  // Liên kết n-n (backend trả về object lists, không có IDs)
  amenities?: AmenityDTO[];
  utilities?: UtilityDTO[];
  promotions?: PromotionDTO[]; // Danh sách tất cả promotions (không chỉ active)
  activePromotion?: PromotionDTO | null; // Promotion đang active (nếu có)
  activePrice?: PriceDTO | null; // Price đang active (nếu có)
}

// CreateCondotelDTO - For creating new condotel (matches CondotelCreateDTO from backend)
// Lưu ý: HostId sẽ được backend tự động lấy từ JWT token, không cần gửi từ frontend
export interface CreateCondotelDTO {
  resortId?: number; // Optional
  name: string;
  description?: string;
  pricePerNight: number;
  beds: number;
  bathrooms: number;
  status: string; // "Active" hoặc "Inactive"

  // Liên kết 1-n
  images?: Array<{
    imageUrl: string;
    caption?: string;
    // ImageId không cần khi create (sẽ được backend tự tạo)
  }>;

  prices?: Array<{
    startDate: string; // DateOnly format: YYYY-MM-DD
    endDate: string; // DateOnly format: YYYY-MM-DD
    basePrice: number;
    priceType: string;
    description: string; // Required trong backend PriceDTO
    // PriceId không cần khi create (sẽ được backend tự tạo)
  }>;

  details?: Array<{
    buildingName?: string;
    roomNumber?: string;
    beds?: number; // byte in C# - optional
    bathrooms?: number; // byte in C# - optional
    safetyFeatures?: string;
    hygieneStandards?: string;
  }>;

  // Liên kết n-n - chỉ cần IDs
  amenityIds?: number[];
  utilityIds?: number[];
}

// Search query interface for condotel search
export interface CondotelSearchQuery {
  name?: string;
  location?: string; // Location name
  locationId?: number; // Location ID
  fromDate?: string; // DateOnly format: YYYY-MM-DD
  toDate?: string; // DateOnly format: YYYY-MM-DD
  minPrice?: number; // Minimum price per night
  maxPrice?: number; // Maximum price per night
  beds?: number; // Minimum number of beds (>=)
  bathrooms?: number; // Minimum number of bathrooms (>=)
}

// Helper functions to normalize data (shared across API calls)
const normalizeAmenities = (amenities: any[]): AmenityDTO[] => {
  if (!amenities || !Array.isArray(amenities)) return [];
  return amenities.map((a: any) => ({
    amenityId: a.AmenityId || a.amenityId || a.Id || a.id,
    name: a.Name || a.name,
  }));
};

const normalizeUtilities = (utilities: any[]): UtilityDTO[] => {
  if (!utilities || !Array.isArray(utilities)) return [];
  return utilities.map((u: any) => ({
    utilityId: u.UtilityId || u.utilityId || u.Id || u.id,
    name: u.Name || u.name,
  }));
};

const normalizePromotions = (promotions: any[]): PromotionDTO[] => {
  if (!promotions || !Array.isArray(promotions)) return [];
  return promotions.map((p: any) => ({
    promotionId: p.PromotionId || p.promotionId || 0,
    condotelId: p.CondotelId || p.condotelId || 0,
    condotelName: p.CondotelName || p.condotelName,
    name: p.Name || p.name || p.Title || p.title || "",
    description: p.Description || p.description,
    discountPercentage: p.DiscountPercentage !== undefined ? p.DiscountPercentage : p.discountPercentage,
    discountAmount: p.DiscountAmount !== undefined ? p.DiscountAmount : p.discountAmount,
    startDate: p.StartDate || p.startDate || "",
    endDate: p.EndDate || p.endDate || "",
    isActive: p.IsActive !== undefined ? p.IsActive : (p.isActive !== undefined ? p.isActive : false),
    status: p.Status || p.status,
    createdAt: p.CreatedAt || p.createdAt,
    updatedAt: p.UpdatedAt || p.updatedAt,
  }));
};

const normalizePromotion = (promo: any): PromotionDTO | null => {
  if (!promo) return null;
  return {
    promotionId: promo.PromotionId || promo.promotionId || 0,
    condotelId: promo.CondotelId || promo.condotelId || 0,
    condotelName: promo.CondotelName || promo.condotelName,
    name: promo.Name || promo.name || promo.Title || promo.title || "",
    description: promo.Description || promo.description,
    discountPercentage: promo.DiscountPercentage !== undefined ? promo.DiscountPercentage : promo.discountPercentage,
    discountAmount: promo.DiscountAmount !== undefined ? promo.DiscountAmount : promo.discountAmount,
    startDate: promo.StartDate || promo.startDate || "",
    endDate: promo.EndDate || promo.endDate || "",
    isActive: promo.IsActive !== undefined ? promo.IsActive : (promo.isActive !== undefined ? promo.isActive : false),
    status: promo.Status || promo.status,
    createdAt: promo.CreatedAt || promo.createdAt,
    updatedAt: promo.UpdatedAt || promo.updatedAt,
  };
};

// API Calls
export const condotelAPI = {
  // GET /api/tenant/condotels?name=abc&location=abc&locationId=123&fromDate=...&toDate=...&minPrice=...&maxPrice=...&beds=...&bathrooms=... - Tìm kiếm condotel (public, không cần đăng nhập)
  search: async (query?: CondotelSearchQuery): Promise<CondotelDTO[]> => {
    const params: any = {};
    if (query?.name) {
      params.name = query.name.trim();
    }
    // Location Priority: locationId ưu tiên hơn location string
    if (query?.locationId !== undefined && query?.locationId !== null) {
      // Ưu tiên locationId - chỉ thêm locationId, không thêm location string
      params.locationId = query.locationId;
    } else if (query?.location) {
      // Chỉ thêm location string nếu không có locationId
      params.location = query.location.trim();
    }
    if (query?.fromDate) {
      params.fromDate = query.fromDate;
    }
    if (query?.toDate) {
      params.toDate = query.toDate;
    }
    if (query?.minPrice !== undefined && query?.minPrice !== null) {
      params.minPrice = query.minPrice;
    }
    if (query?.maxPrice !== undefined && query?.maxPrice !== null) {
      params.maxPrice = query.maxPrice;
    }
    if (query?.beds !== undefined && query?.beds !== null) {
      params.beds = query.beds;
    }
    if (query?.bathrooms !== undefined && query?.bathrooms !== null) {
      params.bathrooms = query.bathrooms;
    }

    console.log("🔍 Searching condotels with params:", params);
    console.log("🔍 Full URL will be: /tenant/condotels?" + new URLSearchParams(params).toString());

    try {
      const response = await axiosClient.get<any>("/tenant/condotels", { params });
      console.log("✅ Search response:", response.data);
      console.log("✅ Response type:", Array.isArray(response.data) ? "Array" : typeof response.data);

      // Normalize response - handle both array, object with data property, and success wrapper
      let data: any[] = [];
      if (Array.isArray(response.data)) {
        data = response.data;
      } else if (response.data && typeof response.data === 'object') {
        // Handle { success: true, data: [...] } wrapper
        if ('data' in response.data) {
          if (Array.isArray(response.data.data)) {
            data = response.data.data;
          } else {
            data = [];
          }
        }
      }

      console.log("✅ Processed data count:", data.length);

      // Map response to CondotelDTO format
      const mapped = data.map((item: any) => {
        // Get thumbnailUrl: ưu tiên ThumbnailUrl từ API, nếu không có thì lấy ảnh đầu tiên từ CondotelImages
        let thumbnailUrl = item.ThumbnailUrl || item.thumbnailUrl;
        if (!thumbnailUrl) {
          const images = item.Images || item.images || item.CondotelImages || item.condotelImages || [];
          if (Array.isArray(images) && images.length > 0) {
            const firstImage = images[0];
            thumbnailUrl = firstImage.ImageUrl || firstImage.imageUrl || firstImage.Url || firstImage.url || firstImage;
          }
        }

        // Normalize activePrice
        const rawActivePrice = item.ActivePrice || item.activePrice;
        const normalizedActivePrice = rawActivePrice ? {
          priceId: rawActivePrice.PriceId || rawActivePrice.priceId,
          startDate: rawActivePrice.StartDate || rawActivePrice.startDate,
          endDate: rawActivePrice.EndDate || rawActivePrice.endDate,
          basePrice: rawActivePrice.BasePrice !== undefined ? rawActivePrice.BasePrice : rawActivePrice.basePrice,
          priceType: rawActivePrice.PriceType || rawActivePrice.priceType,
          description: rawActivePrice.Description || rawActivePrice.description,
        } : null;

        return {
        condotelId: item.CondotelId || item.condotelId,
        name: item.Name || item.name,
        pricePerNight: item.PricePerNight !== undefined ? item.PricePerNight : item.pricePerNight,
        beds: item.Beds !== undefined ? item.Beds : item.beds,
        bathrooms: item.Bathrooms !== undefined ? item.Bathrooms : item.bathrooms,
        status: item.Status || item.status,
          thumbnailUrl: thumbnailUrl,
        resortName: item.ResortName || item.resortName,
        hostName: item.HostName || item.hostName,
        activePromotion: normalizePromotion(item.ActivePromotion || item.activePromotion),
          activePrice: normalizedActivePrice,
        };
      });

      console.log("✅ Mapped results:", mapped.length, "condotels");
      return mapped;
    } catch (error: any) {
      console.error("❌ Search error:", error);
      console.error("❌ Error response:", error.response?.data);
      throw error;
    }
  },

  // GET /api/tenant/condotels - Lấy tất cả condotels (public, không cần đăng nhập)
  // Alias for search with no parameters
  getAll: async (): Promise<CondotelDTO[]> => {
    return condotelAPI.search();
  },

  // GET /api/tenant/condotels/{id} - Lấy chi tiết condotel (public, không cần đăng nhập)
  getById: async (id: number): Promise<CondotelDetailDTO> => {
    const response = await axiosClient.get<any>(`/tenant/condotels/${id}`);
    const data = response.data;

    console.log("🔍 Raw API response for condotel:", id, data);
    console.log("🔍 Raw Amenities:", data.Amenities || data.amenities);
    console.log("🔍 Raw Utilities:", data.Utilities || data.utilities);
    console.log("🔍 Raw Promotions:", data.Promotions || data.promotions);
    console.log("🔍 Raw ActivePromotion:", data.ActivePromotion || data.activePromotion);
    console.log("🔍 Raw ActivePrice:", data.ActivePrice || data.activePrice);
    console.log("🔍 Raw Resort:", data.Resort || data.resort);

    const rawAmenities = data.Amenities || data.amenities || [];
    const rawUtilities = data.Utilities || data.utilities || [];
    const rawPromotions = data.Promotions || data.promotions || [];
    const rawActivePromotion = data.ActivePromotion || data.activePromotion;
    const rawActivePrice = data.ActivePrice || data.activePrice;

    const normalizedAmenities = normalizeAmenities(rawAmenities);
    const normalizedUtilities = normalizeUtilities(rawUtilities);
    const normalizedPromotions = normalizePromotions(rawPromotions);
    const normalizedActivePromotion = normalizePromotion(rawActivePromotion);
    
    // Normalize activePrice
    const normalizedActivePrice = rawActivePrice ? {
      priceId: rawActivePrice.PriceId || rawActivePrice.priceId,
      startDate: rawActivePrice.StartDate || rawActivePrice.startDate,
      endDate: rawActivePrice.EndDate || rawActivePrice.endDate,
      basePrice: rawActivePrice.BasePrice !== undefined ? rawActivePrice.BasePrice : rawActivePrice.basePrice,
      priceType: rawActivePrice.PriceType || rawActivePrice.priceType,
      description: rawActivePrice.Description || rawActivePrice.description,
    } : null;

    console.log("✅ Normalized Amenities:", normalizedAmenities);
    console.log("✅ Normalized Utilities:", normalizedUtilities);
    console.log("✅ Normalized Promotions:", normalizedPromotions);
    console.log("✅ Normalized ActivePromotion:", normalizedActivePromotion);
    console.log("✅ Normalized ActivePrice:", normalizedActivePrice);

    // Normalize Resort object nếu có
    const resort = data.Resort || data.resort;
    const resortAddress = resort?.Address || resort?.address || data.ResortAddress || data.resortAddress || "";

    // Normalize response - map PascalCase to camelCase
    return {
      condotelId: data.CondotelId || data.condotelId,
      hostId: data.HostId || data.hostId,
      resortId: data.ResortId || data.resortId,
      name: data.Name || data.name,
      description: data.Description || data.description,
      pricePerNight: data.PricePerNight !== undefined ? data.PricePerNight : data.pricePerNight,
      beds: data.Beds !== undefined ? data.Beds : data.beds,
      bathrooms: data.Bathrooms !== undefined ? data.Bathrooms : data.bathrooms,
      status: data.Status || data.status,
      hostName: data.HostName || data.hostName,
      hostImageUrl: data.HostImageUrl || data.hostImageUrl,
      resortName: resort?.Name || resort?.name || data.ResortName || data.resortName,
      resortAddress: resortAddress,
      images: data.Images || data.images || [],
      prices: data.Prices || data.prices || [],
      details: data.Details || data.details || [],
      amenities: normalizedAmenities,
      utilities: normalizedUtilities,
      promotions: normalizedPromotions,
      activePromotion: normalizedActivePromotion,
      activePrice: normalizedActivePrice,
    };
  },

  // GET /api/tenant/condotels?location=... - Tìm kiếm condotel theo location (sử dụng endpoint mới)
  getCondotelsByLocation: async (locationName?: string): Promise<CondotelDTO[]> => {
    return condotelAPI.search({ location: locationName });
  },

  // GET /api/tenant/condotels?locationId=... - Tìm kiếm condotel theo location ID (public, không cần đăng nhập)
  getCondotelsByLocationId: async (locationId: number): Promise<CondotelDTO[]> => {
    return condotelAPI.search({ locationId });
  },

  // GET /api/tenant/condotels/{id}/amenities - Lấy danh sách amenities của condotel (public)
  getAmenitiesByCondotelId: async (id: number): Promise<AmenityDTO[]> => {
    const response = await axiosClient.get<any>(`/tenant/condotels/${id}/amenities`);
    const data = response.data;

    // Normalize response - handle both array and object with data property
    const amenities = Array.isArray(data) ? data : (data.data || []);

    // Normalize amenities - handle both PascalCase and camelCase
    return amenities.map((a: any) => ({
      amenityId: a.AmenityId || a.amenityId || a.Id || a.id,
      name: a.Name || a.name,
    }));
  },

  // GET /api/tenant/condotels/{id}/utilities - Lấy danh sách utilities của condotel (public)
  getUtilitiesByCondotelId: async (id: number): Promise<UtilityDTO[]> => {
    const response = await axiosClient.get<any>(`/tenant/condotels/${id}/utilities`);
    const data = response.data;

    // Normalize response - handle both array and object with data property
    const utilities = Array.isArray(data) ? data : (data.data || []);

    // Normalize utilities - handle both PascalCase and camelCase
    return utilities.map((u: any) => ({
      utilityId: u.UtilityId || u.utilityId || u.Id || u.id,
      name: u.Name || u.name,
    }));
  },

  // GET /api/tenant/condotels/{id}/amenities-utilities - Lấy cả amenities và utilities trong một request (public)
  getAmenitiesAndUtilitiesByCondotelId: async (id: number): Promise<{ amenities: AmenityDTO[]; utilities: UtilityDTO[] }> => {
    const response = await axiosClient.get<any>(`/tenant/condotels/${id}/amenities-utilities`);
    const data = response.data;

    // Normalize response structure
    const rawAmenities = data.Amenities || data.amenities || [];
    const rawUtilities = data.Utilities || data.utilities || [];

    // Normalize amenities
    const amenities = Array.isArray(rawAmenities) ? rawAmenities : [];
    const normalizedAmenities = amenities.map((a: any) => ({
      amenityId: a.AmenityId || a.amenityId || a.Id || a.id,
      name: a.Name || a.name,
    }));

    // Normalize utilities
    const utilities = Array.isArray(rawUtilities) ? rawUtilities : [];
    const normalizedUtilities = utilities.map((u: any) => ({
      utilityId: u.UtilityId || u.utilityId || u.Id || u.id,
      name: u.Name || u.name,
    }));

    return {
      amenities: normalizedAmenities,
      utilities: normalizedUtilities,
    };
  },

  // GET /api/tenant/condotels?location=... - Tìm kiếm condotel theo location (public, AllowAnonymous)
  // Alias for getCondotelsByLocation - sử dụng endpoint mới
  getCondotelsByLocationPublic: async (locationName?: string): Promise<CondotelDTO[]> => {
    return condotelAPI.search({ location: locationName });
  },

  // GET /api/host/condotel - Lấy tất cả condotels của host (cần đăng nhập)
  getAllForHost: async (): Promise<CondotelDTO[]> => {
    const response = await axiosClient.get<any>("/host/condotel");
    const data = Array.isArray(response.data) ? response.data : (response.data.data || []);

    // Helper function to normalize PromotionDTO
    const normalizePromotion = (promo: any): PromotionDTO | null => {
      if (!promo) return null;
      return {
        promotionId: promo.PromotionId || promo.promotionId || 0,
        condotelId: promo.CondotelId || promo.condotelId || 0,
        condotelName: promo.CondotelName || promo.condotelName,
        name: promo.Name || promo.name || "",
        description: promo.Description || promo.description,
        discountPercentage: promo.DiscountPercentage !== undefined ? promo.DiscountPercentage : promo.discountPercentage,
        discountAmount: promo.DiscountAmount !== undefined ? promo.DiscountAmount : promo.discountAmount,
        startDate: promo.StartDate || promo.startDate || "",
        endDate: promo.EndDate || promo.endDate || "",
        isActive: promo.IsActive !== undefined ? promo.IsActive : (promo.isActive !== undefined ? promo.isActive : false),
        status: promo.Status || promo.status,
        createdAt: promo.CreatedAt || promo.createdAt,
        updatedAt: promo.UpdatedAt || promo.updatedAt,
      };
    };

    return data.map((item: any) => {
      // Normalize activePrice
      const rawActivePrice = item.ActivePrice || item.activePrice;
      const normalizedActivePrice = rawActivePrice ? {
        priceId: rawActivePrice.PriceId || rawActivePrice.priceId,
        startDate: rawActivePrice.StartDate || rawActivePrice.startDate,
        endDate: rawActivePrice.EndDate || rawActivePrice.endDate,
        basePrice: rawActivePrice.BasePrice !== undefined ? rawActivePrice.BasePrice : rawActivePrice.basePrice,
        priceType: rawActivePrice.PriceType || rawActivePrice.priceType,
        description: rawActivePrice.Description || rawActivePrice.description,
      } : null;

      return {
      condotelId: item.CondotelId || item.condotelId,
      name: item.Name || item.name,
      pricePerNight: item.PricePerNight !== undefined ? item.PricePerNight : item.pricePerNight,
      beds: item.Beds !== undefined ? item.Beds : item.beds,
      bathrooms: item.Bathrooms !== undefined ? item.Bathrooms : item.bathrooms,
      status: item.Status || item.status,
      thumbnailUrl: item.ThumbnailUrl || item.thumbnailUrl,
      resortName: item.ResortName || item.resortName,
      hostName: item.HostName || item.hostName,
      activePromotion: normalizePromotion(item.ActivePromotion || item.activePromotion),
        activePrice: normalizedActivePrice,
      };
    });
  },

  // GET /api/host/condotel/{id} - Lấy condotel theo ID của host (cần đăng nhập)
  getByIdForHost: async (id: number): Promise<CondotelDetailDTO> => {
    const response = await axiosClient.get<any>(`/host/condotel/${id}`);
    const data = response.data;

    const rawAmenities = data.Amenities || data.amenities || [];
    const rawUtilities = data.Utilities || data.utilities || [];
    const rawPromotions = data.Promotions || data.promotions || [];
    const rawActivePromotion = data.ActivePromotion || data.activePromotion;
    const rawActivePrice = data.ActivePrice || data.activePrice;

    // Normalize activePrice
    const normalizedActivePrice = rawActivePrice ? {
      priceId: rawActivePrice.PriceId || rawActivePrice.priceId,
      startDate: rawActivePrice.StartDate || rawActivePrice.startDate,
      endDate: rawActivePrice.EndDate || rawActivePrice.endDate,
      basePrice: rawActivePrice.BasePrice !== undefined ? rawActivePrice.BasePrice : rawActivePrice.basePrice,
      priceType: rawActivePrice.PriceType || rawActivePrice.priceType,
      description: rawActivePrice.Description || rawActivePrice.description,
    } : null;

    return {
      condotelId: data.CondotelId || data.condotelId,
      hostId: data.HostId || data.hostId,
      resortId: data.ResortId || data.resortId,
      name: data.Name || data.name,
      description: data.Description || data.description,
      pricePerNight: data.PricePerNight !== undefined ? data.PricePerNight : data.pricePerNight,
      beds: data.Beds !== undefined ? data.Beds : data.beds,
      bathrooms: data.Bathrooms !== undefined ? data.Bathrooms : data.bathrooms,
      status: data.Status || data.status,
      hostName: data.HostName || data.hostName,
      hostImageUrl: data.HostImageUrl || data.hostImageUrl,
      images: data.Images || data.images || [],
      prices: data.Prices || data.prices || [],
      details: data.Details || data.details || [],
      amenities: normalizeAmenities(rawAmenities),
      utilities: normalizeUtilities(rawUtilities),
      promotions: normalizePromotions(rawPromotions),
      activePromotion: normalizePromotion(rawActivePromotion),
      activePrice: normalizedActivePrice,
    };
  },

  // POST /api/host/condotel - Tạo condotel mới
  // Lưu ý: HostId sẽ được backend tự động lấy từ JWT token (JsonIgnore trong DTO)
  create: async (condotel: CreateCondotelDTO): Promise<CondotelDetailDTO> => {
    // Map camelCase sang PascalCase để khớp với backend C# DTO
    const requestData: any = {
      Name: condotel.name,
      PricePerNight: condotel.pricePerNight,
      Beds: condotel.beds,
      Bathrooms: condotel.bathrooms,
      Status: condotel.status,
    };

    // Optional fields
    if (condotel.resortId !== undefined && condotel.resortId !== null) {
      requestData.ResortId = condotel.resortId;
    }
    if (condotel.description) {
      requestData.Description = condotel.description;
    }

    // Images - map sang PascalCase (không gửi ImageId khi create)
    if (condotel.images && condotel.images.length > 0) {
      requestData.Images = condotel.images.map(img => ({
        ImageUrl: img.imageUrl,
        Caption: img.caption || null,
      }));
    }

    // Prices - map sang PascalCase (không gửi PriceId khi create, nhưng Description là required)
    if (condotel.prices && condotel.prices.length > 0) {
      requestData.Prices = condotel.prices.map(p => ({
        StartDate: p.startDate,
        EndDate: p.endDate,
        BasePrice: p.basePrice,
        PriceType: p.priceType,
        Description: p.description || "", // Required trong backend
      }));
    }

    // Details - map sang PascalCase
    if (condotel.details && condotel.details.length > 0) {
      requestData.Details = condotel.details.map(d => {
        const detail: any = {};
        if (d.buildingName) detail.BuildingName = d.buildingName;
        if (d.roomNumber) detail.RoomNumber = d.roomNumber;
        if (d.beds !== undefined) detail.Beds = d.beds;
        if (d.bathrooms !== undefined) detail.Bathrooms = d.bathrooms;
        if (d.safetyFeatures) detail.SafetyFeatures = d.safetyFeatures;
        if (d.hygieneStandards) detail.HygieneStandards = d.hygieneStandards;
        return detail;
      });
    }

    // AmenityIds và UtilityIds
    if (condotel.amenityIds && condotel.amenityIds.length > 0) {
      requestData.AmenityIds = condotel.amenityIds;
    }
    if (condotel.utilityIds && condotel.utilityIds.length > 0) {
      requestData.UtilityIds = condotel.utilityIds;
    }

    console.log("📤 Creating condotel with data:", JSON.stringify(requestData, null, 2));

    const response = await axiosClient.post<any>("/host/condotel", requestData);
    const rawData = response.data;

    // Handle response wrapper: { success: true, message: "...", data: {...} }
    const data = rawData.success && rawData.data ? rawData.data : rawData;

    console.log("✅ Condotel created successfully:", data);

    // Normalize response to CondotelDetailDTO
    const rawAmenities = data.Amenities || data.amenities || [];
    const rawUtilities = data.Utilities || data.utilities || [];
    const rawPromotions = data.Promotions || data.promotions || [];
    const rawActivePromotion = data.ActivePromotion || data.activePromotion;

    return {
      condotelId: data.CondotelId || data.condotelId,
      hostId: data.HostId || data.hostId,
      resortId: data.ResortId || data.resortId,
      name: data.Name || data.name,
      description: data.Description || data.description,
      pricePerNight: data.PricePerNight !== undefined ? data.PricePerNight : data.pricePerNight,
      beds: data.Beds !== undefined ? data.Beds : data.beds,
      bathrooms: data.Bathrooms !== undefined ? data.Bathrooms : data.bathrooms,
      status: data.Status || data.status,
      hostName: data.HostName || data.hostName,
      hostImageUrl: data.HostImageUrl || data.hostImageUrl,
      images: data.Images || data.images || [],
      prices: data.Prices || data.prices || [],
      details: data.Details || data.details || [],
      amenities: normalizeAmenities(rawAmenities),
      utilities: normalizeUtilities(rawUtilities),
      promotions: normalizePromotions(rawPromotions),
      activePromotion: normalizePromotion(rawActivePromotion),
    };
  },

  // PUT /api/condotel/{id} - Cập nhật condotel
  update: async (
    id: number,
    condotel: CondotelDetailDTO
  ): Promise<CondotelDetailDTO> => {
    const response = await axiosClient.put<CondotelDetailDTO>(`/host/condotel/${id}`, condotel);
    return response.data;
  },

  // DELETE /api/condotel/{id} - "Xóa" condotel bằng cách chuyển status sang "Inactive"
  delete: async (id: number): Promise<CondotelDetailDTO> => {
    // Lấy thông tin condotel hiện tại
    const currentCondotel = await axiosClient.get<CondotelDetailDTO>(`/host/condotel/${id}`).then(res => res.data);

    // Cập nhật status thành "Inactive" thay vì xóa thật sự
    const updatedCondotel: CondotelDetailDTO = {
      ...currentCondotel,
      status: "Inactive",
    };

    // Gọi API update để thay đổi status
    const response = await axiosClient.put<CondotelDetailDTO>(`/host/condotel/${id}`, updatedCondotel);
    return response.data;
  },

  // Promotion APIs - Sử dụng endpoints từ PromotionController
  // GET /api/promotion - Lấy tất cả promotions
  // Promotion APIs - ĐÃ SỬA ĐÚNG THEO BACKEND CỦA BẠN
  // LẤY TẤT CẢ KHUYẾN MÃI CỦA HOST HIỆN TẠI (dùng cho trang Promotions)
  getPromotions: async (): Promise<PromotionDTO[]> => {
    const response = await axiosClient.get<PromotionDTO[]>("/host/promotions");
    return response.data;
  },

  // LẤY KHUYẾN MÃI THEO TỪNG CONDOTEL (nếu vẫn dùng được ở những chỗ khác)
  getPromotionsByCondotel: async (condotelId: number): Promise<PromotionDTO[]> => {
    const response = await axiosClient.get<PromotionDTO[]>(`/promotions/condotel/${condotelId}`);
    return response.data;
  },

  // Các hàm CRUD khác (giữ nguyên route đúng)
  createPromotion: async (promotion: CreatePromotionDTO): Promise<PromotionDTO> => {
    const res = await axiosClient.post<PromotionDTO>("/host/promotion", promotion);
    return res.data;
  },

  updatePromotion: async (promotionId: number, promotion: UpdatePromotionDTO): Promise<any> => {
    const res = await axiosClient.put(`/host/promotion/${promotionId}`, promotion);
    return res.data;
  },

  deletePromotion: async (promotionId: number): Promise<void> => {
    await axiosClient.delete(`/host/promotion/${promotionId}`);
  },
};

export default condotelAPI;
