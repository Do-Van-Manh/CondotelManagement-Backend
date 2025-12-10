import axiosClient from "./axiosClient";

export interface PackageDto {
    packageId: number;
    name: string;
    price: number;
    duration: string;
    description: string;
    maxListings: number;
    canUseFeaturedListing: boolean;
}

export interface HostPackageDetailsDto {
    packageName: string;
    status: string;
    startDate: string;
    endDate: string;
    maxListings: number;
    currentListings: number;
    canUseFeaturedListing: boolean;
    message?: string;
    paymentUrl?: string;
    orderCode?: number;
    amount?: number;
    isVerifiedBadgeEnabled: boolean;
}

export interface CancelPackageResponseDto {
    success: boolean;
    message: string;
    refundAmount?: number;
    refundUrl?: string;
}

export interface PurchasePackageRequestDto {
    packageId: number;
}

// Helper function to normalize PackageDto (handle missing columns gracefully)
const normalizePackageDto = (item: any): PackageDto => {
    return {
        packageId: item.PackageId || item.packageId || 0,
        name: item.Name || item.name || "",
        price: item.Price !== undefined ? item.Price : item.price,
        duration: item.Duration || item.duration || "",
        description: item.Description || item.description || "",
        maxListings: item.MaxListings !== undefined ? item.MaxListings : (item.maxListings !== undefined ? item.maxListings : 0),
        canUseFeaturedListing: item.CanUseFeaturedListing !== undefined ? item.CanUseFeaturedListing : (item.canUseFeaturedListing !== undefined ? item.canUseFeaturedListing : false),
    };
};

// Helper function to normalize HostPackageDetailsDto (handle missing columns gracefully)
const normalizeHostPackageDetails = (item: any): HostPackageDetailsDto => {
    return {
        packageName: item.PackageName || item.packageName || "",
        status: item.Status || item.status || "",
        startDate: item.StartDate || item.startDate || "",
        endDate: item.EndDate || item.endDate || "",
        maxListings: item.MaxListings !== undefined ? item.MaxListings : (item.maxListings !== undefined ? item.maxListings : 0),
        currentListings: item.CurrentListings !== undefined ? item.CurrentListings : (item.currentListings !== undefined ? item.currentListings : 0),
        canUseFeaturedListing: item.CanUseFeaturedListing !== undefined ? item.CanUseFeaturedListing : (item.canUseFeaturedListing !== undefined ? item.canUseFeaturedListing : false),
        message: item.Message || item.message,
        paymentUrl: item.PaymentUrl || item.paymentUrl,
        orderCode: item.OrderCode !== undefined ? item.OrderCode : item.orderCode,
        amount: item.Amount !== undefined ? item.Amount : item.amount,
        isVerifiedBadgeEnabled: item.IsVerifiedBadgeEnabled !== undefined ? item.IsVerifiedBadgeEnabled : (item.isVerifiedBadgeEnabled !== undefined ? item.isVerifiedBadgeEnabled : false),
    };
};

export const packageAPI = {
    getAllPackages: async (): Promise<PackageDto[]> => {
        try {
            const response = await axiosClient.get<any>("/Package");
            const data = Array.isArray(response.data) ? response.data : (response.data?.data || []);
            if (!Array.isArray(data)) {
                console.warn("PackageAPI.getAllPackages: Expected an array, but received:", response.data);
                return [];
            }
            return data.map(normalizePackageDto);
        } catch (error: any) {
            // Check if error is related to missing database columns
            const errorMessage = error.response?.data?.message || error.message || "";
            if (errorMessage.includes("Invalid column name") || errorMessage.includes("CanUseFeaturedListing") || errorMessage.includes("MaxListingCount")) {
                console.warn("⚠️ Backend database missing columns. Returning empty array. Backend needs to add columns or fix query.");
                console.warn("Error details:", errorMessage);
                // Return empty array to prevent crashes - backend needs to fix this
                return [];
            }
            console.error("Error fetching packages:", error);
            // Return empty array on error to prevent crashes
            return [];
        }
    },

    getMyPackage: async (): Promise<HostPackageDetailsDto | null> => {
        try {
            const response = await axiosClient.get<any>("/host/packages/my-package");
            const normalized = normalizeHostPackageDetails(response.data);
            return normalized.packageName ? normalized : null;
        } catch (error: any) {
            // Check if error is related to missing database columns
            const errorMessage = error.response?.data?.message || error.message || "";
            if (errorMessage.includes("Invalid column name") || errorMessage.includes("IsVerifiedBadgeEnabled") || errorMessage.includes("CanUseFeaturedListing")) {
                console.warn("⚠️ Backend database missing columns. Returning null. Backend needs to add columns or fix query.");
                console.warn("Error details:", errorMessage);
                // Return null to prevent crashes - backend needs to fix this
                return null;
            }
            // Nếu 400 hoặc 404 → người dùng chưa có package → bình thường
            if (error.response?.status === 400 || error.response?.status === 404) {
                return null;
            }
            console.error("Error fetching my package:", error);
            return null;
        }
    },

    // TRẢ VỀ THÔNG TIN ĐỂ GỌI PAYOS
    purchasePackage: async (packageId: number): Promise<HostPackageDetailsDto> => {
        const response = await axiosClient.post<HostPackageDetailsDto>(
            "/host/packages/purchase",
            { packageId }
        );
        return response.data;
    },

    // POST /api/host/packages/cancel - Hủy package và hoàn tiền
    cancelPackage: async (): Promise<CancelPackageResponseDto> => {
        const response = await axiosClient.post<CancelPackageResponseDto>("/host/packages/cancel");
        return response.data;
    },
};