// src/api/adminPackageAPI.ts
import axiosClient from "./axiosClient";

export interface HostPackageItem {
    hostPackageId: number;
    hostName: string;
    email: string;
    phone: string;
    packageName: string;
    orderCode: string;
    amount: number;
    status: string;
    startDate: string;
    endDate: string;
    canActivate: boolean;
}

export interface ActivateResponse {
    message: string;
    hostName?: string;
}

// ============ THÊM CÁC TRƯỜNG FEATURES MỚI ============
export interface CatalogPackage {
    packageId: number;
    name: string;
    price: number;
    durationDays: number | null;
    description: string | null;
    isActive: boolean;

    // ======== THÊM CÁC TRƯỜNG FEATURES ========
    maxListingCount: number;           // Số condotel tối đa
    canUseFeaturedListing: boolean;    // Được đăng tin nổi bật không
    maxBlogRequestsPerMonth: number;   // Số blog tối đa mỗi tháng
    isVerifiedBadgeEnabled: boolean;   // Có badge xác minh không
    displayColorTheme: string;         // Theme màu hiển thị
    priorityLevel: number;             // Mức độ ưu tiên
}

export interface CreatePackageDto {
    name: string;
    price: number;
    durationDays: number;
    description: string;
    isActive: boolean;

    // ======== THÊM CÁC TRƯỜNG FEATURES ========
    maxListingCount: number;
    canUseFeaturedListing: boolean;
    maxBlogRequestsPerMonth: number;
    isVerifiedBadgeEnabled: boolean;
    displayColorTheme: string;
    priorityLevel: number;
}

export interface UpdatePackageDto {
    name: string;
    price: number;
    durationDays: number;
    description: string;
    isActive: boolean;

    // ======== THÊM CÁC TRƯỜNG FEATURES ========
    maxListingCount: number;
    canUseFeaturedListing: boolean;
    maxBlogRequestsPerMonth: number;
    isVerifiedBadgeEnabled: boolean;
    displayColorTheme: string;
    priorityLevel: number;
}

export const adminPackageAPI = {
    // ============ PHẦN CŨ ============
    getAll: async (search?: string): Promise<HostPackageItem[]> => {
        const response = await axiosClient.get<{ data: HostPackageItem[] } | HostPackageItem[]>(
            "/admin/packages",
            { params: search ? { search } : {} }
        );
        const result = (response as any).data || response;
        return Array.isArray(result) ? result : result.data || [];
    },

    activate: async (id: number): Promise<ActivateResponse> => {
        const response = await axiosClient.post<ActivateResponse>(
            `/admin/packages/${id}/activate`
        );
        return response.data;
    },

    // ============ THÊM PHẦN CATALOG ============
    // Helper function to normalize CatalogPackage (handle missing columns gracefully)
    normalizeCatalogPackage: (item: any): CatalogPackage => {
        return {
            packageId: item.PackageId || item.packageId || 0,
            name: item.Name || item.name || "",
            price: item.Price !== undefined ? item.Price : item.price,
            durationDays: item.DurationDays !== undefined ? item.DurationDays : (item.durationDays !== undefined ? item.durationDays : null),
            description: item.Description || item.description || null,
            isActive: item.IsActive !== undefined ? item.IsActive : (item.isActive !== undefined ? item.isActive : false),
            // Handle missing columns with default values
            maxListingCount: item.MaxListingCount !== undefined ? item.MaxListingCount : (item.maxListingCount !== undefined ? item.maxListingCount : 0),
            canUseFeaturedListing: item.CanUseFeaturedListing !== undefined ? item.CanUseFeaturedListing : (item.canUseFeaturedListing !== undefined ? item.canUseFeaturedListing : false),
            maxBlogRequestsPerMonth: item.MaxBlogRequestsPerMonth !== undefined ? item.MaxBlogRequestsPerMonth : (item.maxBlogRequestsPerMonth !== undefined ? item.maxBlogRequestsPerMonth : 0),
            isVerifiedBadgeEnabled: item.IsVerifiedBadgeEnabled !== undefined ? item.IsVerifiedBadgeEnabled : (item.isVerifiedBadgeEnabled !== undefined ? item.isVerifiedBadgeEnabled : false),
            displayColorTheme: item.DisplayColorTheme || item.displayColorTheme || "default",
            priorityLevel: item.PriorityLevel !== undefined ? item.PriorityLevel : (item.priorityLevel !== undefined ? item.priorityLevel : 0),
        };
    },

    getCatalog: async (): Promise<CatalogPackage[]> => {
        try {
            const response = await axiosClient.get<any>(
                "/admin/packages/catalog"
            );
            const data = Array.isArray(response.data) ? response.data : (response.data?.data || []);
            if (!Array.isArray(data)) {
                console.warn("AdminPackageAPI.getCatalog: Expected an array, but received:", response.data);
                return [];
            }
            return data.map(adminPackageAPI.normalizeCatalogPackage);
        } catch (error: any) {
            // Check if error is related to missing database columns
            const errorMessage = error.response?.data?.message || error.message || "";
            if (errorMessage.includes("Invalid column name") || 
                errorMessage.includes("CanUseFeaturedListing") || 
                errorMessage.includes("DisplayColorTheme") ||
                errorMessage.includes("IsVerifiedBadgeEnabled") ||
                errorMessage.includes("MaxBlogRequestsPerMonth") ||
                errorMessage.includes("MaxListingCount") ||
                errorMessage.includes("PriorityLevel")) {
                console.warn("⚠️ Backend database missing columns. Returning empty array. Backend needs to add columns or fix query.");
                console.warn("Error details:", errorMessage);
                // Return empty array to prevent crashes - backend needs to fix this
                return [];
            }
            console.error("Error fetching catalog packages:", error);
            // Return empty array on error to prevent crashes
            return [];
        }
    },

    createCatalog: async (data: CreatePackageDto): Promise<any> => {
        const response = await axiosClient.post(
            "/admin/packages/catalog",
            data
        );
        return response.data;
    },

    updateCatalog: async (id: number, data: UpdatePackageDto): Promise<any> => {
        const response = await axiosClient.put(
            `/admin/packages/catalog/${id}`,
            data
        );
        return response.data;
    },

    deleteCatalog: async (id: number): Promise<any> => {
        const response = await axiosClient.delete(
            `/admin/packages/catalog/${id}`
        );
        return response.data;
    },
};

export default adminPackageAPI;